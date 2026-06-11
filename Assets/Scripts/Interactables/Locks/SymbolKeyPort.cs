using UnityEngine;

namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public class SymbolKeyPort : KeyPort
    {
        protected override Utilities.KeyTypes keyType => Utilities.KeyTypes.SymbolSlidingBlock;

        [SerializeField] private string symbol;
        public string Symbol => symbol;
    }
}