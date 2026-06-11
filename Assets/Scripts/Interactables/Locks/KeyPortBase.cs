using UnityEngine;

namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public abstract class KeyPortBase : MonoBehaviour
    {
        protected virtual GameUtilities.KeyTypes keyType => GameUtilities.KeyTypes.None;
        public bool Matches(GameUtilities.KeyTypes key) => key == keyType;
    }
}