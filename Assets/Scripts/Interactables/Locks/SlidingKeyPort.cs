namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public class SlidingKeyPort : KeyPortBase
    {
        protected override GameUtilities.KeyTypes keyType => GameUtilities.KeyTypes.SlidingBlock;
    }
}

//HoleKeyPort