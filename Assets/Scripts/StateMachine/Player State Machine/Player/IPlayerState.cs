using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine
{
    using IT.Player.StateMachine.States;

    public interface IPlayerState
    {
        class DefaultState { }
        class MoveItemState { }
        class SlideItemState { }
        class ThrowItemState { }
        class UseItemState { }
        class PlayerBaseState { }

        public void SwitchState(PlayerBaseState state);

    }
}
