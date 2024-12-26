using System.Collections;
using System.Collections.Generic;
using Items.Scriptable_object_scripts_for_items;
using UnityEngine;

public abstract class Openable : InteractableBase
{
    [SerializeField] private Utilities.KeyTypes key;
    protected bool isClosed = true;

    protected bool CorrectKey(IItem item)
    {
        if (item is Key Item)
        {
            Debug.Log($"Key Type: {Item.keyType}");
        }
        

        if (key == Utilities.KeyTypes.None)
        {
            return true;
        }
        if (item is Key keyItem && keyItem.keyType == key)
        {
            return true;
        }

        return false;
    }
    
    public override bool Interact(PlayerStateMachineManager player)
    {
        
        Debug.Log($"Interact {player.itemManager.GetItem()}");
        if (CorrectKey(player.itemManager.GetItem()))
        {
            OpenAnimation();
            return true;
        }

        return false;
    }

    protected virtual void OpenAnimation() { }


    public override void Release(PlayerStateMachineManager player)
    {
        Debug.Log("Destroy");
    }
}
