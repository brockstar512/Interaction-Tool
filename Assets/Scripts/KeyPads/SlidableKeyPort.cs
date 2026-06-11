namespace IT.Interactables.Locks
{
    using IT.Core.Utilities;

    public class SlidableKeyPort : KeyPort
    {
        protected override Utilities.KeyTypes keyType => Utilities.KeyTypes.SlidingBlock;
    }
}

//HoleKeyPort