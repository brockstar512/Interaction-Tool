using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interactables.Slidable
{
    using IT.Core.Utilities;
    using IT.Interactables.Locks;

    public class SymbolSlidingBlock : SlidableBase
    {
        protected override GameUtilities.KeyTypes key => GameUtilities.KeyTypes.SymbolSlidingBlock;

        [SerializeField] private string symbol;

        protected override bool AcceptsPort(KeyPortBase port)
        {
            if (!base.AcceptsPort(port)) return false;             // must be the right key type
            return port is SymbolKeyPort sp && sp.Symbol == symbol; // and the symbol must match
        }
    }
}
