using UnityEngine;
using UnityEngine.InputSystem;
using Interactable;

namespace Player.InputManager
{
    
    [RequireComponent(typeof(PlayerStateMachineManager))]
    public class InputManager : MonoBehaviour
    {
        PlayerStateMachineManager _stateManager;
        PlayerController _playerInputActions;
        //Action<IPlayerState.PlayerBaseState> ChangeState;

        void Awake()
        {
            _stateManager = GetComponent<PlayerStateMachineManager>();
            _playerInputActions = new PlayerController();

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

        //todo make this run only when there is a interacle where you hold down the button such as pulling
        private void ReleaseInteraction(InputAction.CallbackContext context)
        {
            PlayerBaseState current = _stateManager.getState;
            // if (current is IButtonUp usingItem)
            // {
            //     usingItem.ButtonUp();
            //     return;
            // }
            if (current is not MoveItemState)
            {
                return;
            }

            _stateManager.Release();
        }

        private void SwitchItem(InputAction.CallbackContext context)
        {
            _stateManager.itemManager.SwitchItem();
        }
        //When am i releasing it... i should be able to clean this up

        private void UseItem(InputAction.CallbackContext context)
        {
            PlayerBaseState current = _stateManager.getState;
            if (current is IButtonUp usingItem)
            {
                //usingItem.ButtonUp();
                Debug.Log("Item up 1");
            
                return;
            }
            Debug.Log("Use item");
            _stateManager.UseItem();
        }

        private void ButtonUp(InputAction.CallbackContext context)
        {
            Debug.Log("Item up 2");

            PlayerBaseState current = _stateManager.getState;
            if (current is IButtonUp usingItem)
            {
                //Debug.Log("Stop item");
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
