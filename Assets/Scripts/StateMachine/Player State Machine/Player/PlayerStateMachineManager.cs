using Interface;
using UnityEngine;


public class PlayerStateMachineManager : MonoBehaviour, IStateMachine
{
    public PlayerBaseState currentState{ get; private set; }

    //States
    public readonly DefaultState defaultState = new DefaultState();
    public readonly MoveItemState moveItemState = new MoveItemState();
    public readonly SlideItemState slideItemState = new SlideItemState();
    public readonly ThrowItemState throwItemState = new ThrowItemState();
    public readonly UseItemState useItemState = new UseItemState();
    public readonly EquipItemState equipItemState = new EquipItemState();
    public readonly OpenItemState OpenItemState = new OpenItemState();
    public readonly DeathState deathState = new DeathState();

    public PlayerBaseState getState => currentState; 
    
    //should this be interface variables... should I put them in a payer controller? or state machine components
    public Vector2 movement { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public InteractableBase item { get; private set; }

    public IItemManager itemManager { get; private set; }

    
    public Animator animator { get; private set; }
    
    private IGetMostOverlap overlapObjectCheck {  get;  set; }
    
    public PlayerStatusManager playerStatusManager { get; private set; }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemManager = GetComponentInChildren<IItemManager>();
        animator = GetComponent<Animator>();
        overlapObjectCheck = GetComponentInChildren<IGetMostOverlap>();
        playerStatusManager = GetComponent<PlayerStatusManager>();
        currentState = defaultState;

    }

    void Start()
    {
        playerStatusManager.Init(this);
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

    public void SwitchState(PlayerBaseState newState)
    {
        newState.LookDirection= currentState.LookDirection;
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
        if(currentState is DefaultState)
        {
            item = null;
        }
    }

    //button controlled
    public void UseItem()
    {
        if (currentState is DefaultState)
        {
            SwitchState(useItemState);
        }
    }
    
    //button controlled
    public void Interact()
    {
        if (currentState is DefaultState)
        {
            UpdateItem(overlapObjectCheck.GetOverlapObject(this.transform.position,currentState.LookDirection));
            
            if (item == null)
                return;
        }
        //moved out of the block so ifgrappling use item called this the state would still beupdated to throw
        currentState.Action(this);

    }
    
    //button controlled
    public void Release()
    {
        currentState.Action(this);
    }

    public void UpdateMove(Vector2 inputMovement)
    {
        movement = inputMovement;
    }

    void UpdateItem(InteractableBase newItem)
    {
        //this is the keys for every interactable item
        item = newItem;
    }
   

    public void SwitchStateFromEquippedItem(InteractableBase newItem = null)
    {
        item = newItem;
        if (item != null)
        {
            SwitchState(this.throwItemState);
            return;
        }

        SwitchState(this.defaultState);
    }


    public void PlayerDeath()
    {
        //
    }

}
