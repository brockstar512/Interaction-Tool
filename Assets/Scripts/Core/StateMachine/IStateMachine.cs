using UnityEngine;

namespace IT.Core.StateMachine
{
    using IT.Items;
    using IT.Player.StateMachine;

    public interface IStateMachine
    {
        //consider passing these into the player states instead of the class
        public PlayerStateBase currentState{ get; }
        public Vector2 movement { get;}
        public Animator animator { get; }
        public IInventory itemManager { get;}
        public Rigidbody2D rb { get; }

    }
}
