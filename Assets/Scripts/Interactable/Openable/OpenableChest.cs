using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableChest : Openable
{
    private ChestOpenAnimation _chestOpenAnimation;
    
    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        _chestOpenAnimation = new ChestOpenAnimation(anim);
    }
    

    protected override void OpenAnimation()
    {
        _chestOpenAnimation.Play();

    }


}
