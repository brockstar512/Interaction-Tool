using UnityEngine;

namespace Interface
{
    public interface IStateMachine
    {
        public PlayerBaseState currentState{ get; }
        public Vector2 movement { get;}
        public Animator animator { get; }
        public ItemManager itemManager { get;}
        public Rigidbody2D rb { get; }

    }
}
