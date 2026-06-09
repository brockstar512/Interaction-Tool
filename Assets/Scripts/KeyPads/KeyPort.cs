using UnityEngine;

namespace KeyPortSystem
{
    public abstract class KeyPort : MonoBehaviour
    {
        protected virtual Utilities.KeyTypes keyType => Utilities.KeyTypes.None;
        public bool Matches(Utilities.KeyTypes key) => key == keyType;
    }
}