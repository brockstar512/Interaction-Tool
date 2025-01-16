using System.Collections;
using UnityEngine;
using System;
using Doors;
using Unity.VisualScripting;

namespace Items.Scriptable_object_scripts_for_items
{
   public class Key : Item
   {
      Action _disposeOfItem = null;
      public Utilities.KeyTypes keyType;
      private IGetMostOverlap<ILocked> overlapObjectCheck {  get;  set; }

      private void Awake()
      {
         overlapObjectCheck = GetComponentInChildren<IGetMostOverlap<ILocked>>();
      }
      public override void Use(PlayerStateMachineManager stateManager)
      {
         ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
         _disposeOfItem = stateManager.itemManager.DisposeOfCurrentItem;
         Action(stateManager.transform.position,stateManager.currentState.LookDirection,stateManager.itemManager.GetItem());
      }

      async void Action(Vector3 characterPos, Vector3 LookDirection, IItem item)
      {
         if (item is Key key)
         {
            ILocked door = overlapObjectCheck.GetOverlapObject(characterPos, LookDirection);
            
            //turn this to Ilock?
            if (door != null && await door.CanOpen(key.keyType))
            {
               _disposeOfItem?.Invoke();
            }
            
         }
         PutAway();
      }
      
      
      public override void PutAway()
      {
         _disposeOfItem = null;
         ItemFinishedCallback?.Invoke(null);
      }
   }
}
