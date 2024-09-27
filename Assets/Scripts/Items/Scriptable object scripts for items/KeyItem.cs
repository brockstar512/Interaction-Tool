using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
   [CreateAssetMenu(fileName = "KeyItemObject", menuName = "ScriptableObjects/Key")]
   public class Key : Item
   {

      public override void Use(PlayerStateMachineManager stateManager)
      {
         ItemFinishedCallback = stateManager.SwitchState;
         TargetState = stateManager.defaultState;
         Action();
      }

      void Action()
      {
         
         PutAway();
      }
      
      
      public override void PutAway()
      {
         ItemFinishedCallback?.Invoke(TargetState);
      }
   }
}
