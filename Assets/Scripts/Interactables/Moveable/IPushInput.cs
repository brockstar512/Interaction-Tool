using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Moveable
{
    using IT.Player.StateMachine;

    public interface IPushInput
    {
        int Push { get; }
        int Pull { get; }
        int Hold { get; }
        public abstract bool IsInputInDirection(Vector2 input);
        public void Play(PlayerStateMachine playerstate);



    }
}
