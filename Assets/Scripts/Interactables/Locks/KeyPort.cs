using UnityEngine;

namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public abstract class KeyPort : MonoBehaviour
    {
        protected virtual Utilities.KeyTypes keyType => Utilities.KeyTypes.None;
        public bool Matches(Utilities.KeyTypes key) => key == keyType;
    }
}