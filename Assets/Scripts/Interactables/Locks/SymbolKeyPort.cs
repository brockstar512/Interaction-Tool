using UnityEngine;

namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public class SymbolKeyPort : KeyPortBase
    {
        protected override GameUtilities.KeyTypes keyType => GameUtilities.KeyTypes.SymbolSlidingBlock;

        [SerializeField] private string symbol;
        public string Symbol => symbol;
    }
}