using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pullable : InteractableBase
{
    //pull direction.
    //origin. 
    //distance
    //time
    public override InteractionKind Kind => InteractionKind.Pull;
    public override bool Interact(IInteractionContext context)
    {
        throw new System.NotImplementedException();
    }

    public override void Release(IInteractionContext context)
    {
        throw new System.NotImplementedException();
    }
}
