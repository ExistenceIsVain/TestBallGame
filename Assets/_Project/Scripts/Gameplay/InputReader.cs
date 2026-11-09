using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BallGame.Gameplay
{
    /// <summary>Hold-to-charge input: touch, mouse or Space. Presses that start over UI are ignored.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class InputReader : MonoBehaviour
    {
        private readonly List<RaycastResult> uiHits = new();
        private InputAction press;
        private PointerEventData pointerData;

        public bool PressStarted { get; private set; }
        public bool IsHeld { get; private set; }

        private void Awake()
        {
            press = new InputAction("Press", InputActionType.Button);
            press.AddBinding("<Pointer>/press");
            press.AddBinding("<Keyboard>/space");
        }

        private void OnEnable() => press.Enable();

        private void OnDisable()
        {
            press.Disable();
            PressStarted = false;
            IsHeld = false;
        }

        private void OnDestroy() => press.Dispose();

        private void Update()
        {
            PressStarted = false;

            if (press.WasPressedThisFrame() && !IsPointerOverUi())
            {
                PressStarted = true;
                IsHeld = true;
            }

            if (!press.IsPressed())
                IsHeld = false;
        }

        private bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            var pointer = Pointer.current;
            if (eventSystem == null || pointer == null)
                return false;

            pointerData ??= new PointerEventData(eventSystem);
            pointerData.position = pointer.position.ReadValue();
            uiHits.Clear();
            eventSystem.RaycastAll(pointerData, uiHits);
            return uiHits.Count > 0;
        }
    }
}
