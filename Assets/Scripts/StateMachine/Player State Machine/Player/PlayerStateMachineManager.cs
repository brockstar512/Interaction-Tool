using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachineManager : MonoBehaviour
{
    PlayerBaseState currentState;
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
    public Vector2 LookDirection { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D col { get; private set; }

    public PlayerBaseState GetState { get { return currentState; } }
    public InteractableBase item { get; private set; }
    public RaycastHit2D GetRaycast {
        get {
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, LookDirection, .75f);
            return hit;
        }
    }

    public ItemManager itemManager { get; private set; }
    public PlayerStatus playerStatus { get; private set; }
    public HUDReader hud;
    //get item
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        itemManager = new ItemManager();
    }

    void Start()
    {
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
            RaycastHit2D hit = GetRaycast;
            if (hit.collider == null) return;
            item = hit.collider.transform.GetComponent<InteractableBase>();
            if (item == null) return;
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

        if (Movement == Vector2.up)
        {
            LookDirection = Movement;
        }
        if (Movement == Vector2.down)
        {
            LookDirection = Movement;
        }
        if (Movement == Vector2.right)
        {
            LookDirection = Movement;
        }
        if (Movement == Vector2.left)
        {
            LookDirection = Movement;
        }
    }

}
