using System.Collections;
using System.Collections.Generic;
using Items.Scripts;
using UnityEngine;

public abstract class Openable : InteractableBase
{
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
    
    public override bool Interact(PlayerStateMachineManager player)
    { 
        if (key == Utilities.KeyTypes.None)
        {
            OpenAnimation();
            return true;
        }
        if (CorrectKey(player.itemManager.GetItem()))
        {
            ((Key)player.itemManager.GetItem()).Use(player);
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
