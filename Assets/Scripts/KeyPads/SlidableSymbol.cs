using System.Collections;
using System.Collections.Generic;
using KeyPortSystem;
using UnityEngine;

public class SlidableSymbol : Slidable
{
    protected override Utilities.KeyTypes key => Utilities.KeyTypes.SymbolSlidingBlock;

    [SerializeField] private string symbol;

    protected override bool AcceptsPort(KeyPort port)
    {
        if (!base.AcceptsPort(port)) return false;             // must be the right key type
        return port is SymbolKeyPort sp && sp.Symbol == symbol; // and the symbol must match
    }
}
