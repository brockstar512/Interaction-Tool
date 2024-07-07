using System.Collections.Generic;
using UnityEngine;

namespace AnimationStates
{


    public class AnimationCarry : AnimationState
    {
        readonly int _holdStillRight = Animator.StringToHash("HoldStillRight");
        readonly int _holdStillLeft = Animator.StringToHash("HoldStillLeft");
        readonly int _holdStillUp = Animator.StringToHash("HoldStillUp");
        readonly int _holdStillDown = Animator.StringToHash("HoldStillDown");
        readonly int _holdWalkRight = Animator.StringToHash("HoldWalkRight");
        readonly int _holdWalkLeft = Animator.StringToHash("HoldWalkLeft");
        readonly int _holdWalkUp = Animator.StringToHash("HoldWalkUp");
        readonly int _holdWalkDown = Animator.StringToHash("HoldWalkDown");
        readonly Dictionary<int, float> _timeSheet;

        public AnimationCarry()
        {
            _timeSheet = new()
            {
                { _holdStillRight, 1f },
                { _holdStillLeft, 1f },
                { _holdStillUp, 1f },
                { _holdStillDown, 1f },
                { _holdWalkRight, .333f },
                { _holdWalkLeft, .333f },
                { _holdWalkUp, .333f },
                { _holdWalkDown, .333f }
            };
        }

        public void Play(PlayerStateMachineManager state)
        {
            if (state.movement.x != 0 || state.movement.y != 0)
            {
                if (state.currentState.LookDirection == Vector2.down)
                {
                    state.animator.Play(_holdWalkDown);
                }

                if (state.currentState.LookDirection == Vector2.up)
                {
                    state.animator.Play(_holdWalkUp);
                }

                if (state.currentState.LookDirection == Vector2.right)
                {
                    state.animator.Play(_holdWalkRight);

                }

                if (state.currentState.LookDirection == Vector2.left)
                {
                    state.animator.Play(_holdWalkLeft);
                }

            }
            else
            {
                if (state.currentState.LookDirection == Vector2.down)
                {
                    state.animator.Play(_holdStillDown);
                }

                if (state.currentState.LookDirection == Vector2.up)
                {
                    state.animator.Play(_holdStillUp);
                }

                if (state.currentState.LookDirection == Vector2.right)
                {
                    state.animator.Play(_holdStillRight);

                }

                if (state.currentState.LookDirection == Vector2.left)
                {
                    state.animator.Play(_holdStillLeft);
                }
            }
            //await Task.CompletedTask;
        }


    }
}
