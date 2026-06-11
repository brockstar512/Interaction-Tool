using UnityEngine;

namespace IT.Interactables.Chests
{
    using IT.Items;

    public class SpawnItemOnOpen : MonoBehaviour, IOpenEffect
    {
        [SerializeField] private ItemBase itemPrefab;   // a prefab — the chest spawns one when opened

        public void OnOpen(IInteractionContext context)
        {
            if (itemPrefab == null) return;
            ItemBase item = Instantiate(itemPrefab);     // fresh scene instance, not the asset
            context.Items.PickUpItem(item);          // PlayerInventory parents it to the player / drops the displaced one
        }
    }
}
