using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

//todo delete this
//AnimationPickUpAndThrow
public class AnimationPickUpAndThrow
{
    readonly int PickUpRight = Animator.StringToHash("PickUpRight");
    readonly int PickUpUp = Animator.StringToHash("PickUpUp");
    readonly int PickUpDown = Animator.StringToHash("PickUpDown");
    readonly int PickUpLeft = Animator.StringToHash("PickUpLeft");
    //seperate pick up and seperate hold with throw
    //readonly int ThrowRight = Animator.StringToHash("ThrowRight");
    //readonly int ThrowUp = Animator.StringToHash("ThrowUp");
    //readonly int ThrowDown = Animator.StringToHash("ThrowDown");
    //readonly int ThrowLeft = Animator.StringToHash("ThrowLeft");

    //readonly int HoldStillRight = Animator.StringToHash("HoldStillRight");
    //readonly int HoldStillLeft = Animator.StringToHash("HoldStillLeft");
    //readonly int HoldStillUp = Animator.StringToHash("HoldStillUp");
    //readonly int HoldStillDown = Animator.StringToHash("HoldStillDown");


    //readonly int HoldWalkRight = Animator.StringToHash("HoldWalkRight");
    //readonly int HoldWalkLeft = Animator.StringToHash("HoldWalkLeft");
    //readonly int HoldWalkUp = Animator.StringToHash("HoldWalkUp");
    //readonly int HoldWalkDown = Animator.StringToHash("HoldWalkDown");
    readonly Dictionary<int, float> TimeSheet;


    public AnimationPickUpAndThrow()
    {
        TimeSheet = new()
        {
            //{ PickUpRight, 0.250f },
            //{ PickUpUp,0.250f  },
            //{ PickUpDown, 0.250f },
            //{ PickUpLeft, 0.250f },
            //{ ThrowRight, 0.500f },
            //{ ThrowUp,0.500f},
            //{ ThrowDown, 0.500f},
            //{ ThrowLeft,0.500f },
            //{ HoldStillRight,1f},
            //{ HoldStillLeft,1f},
            //{ HoldStillUp, 1f},
            //{ HoldStillDown,1f },
            ////walking
            //{ HoldWalkRight,.333f},
            //{ HoldWalkLeft,.333f},
            //{ HoldWalkUp, .333f},
            //{ HoldWalkDown,.333f }
        };

    }


    public async Task Play(PlayerStateMachineManager playerstate)
    {
        if (playerstate.LookDirection == Vector2.down)
        {
            playerstate.animator.Play(PickUpDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[PickUpDown]);
            return;
        }
    }


}
