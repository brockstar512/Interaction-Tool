using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableDoor : Openable
{
    private SingleDoorAnimation _singleDoorAnimation;

    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        _singleDoorAnimation = new SingleDoorAnimation(anim);
    }
    
    protected override void OpenAnimation()
    {
        _singleDoorAnimation.Play();
    }
    

}
