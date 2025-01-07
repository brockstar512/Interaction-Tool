using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
    public abstract class Item : ScriptableObject, IItem
    {
        //determines if player can walk when using the item
        [SerializeField] bool canWalk;
        //serializes the item for the pickup holder and HUD
        [SerializeField] Sprite sprite;
        //helps switch states after items is used
        protected Action<InteractableBase> ItemFinishedCallback;

        public bool CanWalk =>canWalk;
        public Sprite Sprite => sprite;
        public abstract void Use(PlayerStateMachineManager stateManager);

        public abstract void PutAway();



    }
}

