// Assets/Scripts/KeyPads/KeyPort.cs
// Source. Free to extend DependencySource<bool> because it had no other base.
using UnityEngine;

namespace KeySystem
{
    public class KeyPort : DependencySource<bool>
    {
        [SerializeField] private Utilities.KeyTypes keyPort;

        public bool Lock(Utilities.KeyTypes keyType)
        {
            bool matched = keyType == keyPort;
            if (matched) Value = true;   // protected setter on the base
            return matched;
        }
    }
}