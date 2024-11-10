using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
    public abstract class Item : ScriptableObject, IItem
    {

        [SerializeField] Sprite sprite;
        protected Action<InteractableBase> ItemFinishedCallback;
        public Sprite Sprite => sprite;

        public abstract void Use(PlayerStateMachineManager stateManager);

        public abstract void PutAway();



    }
}

