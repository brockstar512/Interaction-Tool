using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IT.Animation.Player.States
{
    using IT.Player.Movement;
    using IT.Player.StateMachine;

    public class EquipAnimState : AnimStateBase
    {

        readonly int HoldStillDown = Animator.StringToHash("HoldStillDown");
        readonly Dictionary<int, float> TimeSheet;
        PickUpAnimState PickUpAnimation;

        public EquipAnimState()
        {
            PickUpAnimation = new PickUpAnimState();
            TimeSheet = new()
            {
                { HoldStillDown, 0.250f },
                //{ PickUpUp,0.250f  },
                //{ PickUpDown, 0.250f },
                //{ PickUpLeft, 0.250f }
            };
        }


    
        //show animation?

        public async Task Play(PlayerStateMachine playerstate)
        {
                await PickUpAnimation.Play(playerstate);
                SpriteRenderer itemOriginSpriteRenderer = playerstate.GetComponentInChildren<ItemAnchorPoint>().getSpriteRenderer;
                itemOriginSpriteRenderer.sprite = playerstate.itemManager.GetCurrentSprite();
                playerstate.animator.Play(HoldStillDown);
                await Awaitable.WaitForSecondsAsync(TimeSheet[HoldStillDown]);
                await Task.Delay(250);
                itemOriginSpriteRenderer.sprite = null;

        }
    }
}
