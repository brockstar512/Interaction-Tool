using UnityEngine;


namespace IT.Player.StateMachine
{
    using IT.Core.StateMachine;
    using IT.Interactables;
    using IT.Items;
    using IT.Overlap;
    using IT.Player.StateMachine.States;
    using IT.Player.Status;

    public class PlayerStateMachine : MonoBehaviour, IStateMachine, IInteractionContext
    {

        public PlayerStateBase currentState{ get; private set; }

        //States
        public readonly PlayerIdleState defaultState = new PlayerIdleState();
        public readonly PlayerMoveItemState moveItemState = new PlayerMoveItemState();
        public readonly PlayerSlideState slideItemState = new PlayerSlideState();
        public readonly PlayerThrowState throwItemState = new PlayerThrowState();
        public readonly PlayerUseState useItemState = new PlayerUseState();
        public readonly PlayerEquipState equipItemState = new PlayerEquipState();
        public readonly PlayerOpenState PlayerOpenState = new PlayerOpenState();
        public readonly PlayerPullState pullItemState = new PlayerPullState();

        public readonly PlayerDeathState deathState = new PlayerDeathState();

        public PlayerStateBase getState => currentState;

        // Stale-continuation guard (Story 1.4): bumped on every SwitchState. An async state
        // action captures this token before awaiting and checks IsStale(token) after each await.
        public int TransitionCount { get; private set; }
        public bool IsStale(int token) => token != TransitionCount;

        // Possession-release hook (Story 3.1): OnFootController.OnRelease() calls this to
        // trip the stale guard exactly as SwitchState does, so an in-flight async state
        // action aborts when this state machine stops being driven.
        public void IncrementTransitionCount() => TransitionCount++;

        //should this be interface variables... should I put them in a payer controller? or state machine components
        public Vector2 movement { get; private set; }
        public Rigidbody2D rb { get; private set; }

        public Interactable item { get; private set; }

        public IInventory itemManager { get; private set; }

    
        public Animator animator { get; private set; }
    
        private IBestOverlap<Interactable> overlapObjectCheck {  get;  set; }
    
        public PlayerStatusManager playerStatusManager { get; private set; }
        IInventory IInteractionContext.Items => itemManager;
        Transform IInteractionContext.Transform => transform;
        Vector2 IInteractionContext.LookDirection => currentState.LookDirection;
        Animator IInteractionContext.Animator => animator;
        void IInteractionContext.EndInteraction(Interactable next) => SwitchStateFromEquippedItem(next);

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            itemManager = GetComponentInChildren<IInventory>();
            animator = GetComponent<Animator>();
            overlapObjectCheck = GetComponentInChildren<IBestOverlap<Interactable>>();
            playerStatusManager = GetComponent<PlayerStatusManager>();
            currentState = defaultState;

        }

        void Start()
        {
            playerStatusManager.Init(this);
            currentState.EnterState(this);
        }

        // Driven by PlayerWrapper via OnFootController — not called by Unity directly (Story 3.1+).
        public void UpdateTick()
        {
            currentState.UpdateState(this);
        }

        // Driven by PlayerWrapper via OnFootController — not called by Unity directly (Story 3.1+).
        public void FixedTick()
        {
            currentState.FixedUpdateState(this);
            // Physics2D.IgnoreCollision(col, col2, true);

        }

        public void SwitchState(PlayerStateBase newState)
        {
            TransitionCount++;   // bump first so any in-flight continuation that captured the old token sees it as stale
            newState.LookDirection= currentState.LookDirection;
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
            if(currentState is PlayerIdleState)
            {
                item = null;
            }
            Debug.Log($"Updateing state {newState}");

        }

        //button controlled
        public void UseItem()
        {
            if (currentState is PlayerIdleState)
            {
                SwitchState(useItemState);
            }
        }
    
        //button controlled
        public void Interact()
        {
            if (currentState is PlayerIdleState)
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

        void UpdateItem(Interactable newItem)
        {
            //this is the keys for every interactable item
            item = newItem;
        }
   

        public void SwitchStateFromEquippedItem(Interactable newItem = null)
        {
            item = newItem;
            if (item != null)
            {
                SwitchState(this.throwItemState);
                return;
            }

            SwitchState(this.defaultState);
        }


        // Safe entry point into the death state. Real 0-HP→death wiring is Story 4.2;
        // for now this is invoked by a DEBUG key in PlayerStatusManager (Story 1.4 test hook).
        public void EnterDeath()
        {
            SwitchState(deathState);
        }

    }
}
