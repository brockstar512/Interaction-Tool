
using UnityEngine;
using Items.Scriptable_object_scripts_for_items;

public class OpenableLock : OpenableChest
{
    [SerializeField] private Utilities.KeyTypes key;
    private DoubleDoorAnimation _doubleDoorAnimation;
    private void Awake()
    {
        UpdateLayerName();
        Animator anim = GetComponent<Animator>();
        //_doubleDoorAnimation = new DoubleDoorAnimation(anim);
    }

    public override bool Interact(PlayerStateMachineManager player)
    {
        if (player.itemManager.GetItem() is Key keyItem && keyItem.keyType == key)
        {
            return base.Interact(player);
        }

        return false;
    }

    protected override void OpenAnimation()
    {
        _doubleDoorAnimation.Play();
    }
    

    public override void Release(PlayerStateMachineManager player)
    {
        throw new System.NotImplementedException();
    }
}
