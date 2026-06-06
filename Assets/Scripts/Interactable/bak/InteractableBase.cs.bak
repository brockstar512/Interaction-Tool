using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    public Rigidbody2D rb { get; set; }
    
    public abstract bool Interact(PlayerStateMachineManager player);

    public abstract void Release(PlayerStateMachineManager player);

    protected void UpdateLayerName()
    {
        this.gameObject.layer = LayerMask.NameToLayer(Utilities.InteractableLayer);
    }
    



}
