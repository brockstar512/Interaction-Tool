using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Interactable;

namespace Items.Scriptable_object_scripts_for_items
{
    [CreateAssetMenu(fileName = "CandleItemObject", menuName = "ScriptableObjects/Candle")]
    public class CandleItem : Item, IButtonUp
    {
        [SerializeField] private Transform lightPrefab;
        private Transform _light;

        public override void Use(PlayerStateMachineManager stateManager)
        {  
             ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
             Action(stateManager);
        }
        
        void Action(PlayerStateMachineManager stateManager)
        {
            _light = Instantiate(lightPrefab,stateManager.transform);
        }
        public override void PutAway()
        {
            ItemFinishedCallback?.Invoke(null);
        }
        
        
        

        public void ButtonUp()
        {
            Destroy(_light.gameObject);
            PutAway();
        }
    }
}


