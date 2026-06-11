using UnityEngine;
//change animation state to interface
namespace IT.Animation.Player.States
{
    using IT.Interactables.Moveable;
    using IT.Player.StateMachine;

    public class PushPullAnimState : AnimStateBase
    {

        IPushInput _pushDirection;

        public void EnterPushAnimation(PlayerStateMachine state)
        {
            if (state.currentState.LookDirection == Vector2.down)
            {
                _pushDirection = new AnimationDownDirectionPush();
                return;
            }
            if (state.currentState.LookDirection == Vector2.up)
            {
                _pushDirection = new AnimationUpDirectionPush();
                return;
            }
            if (state.currentState.LookDirection == Vector2.right)
            {
                _pushDirection = new AnimationRightDirectionPush();

                return;
            }
            if (state.currentState.LookDirection == Vector2.left)
            {
                _pushDirection = new AnimationLeftDirectionPush();

                return;
            }

            _pushDirection = null;

        
        }

        public void Play(PlayerStateMachine state)
        {
        
            if (_pushDirection == null || !_pushDirection.IsInputInDirection(state.currentState.LookDirection))
            {
                return;
            } 
        
            _pushDirection.Play(state);
        }

        public void LeavePushAnimation()
        {
            _pushDirection = null;
        }

        class AnimationRightDirectionPush : IPushInput
        {
            readonly int _push = Animator.StringToHash("PushRight");
            readonly int _hold = Animator.StringToHash("PushHoldRight");
            readonly int _pull = Animator.StringToHash("PullLeft");
            public int Push { get { return _push; } }
            public int Hold { get { return _hold; } }
            public int Pull { get { return _pull; } }

            public void Play(PlayerStateMachine playerstate)
            {
              
                if(playerstate.movement.x > 0)
                {
                    playerstate.animator.Play(_push);

                }
                else if(playerstate.movement.x < 0)
                {
                    playerstate.animator.Play(_pull);

                }
                else
                {
                    playerstate.animator.Play(_hold);
                }

            }

            public bool IsInputInDirection(Vector2 input)
            {
                //if you are moving in the x direction
                if (input.x!= 0)
                {
                    return true;
                }

                //limit weird number direction?
                return false;
            }
        }
        class AnimationLeftDirectionPush : IPushInput
        {
            readonly int _push = Animator.StringToHash("PushLeft");
            readonly int _hold = Animator.StringToHash("PushHoldLeft");
            readonly int _pull = Animator.StringToHash("PullRight");
            public int Push {get { return _push; } }
            public int Hold { get { return _hold; } }
            public int Pull { get { return _pull; } }

            public bool IsInputInDirection(Vector2 input)
            {

                if (input.x != 0)
                {
                    return true;
                }

                //limit weird number direction
                return false;   
            }

            public void Play(PlayerStateMachine playerstate)
            {

                if (playerstate.movement.x < 0)
                {
                    playerstate.animator.Play(_push);

                }
                else if (playerstate.movement.x > 0)
                {
                    playerstate.animator.Play(_pull);

                }
                else
                {
                    playerstate.animator.Play(_hold);
                }
            }
        }
        class AnimationUpDirectionPush : IPushInput
        {
            readonly int _push = Animator.StringToHash("PushUp");
            readonly int _pull = Animator.StringToHash("PullDown");
            readonly int _hold = Animator.StringToHash("PushHoldUp");
            public int Push { get { return _push; } }
            public int Hold { get { return _hold; } }
            public int Pull { get { return _hold; } }

            public bool IsInputInDirection(Vector2 input)
            {
                if (input.y != 0)
                {
                    return true;
                }

                //limit weird number direction
                return false;
            }

            public void Play(PlayerStateMachine playerstate)
            {

                if (playerstate.movement.y > 0)
                {
                    playerstate.animator.Play(_push);

                }
                else if (playerstate.movement.y < 0)
                {
                    playerstate.animator.Play(_pull);

                }
                else
                {
                    playerstate.animator.Play(_hold);
                }
            }
        }
        class AnimationDownDirectionPush : IPushInput
        {
            readonly int _push = Animator.StringToHash("PushDown");
            readonly int _pull = Animator.StringToHash("PullUp");
            readonly int _hold = Animator.StringToHash("PushHoldDown");
            public int Push { get { return _push; } }
            public int Hold { get { return _hold; } }
            public int Pull { get { return _hold; } }


            public bool IsInputInDirection(Vector2 input)
            {
                if (input.y != 0)
                {
                    return true;
                }
            
                //limit weird number direction
                return false;
            }

            public void Play(PlayerStateMachine playerstate)
            {
                if (playerstate.movement.y < 0)
                {
                    playerstate.animator.Play(_push);

                }
                else if (playerstate.movement.y > 0)
                {
                    playerstate.animator.Play(_pull);

                }
                else
                {
                    playerstate.animator.Play(_hold);
                }
            }
        }

    }
}
