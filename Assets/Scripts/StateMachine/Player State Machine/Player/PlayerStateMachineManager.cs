using Interface;
using UnityEngine;
using Player.ItemOverlap;

public class PlayerStateMachineManager : MonoBehaviour, IStateMachine
{       
    [SerializeField] OverlapObjectCheck overlapObjectCheck;

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
        overlapObjectCheck.UpdateCheckPosition(currentState.LookDirection);//put this in state

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
        Debug.Log($"I am looking at item 1");

        if (currentState is DefaultState)
        {
            Debug.Log($"I am looking at item 2");

            InteractableBase i = overlapObjectCheck.GetOverlapObject(this.transform.position);
            if (i == null)
                return;
            
            Debug.Log($"I am looking at item {i}");
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
