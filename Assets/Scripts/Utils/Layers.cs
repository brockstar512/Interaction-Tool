using UnityEngine;

/// Cached layer indices. Replaces scattered LayerMask.NameToLayer(Utilities.*Layer) calls.
/// Add a layer here once; every call site reads from this.
namespace IT.Core.Utilities
{
    using IT.Interactables.Locks;
    using IT.Interactables.Slidable;

    public static class Layers
    {
        public static readonly int None                = LayerMask.NameToLayer(Utilities.NoneLayer);
        public static readonly int Interactable         = LayerMask.NameToLayer(Utilities.InteractableLayer);
        public static readonly int Interacting          = LayerMask.NameToLayer(Utilities.InteractingLayer);
        public static readonly int SlidableObstruction  = LayerMask.NameToLayer(Utilities.SlidableObstructionLayer);
        public static readonly int Player               = LayerMask.NameToLayer(Utilities.PlayerLayer);
        public static readonly int KeyPort              = LayerMask.NameToLayer(Utilities.KeyPortLayer);
        public static readonly int TargetOverlap        = LayerMask.NameToLayer(Utilities.TargetOverlapLayer);
        public static readonly int SocketUsed           = LayerMask.NameToLayer(Utilities.SocketUsedLayer);
        public static readonly int SocketUnused         = LayerMask.NameToLayer(Utilities.SocketUnusedLayer);
        public static readonly int Locked               = LayerMask.NameToLayer(Utilities.LockedLayer);
        public static readonly int Obstruction          = LayerMask.NameToLayer(Utilities.ObstructionLayer);
        public static readonly int Depth                = LayerMask.NameToLayer(Utilities.DepthLayer);
    }
}
