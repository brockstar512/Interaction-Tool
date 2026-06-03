using Items;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ChestPickable : Pickupable
{
    [SerializeField] private Pickupable _droppedItemHolderPrefab;
    [SerializeField] private float _dropForce = 3f;

    private ChestOpenAnimation _chestOpenAnimation;
    public override Sprite Sprite => item?.Sprite;
    public override IItem item => heldItem;
    private IItem heldItem => GetComponentInChildren<IItem>();

    protected override void Awake()
    {
        base.Awake();
        Animator anim = GetComponent<Animator>();
        _chestOpenAnimation = new ChestOpenAnimation(anim);
    }

    protected override void ApplyItemSprite() { }

    public override bool Interact(IInteractionContext context)
    {
        if (item == null) return false;
        _chestOpenAnimation.Play();
        return base.Interact(context);
    }

    public override void Swap(IItem displaced)
    {
        if (_droppedItemHolderPrefab == null || displaced is not Item itemToDrop) return;
        //this is wrong and should not be the chests responbisility
        var dropHolder = Instantiate(_droppedItemHolderPrefab, transform.position, Quaternion.identity);
        dropHolder.InitAsDropHolder(itemToDrop);
        dropHolder.rb.AddForce(Random.insideUnitCircle.normalized * _dropForce, ForceMode2D.Impulse);
        PickedUp();
    }

    public override void PickedUp()
    {
        item = null;
        Destroy(this);
    }
    
}
