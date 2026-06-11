using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Player.Input
{
    using IT.Interactables;
    using IT.Player.StateMachine;
    using IT.Player.StateMachine.States;

    [RequireComponent(typeof(PlayerStateMachine))]
    public class PlayerInputHandler : MonoBehaviour
    {
        PlayerStateMachine _stateManager;
        PlayerInputActions _playerInputActions;

        void Awake()
        {
            _stateManager = GetComponent<PlayerStateMachine>();
            _playerInputActions = new PlayerInputActions();

            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Interact.performed += Interact;
            _playerInputActions.Player.UseItem.performed += UseItem;
            _playerInputActions.Player.UseItem.canceled += ButtonUp;
            _playerInputActions.Player.SwitchItem.performed += SwitchItem;
            _playerInputActions.Player.Interact.canceled += ReleaseInteraction;
        }

        private void FixedUpdate()
        {
            _stateManager.UpdateMove(_playerInputActions.Player.Movement.ReadValue<Vector2>());
        }

        private void Interact(InputAction.CallbackContext context)
        {
            _stateManager.Interact();
        }

        private void ReleaseInteraction(InputAction.CallbackContext context)
        {
            PlayerStateBase current = _stateManager.getState;
            if (current is IButtonUp buttonUp)   // hold-to-interact states (pull) release here
            {
                buttonUp.ButtonUp();
                return;
            }
            if (current is not PlayerMoveItemState)
            {
                return;
            }
            _stateManager.Release();
        }

        private void SwitchItem(InputAction.CallbackContext context)
        {
            _stateManager.itemManager.SwitchItem();
        }

        private void UseItem(InputAction.CallbackContext context)
        {
            PlayerStateBase current = _stateManager.getState;
            if (current is IButtonUp usingItem)
            {
                return;
            }
            _stateManager.UseItem();
        }

        private void ButtonUp(InputAction.CallbackContext context)
        {
            PlayerStateBase current = _stateManager.getState;
            if (current is IButtonUp usingItem)
            {
                usingItem.ButtonUp();
            }
        }

        private void OnDestroy()
        {
            _playerInputActions.Player.Interact.performed -= Interact;
            _playerInputActions.Player.UseItem.performed -= UseItem;
            _playerInputActions.Player.UseItem.canceled -= ButtonUp;
            _playerInputActions.Player.SwitchItem.performed -= SwitchItem;
            _playerInputActions.Player.Interact.canceled -= ReleaseInteraction;
        }
    }
}