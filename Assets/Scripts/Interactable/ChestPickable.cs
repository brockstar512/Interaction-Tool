using Items;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class ChestPickable : Pickupable
{
    [FormerlySerializedAs("item")]
    [SerializeField] private Item _containedItem;

    private ChestOpenAnimation _chestOpenAnimation;
    public override Sprite Sprite => item?.Sprite;

    protected override void Awake()
    {
        base.Awake();
        item = _containedItem;
        Animator anim = GetComponent<Animator>();
        _chestOpenAnimation = new ChestOpenAnimation(anim);
    }

    protected override void ApplyItemSprite() { }
    
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
