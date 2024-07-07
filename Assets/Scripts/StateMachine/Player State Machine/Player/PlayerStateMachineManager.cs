using Interface;
using UnityEngine;
using Player.ItemOverlap;

public class PlayerStateMachineManager : MonoBehaviour, IStateMachine
{
    //have a hurt state... tht determines if you die
    
    //simplify to basestate
    public PlayerBaseState currentState{ get; private set; }

    //States
    public readonly DefaultState DefaultState = new DefaultState();
    public readonly MoveItemState MoveItemState = new MoveItemState();
    public readonly SlideItemState SlideItemState = new SlideItemState();
    public readonly ThrowItemState ThrowItemState = new ThrowItemState();
    public readonly UseItemState UseItemState = new UseItemState();
    public readonly EquipItemState EquipItemState = new EquipItemState();
    //public readonly HurtState HurtState = new EquipItemState();

    
    //should this be interface variables
    public Vector2 movement { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public PlayerBaseState getState => currentState; 
    public InteractableBase item { get; private set; }

    public ItemManager itemManager { get; private set; }

    
    public Animator animator { get; private set; }
    
    private OverlapObjectCheck overlapObjectCheck {  get;  set; }
    
    public PlayerStatusManager playerStatusManager { get; private set; }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        itemManager = new ItemManager();
        animator = GetComponent<Animator>();
        overlapObjectCheck = GetComponentInChildren<OverlapObjectCheck>();
        playerStatusManager = GetComponent<PlayerStatusManager>();
    }

    void Start()
    {
        playerStatusManager.Init(this);
        currentState = DefaultState;
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

    public void UseItem()
    {
        SwitchState(UseItemState);
    }

    public void Interact()
    {
        if (currentState is DefaultState)
        {
                item = overlapObjectCheck.GetOverlapObject(this.transform.position,currentState.LookDirection);
            if (item == null)
                return;
        }
        currentState.Action(this);
    }

    public void Release()
    {
        currentState.Action(this);
    }

    public void UpdateMove(Vector2 inputMovement)
    {
        movement = inputMovement;
    }

}
