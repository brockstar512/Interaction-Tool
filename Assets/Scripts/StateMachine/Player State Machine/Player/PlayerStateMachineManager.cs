using Interface;
using UnityEngine;
using Player.ItemOverlap;

public class PlayerStateMachineManager : MonoBehaviour, IStateMachine
{
    //simplify to basestate
    public PlayerBaseState currentState{ get; private set; }
    //switching items does not matter on the state

    //States
    public DefaultState defaultState = new DefaultState();
    public MoveItemState moveItemState = new MoveItemState();
    public SlideItemState slideItemState = new SlideItemState();
    public ThrowItemState throwItemState = new ThrowItemState();
    public UseItemState useItemState = new UseItemState();
    public EquipItemState equipItemState = new EquipItemState();
    
    //should this be interface variables
    public Vector2 Movement { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D col { get; private set; }

    public PlayerBaseState GetState { get { return currentState; } }
    public InteractableBase item { get; private set; }

    public ItemManager itemManager { get; private set; }
    //playerstatus
    //playerstatus hud
    //those should all be in a HealthManager script
    public PlayerStatus playerStatus { get; private set; }

    public PlayerStatusHUD playerHUD { get; private set; }
    public Animator animator { get; private set; }
    
    public OverlapObjectCheck overlapObjectCheck { get; private set; }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        itemManager = new ItemManager();
        playerStatus = new PlayerStatus();
        animator = GetComponent<Animator>();

        overlapObjectCheck = GetComponentInChildren<OverlapObjectCheck>();
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
        Debug.Log($"SWITCHING TO  {NewState}");

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
            //Debug.Log($"I am looking at item 2");
                item = overlapObjectCheck.GetOverlapObject(this.transform.position,currentState.LookDirection);
            if (item == null)
                return;
            
            //Debug.Log($"I am looking at item {item}");
            //return;
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
