using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Player.StateMachine
{
    using IT.Player.StateMachine.States;

    public interface IPlayerState
    {
        class PlayerIdleState { }
        class PlayerMoveItemState { }
        class PlayerSlideState { }
        class PlayerThrowState { }
        class PlayerUseState { }
        class PlayerStateBase { }

        public void SwitchState(PlayerStateBase state);

    }
}
