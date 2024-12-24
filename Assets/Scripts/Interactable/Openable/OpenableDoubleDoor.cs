using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableDoubleDoor : Openable
{
    private DoubleDoorAnimation _doubleDoorAnimation;

    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        _doubleDoorAnimation = new DoubleDoorAnimation(anim);
    }
    
    public override void OpenAnimation()
    {
        _doubleDoorAnimation.Play();
    }
}
