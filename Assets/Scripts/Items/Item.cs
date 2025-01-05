using System;
using UnityEngine;

namespace Items
{
    public abstract class Item : MonoBehaviour, IItem
    {

        [SerializeField] Sprite sprite;
        
        protected Action<InteractableBase> ItemFinishedCallback;
        public Sprite Sprite => sprite;
        public abstract void Use(PlayerStateMachineManager stateManager);
        public abstract void PutAway();
        
    }
}

