using Items;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ChestPickable : Pickupable
{
    private ChestOpenAnimation _chestOpenAnimation;
    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        _chestOpenAnimation = new ChestOpenAnimation(anim);
    }
    
    public override bool Interact(PlayerStateMachineManager player)
    {
        _chestOpenAnimation.Play();
        return base.Interact(player);
    }
    
    public override void PickedUp()
    {
        item = null;
        Destroy(this);
    }
    
}
