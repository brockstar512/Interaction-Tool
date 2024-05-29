using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationKick 
{

    readonly int KickRightHash = Animator.StringToHash("KickRight");
    readonly int KickUpHash = Animator.StringToHash("KickUp");
    readonly int KickDownHash = Animator.StringToHash("KickDown");
    readonly int KickLeftHash = Animator.StringToHash("KickLeft");

    readonly int HurtToeRightHash = Animator.StringToHash("HurtToeRight");
    readonly int HurtToeLeftHash = Animator.StringToHash("HurtToeLeft");
    readonly int HurtToeUpHash = Animator.StringToHash("HurtToeUp");
    readonly int HurtToeDownHash = Animator.StringToHash("HurtToeDown");
    readonly Dictionary<int, float> TimeSheet;


    public AnimationKick()
    {
        TimeSheet = new()
        {
            { KickRightHash, 0.333f },
            { KickUpHash,0.333f  },
            { KickDownHash, 0.333f },
            { KickLeftHash, 0.333f },
            { HurtToeRightHash, 0.750f },
            { HurtToeLeftHash,0.750f},
            { HurtToeUpHash, 0.750f},
            { HurtToeDownHash,0.750f },
        };

    }


    public async Task Play(PlayerStateMachineManager playerstate)
    {
        
        if (playerstate.LookDirection == Vector2.down)
        {
            playerstate.animator.Play(KickDownHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[KickDownHash]);
            return;
        }
        if (playerstate.LookDirection == Vector2.up)
        {
            playerstate.animator.Play(KickUpHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[KickUpHash]);
            return;
        }
        if (playerstate.LookDirection == Vector2.right)
        {
            playerstate.animator.Play(KickRightHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[KickRightHash]);
            return;

        }
        if (playerstate.LookDirection == Vector2.left)
        {
            playerstate.animator.Play(KickLeftHash);
            await Awaitable.WaitForSecondsAsync(TimeSheet[KickLeftHash]);
            return;
        }

    }

   

}
