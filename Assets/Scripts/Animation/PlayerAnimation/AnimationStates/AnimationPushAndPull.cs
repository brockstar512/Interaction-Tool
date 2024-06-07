using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPushAndPull : AnimationState
{
    //whichside are you on... smaller classes
    readonly int PushDown = Animator.StringToHash("PushDown");
    readonly int PushUp = Animator.StringToHash("PushUp");
    readonly int PushRight = Animator.StringToHash("PushRight");
    readonly int PushLeft = Animator.StringToHash("PushLeft");

    readonly int PushHoldLeft = Animator.StringToHash("PushHoldLeft");
    readonly int PushHoldRight = Animator.StringToHash("PushHoldRight");
    readonly int PushHoldUp = Animator.StringToHash("PushHoldUp");
    readonly int PushHoldDown = Animator.StringToHash("PushHoldDown");

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

    //enter create class
    public void Play(PlayerStateMachineManager playerstate)
    {

        //Debug.Log($"dir {playerstate.LookDirection}");

        if (PushDirection == null || !PushDirection.IsInputInDirection(playerstate.LookDirection))
        {
            return;
        }

        if (playerstate.Movement.x != 0 || playerstate.Movement.y != 0)
        {
            //if you are moving and the look direction
            playerstate.animator.Play(PushDirection.Push);

        }
        else
        {
            playerstate.animator.Play(PushDirection.Hold);
        }

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
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {

            if (input.x!= 0)
            {
                return true;
            }
                //if (input == Vector2.down || input == Vector2.up)
                //{
                //    return false;
                //}
                //if (input == Vector2.left)
                //{
                //    //pull... could be a out variable or tuple
                //    return true;
                //}
                //if (input == Vector2.right)
                //{
                //    //push
                //    return true;
                //}

                //limit weird number direction
                return false;
        }
    }


    

    class AnimationLeftDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushLeft");
        readonly int hold = Animator.StringToHash("PushHoldLeft");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {

            if (input.x != 0)
            {
                return true;
            }
            //if(input == Vector2.down || input == Vector2.up)
            //{
            //    return false;
            //}
            //if (input == Vector2.right)
            //{
            //    //pull... could be a out variable or tuple
            //    return true;
            //}
            //if (input == Vector2.left)
            //{
            //    //push
            //    return true;
            //}

            //limit weird number direction
            return false;   
        }



    }
    class AnimationUpDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushUp");
        readonly int hold = Animator.StringToHash("PushHoldUp");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {
            if (input.y != 0)
            {
                return true;
            }

            //if (input == Vector2.right || input == Vector2.left)
            //{
            //    return false;
            //}
            //if (input == Vector2.down)
            //{
            //    //pull... could be a out variable or tuple
            //    return true;
            //}
            //if (input == Vector2.up)
            //{
            //    //push
            //    return true;
            //}

            //limit weird number direction
            return false;
        }


    }
    class AnimationDownDirectionPush : IPushDirection
    {
        private Vector2 LimitedMovementBounds = Vector2.zero;
        readonly int push = Animator.StringToHash("PushDown");
        readonly int hold = Animator.StringToHash("PushHoldDown");
        public int Push { get { return push; } }
        public int Hold { get { return hold; } }

        public bool IsInputInDirection(Vector2 input)
        {
            if (input.y != 0)
            {
                return true;
            }
            //if (input == Vector2.right || input == Vector2.left)
            //{
            //    return false;
            //}
            //if (input == Vector2.up)
            //{
            //    //pull... could be a out variable or tuple
            //    return true;
            //}
            //if (input == Vector2.down)
            //{
            //    //push
            //    return true;
            //}

            //limit weird number direction
            return false;
        }
    }
}
