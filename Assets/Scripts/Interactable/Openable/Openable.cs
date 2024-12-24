using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Openable : InteractableBase
{
    private ChestOpenAnimation _chestOpenAnimation;
    protected bool isClosed = true;
    
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
            OpenAnimation();

        }
        return isClosed;

    }

    public virtual void OpenAnimation()
    {
        _chestOpenAnimation.Play();

    }

    public override void Release(PlayerStateMachineManager player)
    {
    }
}
