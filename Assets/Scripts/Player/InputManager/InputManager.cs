using UnityEngine;
using UnityEngine.InputSystem;
using Interactable;



[RequireComponent(typeof(PlayerStateMachineManager))]
public class InputManager : MonoBehaviour
{
    PlayerStateMachineManager stateManager;
    PlayerController playerInputActions;
    //Action<IPlayerState.PlayerBaseState> ChangeState;

    void Awake()
    {
        stateManager = GetComponent<PlayerStateMachineManager>();
        playerInputActions = new PlayerController();

        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += Interact;
        playerInputActions.Player.UseItem.performed += UseItem;
        playerInputActions.Player.UseItem.canceled += ButtonUp;
        playerInputActions.Player.SwitchItem.performed += SwitchItem;
        playerInputActions.Player.Interact.canceled += ReleaseInteraction;


    }

    private void FixedUpdate()
    {
        stateManager.UpdateMove(playerInputActions.Player.Movement.ReadValue<Vector2>());
    }

    private void Interact(InputAction.CallbackContext context)
    {
        stateManager.Interact();
    }

    private void ReleaseInteraction(InputAction.CallbackContext context)
    {
        PlayerBaseState current = stateManager.getState;
        // if (current is IButtonUp usingItem)
        // {
        //     usingItem.ButtonUp();
        //     return;
        // }
        if (current is not MoveItemState)
        {
            return;
        }
        
        stateManager.Release();
    }

    private void SwitchItem(InputAction.CallbackContext context)
    {
        stateManager.itemManager.SwitchItem();
    }

    private void UseItem(InputAction.CallbackContext context)
    {
        PlayerBaseState current = stateManager.getState;
        if (current is IButtonUp usingItem)
        {
            usingItem.ButtonUp();

            return;
        }
        stateManager.UseItem();
    }

    private void ButtonUp(InputAction.CallbackContext context)
    {
        PlayerBaseState current = stateManager.getState;
        if (current is IButtonUp usingItem)
        {
            usingItem.ButtonUp();
        }
    }
    

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact;
        playerInputActions.Player.UseItem.performed -= UseItem;
        playerInputActions.Player.UseItem.canceled -= ButtonUp;
        playerInputActions.Player.SwitchItem.performed -= SwitchItem;
        playerInputActions.Player.Interact.canceled -= ReleaseInteraction;

    }

}
