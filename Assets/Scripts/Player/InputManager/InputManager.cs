using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;


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
        playerInputActions.Player.SwitchItem.performed += SwitchItem;
        playerInputActions.Player.Interact.canceled += Release;


    }

    void FixedUpdate()
    {
        stateManager.UpdateMove(playerInputActions.Player.Movement.ReadValue<Vector2>());
    }

    public void Interact(InputAction.CallbackContext context)
    {
        stateManager.Interact();
    }

    public void Release(InputAction.CallbackContext context)
    {
        PlayerBaseState current = stateManager.GetState;
        if (current is not MoveItemState)
        {
            return;
        }
        stateManager.Release();
    }

    public void SwitchItem(InputAction.CallbackContext context)
    {
        Debug.Log("Switch items");
    }

    public void UseItem(InputAction.CallbackContext context)
    {
        ///switch state and maybe pass in fuction that enters state so it is not null
        stateManager.UseItem();

    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact;
        playerInputActions.Player.UseItem.performed -= UseItem;
        playerInputActions.Player.SwitchItem.performed -= SwitchItem;
        playerInputActions.Player.Interact.canceled -= Release;

    }

}
