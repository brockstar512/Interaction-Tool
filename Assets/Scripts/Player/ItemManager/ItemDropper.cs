using UnityEngine;

namespace IT.Player.Inventory
{
    using IT.Items;

    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] private Pickupable emptyItemHolder;
        [SerializeField] private float dropForce = 3f;

        public void Drop(Item item, Vector3 position)
        {
            if (item == null || emptyItemHolder == null) return;
            Pickupable holder = Instantiate(emptyItemHolder, position, Quaternion.identity);
            holder.InitAsDropHolder(item);
            holder.rb.AddForce(Random.insideUnitCircle.normalized * dropForce, ForceMode2D.Impulse);
        }
    }
}
