using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Door : MonoBehaviour
{
    private void Awake()
    {
        UpdateLayerName();
    }

    protected void UpdateLayerName()
    {
        this.gameObject.layer = LayerMask.NameToLayer(Utilities.DoorLayer);
    }

}
