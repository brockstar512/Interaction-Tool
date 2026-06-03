using UnityEngine;
using System;


namespace Items
{
   public class Key : Item
   {
      Action _disposeOfItem = null;
      public Utilities.KeyTypes keyType;
      private IGetMostOverlap<ILocked> overlapObjectCheck { get; set; }

      private void Awake()
      {
         overlapObjectCheck = GetComponentInChildren<IGetMostOverlap<ILocked>>();
      }

      public override void Use(IInteractionContext context)
      {
         ItemFinishedCallback = context.EndInteraction;
         _disposeOfItem = context.Items.DisposeOfCurrentItem;
         TryOpen(context.Transform.position, context.LookDirection, context.Items.GetItem());
      }

      void TryOpen(Vector3 characterPos, Vector3 lookDirection, IItem item)
      {
         if (item is Key key)
         {
            ILocked door = overlapObjectCheck.GetOverlapObject(characterPos, lookDirection);
            if (door != null && door.CanOpen(key.keyType))
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
