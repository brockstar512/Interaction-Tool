using Items;
using UnityEngine;

public class GiveItemEffect : MonoBehaviour, IOpenEffect
{
    [SerializeField] private Item item;          // what this chest holds
    public void OnOpen(IInteractionContext context)
    {
        context.Items.PickUp(item);              // the one capability tweak below
    }
}