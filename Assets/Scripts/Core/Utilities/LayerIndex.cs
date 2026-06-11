using UnityEngine;

/// Cached layer indices. Replaces scattered LayerMask.NameToLayer(GameUtilities.*Layer) calls.
/// Add a layer here once; every call site reads from this.
namespace IT.Core.Utilities
{
    public static class LayerIndex
    {
        public static readonly int None                = LayerMask.NameToLayer(GameUtilities.NoneLayer);
        public static readonly int Interactable         = LayerMask.NameToLayer(GameUtilities.InteractableLayer);
        public static readonly int Interacting          = LayerMask.NameToLayer(GameUtilities.InteractingLayer);
        public static readonly int SlidableObstruction  = LayerMask.NameToLayer(GameUtilities.SlidableObstructionLayer);
        public static readonly int Player               = LayerMask.NameToLayer(GameUtilities.PlayerLayer);
        public static readonly int KeyPort               = LayerMask.NameToLayer(GameUtilities.KeyPortLayer);
        public static readonly int TargetOverlap        = LayerMask.NameToLayer(GameUtilities.TargetOverlapLayer);
        public static readonly int SocketUsed           = LayerMask.NameToLayer(GameUtilities.SocketUsedLayer);
        public static readonly int SocketUnused         = LayerMask.NameToLayer(GameUtilities.SocketUnusedLayer);
        public static readonly int Locked               = LayerMask.NameToLayer(GameUtilities.LockedLayer);
        public static readonly int Obstruction           = LayerMask.NameToLayer(GameUtilities.ObstructionLayer);
        public static readonly int Depth                = LayerMask.NameToLayer(GameUtilities.DepthLayer);
    }
}
