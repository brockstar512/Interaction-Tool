using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.Player.Input
{
    using IT.Player.Control;

    // TODO (Story 3.2): entire bridge replaced by InputUser polling in PlayerWrapper.
    //
    // Story 3.1 turned this handler into a thin input *bridge*. Instead of calling
    // PlayerStateMachine directly, each action callback sets an edge-triggered flag on
    // PlayerWrapper; the wrapper drains those flags in its Update() and forwards them to
    // the active IPlayerController. Same events → same work, just routed through the
    // possession seam — behavior is unchanged.
    [RequireComponent(typeof(PlayerWrapper))]
    public class PlayerInputHandler : MonoBehaviour
    {
        PlayerWrapper _wrapper;
        PlayerInputActions _playerInputActions;

        void Awake()
        {
            _wrapper = GetComponent<PlayerWrapper>();
            _playerInputActions = new PlayerInputActions();

            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Interact.performed += Interact;
            _playerInputActions.Player.Interact.canceled += ReleaseInteraction;
            _playerInputActions.Player.UseItem.performed += UseItem;
            _playerInputActions.Player.UseItem.canceled += ButtonUp;
            _playerInputActions.Player.SwitchItem.performed += SwitchItem;
        }

        private void FixedUpdate()
        {
            // Movement is polled (not edge-triggered); the wrapper reads it during FixedTick.
            _wrapper.SetMoveInput(_playerInputActions.Player.Movement.ReadValue<Vector2>());
        }

        private void Interact(InputAction.CallbackContext context)
        {
            _wrapper._interactPressed = true;
        }

        private void ReleaseInteraction(InputAction.CallbackContext context)
        {
            _wrapper._interactReleased = true;
        }

        private void UseItem(InputAction.CallbackContext context)
        {
            _wrapper._usePressed = true;
        }

        private void ButtonUp(InputAction.CallbackContext context)
        {
            _wrapper._useReleased = true;
        }

        private void SwitchItem(InputAction.CallbackContext context)
        {
            _wrapper._switchItemPressed = true;
        }

        private void OnDestroy()
        {
            _playerInputActions.Player.Interact.performed -= Interact;
            _playerInputActions.Player.Interact.canceled -= ReleaseInteraction;
            _playerInputActions.Player.UseItem.performed -= UseItem;
            _playerInputActions.Player.UseItem.canceled -= ButtonUp;
            _playerInputActions.Player.SwitchItem.performed -= SwitchItem;
        }
    }
}
