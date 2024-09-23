using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
    public abstract class Item : ScriptableObject, IItem
    {
        //could use a factory to generate a concret class rather than pass around scriptable objects
        [SerializeField] Sprite sprite;
        protected Action<DefaultState> ItemFinishedCallback;
        protected DefaultState DefaultState = null;
        public Sprite Sprite => sprite;

        public abstract void Use(PlayerStateMachineManager stateManager, Action<DefaultState> callbackAction,
            DefaultState defaultStateArg);

        public abstract void PutAway();



    }
}

