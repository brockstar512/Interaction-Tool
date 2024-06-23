using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
//change animation state to interface
public class AnimationPushAndPull : AnimationState
{

    IPushDirection PushDirection;

    public void EnterPushAnimation(PlayerStateMachineManager playerstate)
    {
        if (playerstate.LookDirection == Vector2.down)
        {
            PushDirection = new AnimationDownDirectionPush();
            return;
        }
        if (playerstate.LookDirection == Vector2.up)
        {
            PushDirection = new AnimationUpDirectionPush();
            return;
        }
        if (playerstate.LookDirection == Vector2.right)
        {
            PushDirection = new AnimationRightDirectionPush();

            return;
        }
        if (playerstate.LookDirection == Vector2.left)
        {
            PushDirection = new AnimationLeftDirectionPush();

            return;
        }

        PushDirection = null;

        
    }

    public void Play(PlayerStateMachineManager playerstate)
    {

        //Debug.Log($"dir {playerstate.LookDirection}");

        if (PushDirection == null || !PushDirection.IsInputInDirection(playerstate.LookDirection))
        {
            return;
        }

         PushDirection.Play(playerstate);
    }

    public void LeavePushAnimation()
    {
        PushDirection = null;
    }

    class AnimationRightDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushRight");
        readonly int hold = Animator.StringToHash("PushHoldRight");
        readonly int pull = Animator.StringToHash("PullLeft");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }
        public int Pull { get { return hold; } }

        public void Play(PlayerStateMachineManager playerstate)
        {
              
            if(playerstate.Movement.x > 0)
            {
                playerstate.animator.Play(push);

            }
            else if(playerstate.Movement.x < 0)
            {
                playerstate.animator.Play(pull);

            }
            else
            {
                playerstate.animator.Play(hold);
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
    class AnimationLeftDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushLeft");
        readonly int hold = Animator.StringToHash("PushHoldLeft");
        readonly int pull = Animator.StringToHash("PullRight");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }
        public int Pull { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {

            if (input.x != 0)
            {
                return true;
            }

            //limit weird number direction
            return false;   
        }

        public void Play(PlayerStateMachineManager playerstate)
        {

            if (playerstate.Movement.x < 0)
            {
                playerstate.animator.Play(push);

            }
            else if (playerstate.Movement.x > 0)
            {
                playerstate.animator.Play(pull);

            }
            else
            {
                playerstate.animator.Play(hold);
            }
        }
    }
    class AnimationUpDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushUp");
        readonly int pull = Animator.StringToHash("PullDown");
        readonly int hold = Animator.StringToHash("PushHoldUp");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }
        public int Pull { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {
            if (input.y != 0)
            {
                return true;
            }

            //limit weird number direction
            return false;
        }

        public void Play(PlayerStateMachineManager playerstate)
        {

            if (playerstate.Movement.y > 0)
            {
                playerstate.animator.Play(push);

            }
            else if (playerstate.Movement.y < 0)
            {
                playerstate.animator.Play(pull);

            }
            else
            {
                playerstate.animator.Play(hold);
            }
        }
    }
    class AnimationDownDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushDown");
        readonly int pull = Animator.StringToHash("PullUp");
        readonly int hold = Animator.StringToHash("PushHoldDown");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }
        public int Pull { get { return hold; } }


        public bool IsInputInDirection(Vector2 input)
        {
            if (input.y != 0)
            {
                return true;
            }
            
            //limit weird number direction
            return false;
        }

        public void Play(PlayerStateMachineManager playerstate)
        {
            if (playerstate.Movement.y < 0)
            {
                playerstate.animator.Play(push);

            }
            else if (playerstate.Movement.y > 0)
            {
                playerstate.animator.Play(pull);

            }
            else
            {
                playerstate.animator.Play(hold);
            }
        }
    }

}
