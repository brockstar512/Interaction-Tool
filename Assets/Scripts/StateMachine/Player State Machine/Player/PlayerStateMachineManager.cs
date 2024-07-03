using System;
using System.Collections;
using System.Collections.Generic;
using Interface;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachineManager : MonoBehaviour, IStateMachine
{
    public PlayerBaseState currentState{ get; private set; }
    //switching items does not matter on the state

    //States
    public DefaultState defaultState = new DefaultState();
    public MoveItemState moveItemState = new MoveItemState();
    public SlideItemState slideItemState = new SlideItemState();
    public ThrowItemState throwItemState = new ThrowItemState();
    public UseItemState useItemState = new UseItemState();
    public EquipItemState equipItemState = new EquipItemState();

    //Dependencies
    public Vector2 Movement { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D col { get; private set; }

    public PlayerBaseState GetState { get { return currentState; } }
    public InteractableBase item { get; private set; }

    public IOverlapCheck OverlapObjectCheck;
    public ItemManager itemManager { get; private set; }
    public PlayerStatus playerStatus { get; private set; }

    public PlayerStatusHUD playerHUD { get; private set; }
    public Animator animator { get; private set; }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        itemManager = new ItemManager();
        playerStatus = new PlayerStatus();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        playerHUD = HUDReader.Instance.InitializePlayerHUD(this);
        currentState = defaultState;
        currentState.EnterState(this);
    }

    void Update()
    {

        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateState(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        currentState.OnCollisionEnter(this, collision);
    }

    public void SwitchState(PlayerBaseState NewState)
    {
        currentState.ExitState(this);
        currentState = NewState;
        currentState.EnterState(this);
        if(currentState is DefaultState)
        {
            item = null;
        }
    }

    public void UseItem()
    {
        SwitchState(useItemState);
    }

    public void Interact()
    {
        if (currentState is DefaultState)
        {
            InteractableBase item = OverlapObjectCheck.GetOverlapObject();
            if (item == null)
                return;
        }
        currentState.Action(this);
    }

    public void Release()
    {
        currentState.Action(this);
    }

    public void UpdateMove(Vector2 movement)
    {
        Movement = movement;
    }

}
