using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Openable : InteractableBase
{
    private ChestOpenAnimation _chestOpenAnimation;
    private bool isClosed = true;
    
    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        _chestOpenAnimation = new ChestOpenAnimation(anim);
    }
    
    public override bool Interact(PlayerStateMachineManager player)
    {
        if (isClosed)
        {
            isClosed = false;
            _chestOpenAnimation.Play();

        }
        return isClosed;

    }

    public override void Release(PlayerStateMachineManager player)
    {
    }
}
//can give item... or health status ...openable can be a door as well. there should be a key to determine 
//locked openable to inherit from openable...chest should as well// lock base.open... base.open give item 