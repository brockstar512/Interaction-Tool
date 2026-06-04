using Items;
using UnityEngine;

public class GiveItemEffect : MonoBehaviour, IOpenEffect
{
    [SerializeField] private Item itemPrefab;   // a prefab — the chest spawns one when opened

    public void OnOpen(IInteractionContext context)
    {
        if (itemPrefab == null) return;
        Item item = Instantiate(itemPrefab);     // fresh scene instance, not the asset
        context.Items.PickUpItem(item);          // ItemManager parents it to the player / drops the displaced one
    }
}