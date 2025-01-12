using UnityEngine;
using Interactable;
using Unity.VisualScripting;

namespace Items.Scriptable_object_scripts_for_items
{
    public class CandleItem : Item, IButtonUp
    {
        public CandleLight candleLightPrefab;
        private ICandleLight _candleLight;
        
        public override void Use(PlayerStateMachineManager stateManager)
        {  
             ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
             Action(stateManager);
        }
        
        void Action(PlayerStateMachineManager stateManager)
        {
            if(_candleLight == null)
            {
                _candleLight = Instantiate(candleLightPrefab,stateManager.transform);
            }
            _candleLight.On();
        }
        
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
        

        public void ButtonUp()
        {
            _candleLight.Off();
            PutAway();
        }
    }
}


