using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationEquipItem : AnimationState
{

    readonly int HoldStillDown = Animator.StringToHash("HoldStillDown");
    readonly Dictionary<int, float> TimeSheet;
    AnimationPickUp PickUpAnimation;

    public AnimationEquipItem()
    {
        PickUpAnimation = new AnimationPickUp();
        TimeSheet = new()
        {
            { HoldStillDown, 0.250f },
            //{ PickUpUp,0.250f  },
            //{ PickUpDown, 0.250f },
            //{ PickUpLeft, 0.250f }
        };
    }


    
    //show animation?

    public async Task Play(PlayerStateMachineManager playerstate)
    {
            await PickUpAnimation.Play(playerstate);

            playerstate.animator.Play(HoldStillDown);
            await Awaitable.WaitForSecondsAsync(TimeSheet[HoldStillDown]);
            await Task.Delay(250);

    }
}
