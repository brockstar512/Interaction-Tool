using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Items.Scripts
{
   public class Key : Item
   {
      Action _disposeOfItem = null;
      public Utilities.KeyTypes keyType;

      public override void Use(PlayerStateMachineManager stateManager)
      {

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
         Debug.Log("Finished");
         // ItemFinishedCallback?.Invoke(null);
      }
   }
}
