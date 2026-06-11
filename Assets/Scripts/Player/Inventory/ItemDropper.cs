using UnityEngine;

namespace IT.Player.Inventory
{
    using IT.Items;

    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] private PickupHolder emptyItemHolder;
        [SerializeField] private float dropForce = 3f;

        public void Drop(ItemBase item, Vector3 position)
        {
            if (item == null || emptyItemHolder == null) return;
            PickupHolder holder = Instantiate(emptyItemHolder, position, Quaternion.identity);
            holder.InitAsDropHolder(item);
            holder.rb.AddForce(Random.insideUnitCircle.normalized * dropForce, ForceMode2D.Impulse);
        }
    }
}
