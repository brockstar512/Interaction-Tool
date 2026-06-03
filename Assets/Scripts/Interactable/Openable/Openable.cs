using System.Collections;
using System.Collections.Generic;
using Items;
using Items.Scriptable_object_scripts_for_items;
using UnityEngine;

public abstract class Openable : InteractableBase
{
    public override InteractionKind Kind => InteractionKind.Open;
    [SerializeField] private Utilities.KeyTypes key;
    protected bool isClosed = true;

    protected bool CorrectKey(IItem item)
    {
        
        if (item is Key keyItem && keyItem.keyType == key)
        {
            return true;
        }

        return false;
    }
    
    public override bool Interact(IInteractionContext context)
    {
        if (key == Utilities.KeyTypes.None)
        {
            OpenAnimation();
            return true;
        }
        if (CorrectKey(context.Items.GetItem()))
        {
            ((Key)context.Items.GetItem()).Use(context);
            OpenAnimation();
            return true;
        }

        return false;
    }

    public override void Release(IInteractionContext context)
    {
        Debug.Log("Destroy");
    }

    protected virtual void OpenAnimation() { }

    
}
