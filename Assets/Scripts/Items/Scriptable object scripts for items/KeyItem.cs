using System.Collections;
using UnityEngine;
using System;

namespace Items.Scriptable_object_scripts_for_items
{
   public class Key : Item
   {
      Action _disposeOfItem = null;
      public Utilities.KeyTypes keyType;
      private IGetMostOverlap overlapObjectCheck {  get;  set; }

      private void Awake()
      {
         overlapObjectCheck = GetComponentInChildren<IGetMostOverlap>();
      }
      public override void Use(PlayerStateMachineManager stateManager)
      {
         ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
         Action(stateManager.transform.position,stateManager.currentState.LookDirection,stateManager.itemManager.GetItem());
         
      }

      async void Action(Vector3 characterPos, Vector3 LookDirection, IItem item)
      {
         if (item is Key key)
         {
            InteractableBase interactable = overlapObjectCheck.GetOverlapObject(characterPos, LookDirection);
            if (interactable == null)
            {
               return;
            }

            //await lock
            if (true)
            {
               _disposeOfItem?.Invoke();
            }
            
         }
         PutAway();
      }
      
      
      public override void PutAway()
      {
         Debug.Log("Finished");
         _disposeOfItem = null;
         ItemFinishedCallback?.Invoke(null);
      }
   }
}
