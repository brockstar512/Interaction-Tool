using System;
using UnityEngine;

namespace IT.Core
{
    [Serializable]
    public class InterfaceReference<TInterface> where TInterface : class
    {
        [SerializeField] private UnityEngine.Object target;

        public TInterface Value
        {
            get => target as TInterface;
            set => target = value as UnityEngine.Object;
        }

        public UnityEngine.Object UnderlyingObject => target;
    }
}
