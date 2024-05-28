using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    public Rigidbody2D rb { get; set; }

    //added bool for animation of suggest or not
    public abstract bool Interact(PlayerStateMachineManager player);

    public abstract void Release(PlayerStateMachineManager player);

}
