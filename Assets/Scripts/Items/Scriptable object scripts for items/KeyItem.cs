using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
   [CreateAssetMenu(fileName = "KeyItemObject", menuName = "ScriptableObjects/Key")]
   public class Key : Item
   {
      Action _disposeOfItem = null;


      public override void Use(PlayerStateMachineManager stateManager)
      {
         ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
         _disposeOfItem = stateManager.itemManager.DisposeOfCurrentItem;

         Action();
      }

      void Action()
      {
         _disposeOfItem?.Invoke();
         PutAway();
      }
      
      
      public override void PutAway()
      {
         ItemFinishedCallback?.Invoke(null);
      }
   }
}
