// Assets/Scripts/Doors/MultipleDependentDoubleDoor.cs

using System;
using UnityEngine;

public class MultipleDependentDoubleDoor : MultiDependent<bool>
{
    private bool _opened;
    private DoubleDoorAnimation _doubleDoorAnimation;

    private void Awake()
    {
        _doubleDoorAnimation = new DoubleDoorAnimation(GetComponent<Animator>());
    }

    protected override void Reevaluate()
    {
        if (_opened) return;

        bool any = false;
        foreach (bool v in Values())
        {
            any = true;
            if (!v) return;          // one false → bail
        }

        if (any) Open();             // all assigned sources were true
    }

    private void Open()
    {
        _opened = true;
        _doubleDoorAnimation.Play();
        Destroy(this);
    }
}