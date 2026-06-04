using Items;
using UnityEngine;

public class GiveItemEffect : MonoBehaviour, IOpenEffect
{
    [SerializeField] private Item item;   // a child Item of the chest, just like the item inside a Pickupable

    public void OnOpen(IInteractionContext context)
    {
        if (item == null) return;          // empty chest still opens, just gives nothing
        context.Items.PickUpItem(item);    // hands it over; ItemManager adds it or drops the displaced one
    }
}