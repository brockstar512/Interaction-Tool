using UnityEngine;

namespace Interface
{
    public interface IStateMachine
    {
        //consider passing these into the player states instead of the class
        public PlayerBaseState currentState{ get; }
        public Vector2 movement { get;}
        public Animator animator { get; }
        public IItemManager itemManager { get;}
        public Rigidbody2D rb { get; }

    }
}
