using System;
using System.Collections;
using System.Collections.Generic;
using BallGame.Audio;
using BallGame.Core;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// One level being played: charge -> shot -> infection -> ball advances, until the door or a loss.
    /// Rules live in <see cref="ObstacleField"/> and <see cref="GameplayConfig"/>; this class drives timing and the views.
    /// </summary>
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] private GameplayConfig config;
        [SerializeField] private PlayerBall ball;
        [SerializeField] private ShotProjectile shot;
        [SerializeField] private ObstacleSpawner obstacles;
        [SerializeField] private Door door;
        [SerializeField] private TrackView track;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private InputReader input;
        [SerializeField] private BurstFx explosionFx;
        [Tooltip("White dust puffed up where the ball lands after each hop.")]
        [SerializeField] private BurstFx dustFx;
        [SerializeField] private Color dustColor = new(1f, 1f, 1f, 0.85f);
        [SerializeField] private float resultDelay = 0.4f;

        private readonly ObstacleField field = new();
        private readonly List<InfectionNode> infection = new();
        private readonly List<int> swept = new();

        private LevelLayout layout;
        private Vector2 position;
        private float remaining;
        private float startVolume;
        private float ballVolume;
        private float shotVolume;
        private float chargeTime;
        private float nextTickTime;
        private float chargeStartRadius;
        private Color skinColor = Color.white;
        private App app;

        public event Action<SessionState> StateChanged;
        public event Action Won;
        public event Action<LoseReason> Lost;

        public SessionState State { get; private set; }
        public int Level => layout?.Level ?? 0;
        public bool HasShot { get; private set; }

        /// <summary>Ball size left above the critical size: 1 at start, 0 at the critical size.</summary>
        public float SizeLeft
        {
            get
            {
                var min = BallMath.Volume(config.minBallRadius);
                return startVolume > min ? Mathf.Clamp01((ballVolume - min) / (startVolume - min)) : 0f;
            }
        }

        private float BallRadius => BallMath.Radius(ballVolume);
        private Vector2 Direction => layout.Direction;

        private void Awake()
        {
            app = App.Instance;
            door.Opened += OnDoorOpened;
            obstacles.Exploded += OnObstacleExploded;
        }

        private void OnDestroy()
        {
            door.Opened -= OnDoorOpened;
            obstacles.Exploded -= OnObstacleExploded;
        }

        public void Begin(LevelLayout levelLayout)
        {
            StopAllCoroutines();
            layout = levelLayout;

            field.Build(layout.Obstacles, layout.ObstacleRadius, config.linkDistance);
            obstacles.Spawn(layout);

            startVolume = ballVolume = BallMath.Volume(layout.BallRadius);
            shotVolume = 0f;
            position = layout.Start;
            remaining = layout.PathLength;
            HasShot = false;

            ball.ResetBall(BallMath.World(position), layout.BallRadius);
            shot.Hide();
            door.Setup(BallMath.World(layout.Door), Direction, ball.transform, config.doorOpenDistance);
            UpdateTrack();
            cameraRig.Follow(ball.transform, BallMath.World(Direction));

            SetState(SessionState.Idle);
        }

        public void SetSkin(Material material, Color mainColor)
        {
            skinColor = mainColor;
            ball.SetSkin(material);
        }

        private void Update()
        {
            switch (State)
            {
                case SessionState.Idle when input.PressStarted:
                    BeginCharge();
                    break;
                case SessionState.Charging when input.IsHeld:
                    Charge(Time.deltaTime);
                    break;
                case SessionState.Charging:
                    StartCoroutine(ResolveShot());
                    break;
            }
        }

        #region Charge

        private void BeginCharge()
        {
            chargeTime = 0f;
            nextTickTime = 0f;
            // The corridor the shot clears is the one of the ball before charging (the generator's bot uses the same).
            chargeStartRadius = BallRadius;
            shot.Show(ball.Skin);
            SetState(SessionState.Charging);
            TransferToShot(config.minShotVolume);
        }

        private void Charge(float deltaTime)
        {
            chargeTime += deltaTime;
            var rate = startVolume * (config.chargeRate + config.chargeAcceleration * chargeTime);
            TransferToShot(rate * deltaTime);

            if (State == SessionState.Charging && chargeTime >= nextTickTime)
            {
                nextTickTime = chargeTime + config.chargeTickInterval;
                app.Audio.Play(SoundId.ChargeTick, 1f + (1f - SizeLeft) * 0.8f);
                app.Haptics.Light();
            }
        }

        private void TransferToShot(float volume)
        {
            ballVolume -= volume;
            shotVolume += volume;

            if (BallRadius < config.minBallRadius)
            {
                LoseOvercharged();
                return;
            }

            ball.SetRadius(BallRadius);
            PlaceShotInFront();
            UpdateTrack();
        }

        private void PlaceShotInFront()
        {
            var radius = BallMath.Radius(shotVolume);
            var front = position + Direction * BallMath.ShotStartDistance(BallRadius, radius);
            shot.Place(BallMath.World(front), radius);
        }

        #endregion

        #region Shot

        private IEnumerator ResolveShot()
        {
            SetState(SessionState.Resolving);
            HasShot = true;

            var volume = shotVolume;
            shotVolume = 0f;
            app.Audio.Play(SoundId.Shot);

            // The shot always flies straight along the path and bursts at the first obstacle in its way.
            var target = field.FindShotTarget(position, Direction, remaining, chargeStartRadius, config.grazeTolerance,
                shot.Radius, out var distance);
            var explosion = BallMath.ExplosionPoint(position, Direction, distance, BallRadius, shot.Radius);

            yield return shot.FlyTo(BallMath.World(explosion), config.shotSpeed);
            shot.Hide();

            if (target >= 0)
                yield return Explode(target, explosion, volume);

            yield return AdvanceOrFinish();
        }

        private IEnumerator Explode(int target, Vector2 explosion, float volume)
        {
            explosionFx.Burst(BallMath.World(explosion, shot.Radius), skinColor, 0.8f);
            app.Audio.Play(SoundId.Hit);
            app.Haptics.Medium();

            field.CollectInfection(target, explosion, config.InfectionRadius(volume), config.linkDistance, infection);

            var maxDepth = 0;
            foreach (var node in infection)
            {
                field.Kill(node.Index);
                obstacles.Infect(node.Index, node.Depth * config.infectionStepDelay, config.explodeDelay);
                maxDepth = Mathf.Max(maxDepth, node.Depth);
            }

            yield return new WaitForSeconds(maxDepth * config.infectionStepDelay + config.explodeDelay + 0.1f);
        }

        #endregion

        #region Movement and result

        private IEnumerator AdvanceOrFinish()
        {
            var blocker = field.FindFirstBlocker(position, Direction, remaining, BallRadius, config.grazeTolerance, out var contact);

            if (blocker < 0)
            {
                yield return Hop(remaining + config.finishBeyondDoor);
                yield return Finish(SessionState.Won);
                yield break;
            }

            var advance = contact - config.stopGap;
            if (advance >= config.minAdvance)
                yield return Hop(advance);

            if (!CanShoot())
            {
                yield return Finish(SessionState.Lost, LoseReason.OutOfSize);
                yield break;
            }

            SetState(SessionState.Idle);
        }

        private bool CanShoot()
        {
            return ballVolume - config.minShotVolume >= BallMath.Volume(config.minBallRadius);
        }

        private IEnumerator Hop(float distance)
        {
            SetState(SessionState.Moving);

            var hops = Mathf.Max(1, Mathf.CeilToInt(distance / config.hopLength));
            var step = distance / hops;

            for (int i = 0; i < hops; i++)
            {
                var from = position;
                var to = position + Direction * step;
                KnockSwept(from, to);

                for (var t = 0f; t < config.hopDuration; t += Time.deltaTime)
                {
                    var k = t / config.hopDuration;
                    MoveBall(Vector2.Lerp(from, to, k), Mathf.Sin(k * Mathf.PI) * config.hopHeight * BallRadius);
                    yield return null;
                }

                MoveBall(to, 0f);
                remaining -= step;
                dustFx.Burst(BallMath.World(position, 0.1f), dustColor, BallRadius);
                app.Audio.Play(SoundId.Hop);
            }
        }

        private void MoveBall(Vector2 target, float hopHeight)
        {
            ball.Roll(BallMath.World(target - position));
            position = target;
            ball.SetGroundPosition(BallMath.World(position));
            ball.SetHopHeight(hopHeight);
            UpdateTrack();
        }

        // Obstacles that only graze the corridor do not stop the ball: it knocks them away on the way.
        private void KnockSwept(Vector2 from, Vector2 to)
        {
            field.CollectSwept(from, to, BallRadius, swept);
            foreach (var index in swept)
            {
                field.Kill(index);
                obstacles.Knock(index, config.hopDuration * 0.5f);
            }
        }

        private void LoseOvercharged()
        {
            StopAllCoroutines();
            shot.Hide();
            ball.Hide();
            explosionFx.Burst(ball.transform.position + Vector3.up * config.minBallRadius, skinColor, 1.5f);
            StartCoroutine(Finish(SessionState.Lost, LoseReason.Overcharged));
        }

        private IEnumerator Finish(SessionState result, LoseReason reason = default)
        {
            SetState(result);

            var won = result == SessionState.Won;
            app.Audio.Play(won ? SoundId.Win : SoundId.Lose);
            app.Haptics.Heavy();

            yield return new WaitForSeconds(resultDelay);

            if (won)
                Won?.Invoke();
            else
                Lost?.Invoke(reason);
        }

        #endregion

        private void UpdateTrack()
        {
            var from = position - Direction * BallRadius;
            var to = layout.Door + Direction * config.finishBeyondDoor;
            track.SetSegment(BallMath.World(from), BallMath.World(to), BallRadius * 2f);
        }

        private void OnDoorOpened()
        {
            app.Audio.Play(SoundId.DoorOpen);
            app.Haptics.Medium();
        }

        private void OnObstacleExploded() => app.Audio.Play(SoundId.Explode);

        private void SetState(SessionState state)
        {
            if (State == state)
                return;

            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
