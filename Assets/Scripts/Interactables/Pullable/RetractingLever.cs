// Assets/Scripts/Interactable/PullableBase/RetractingLever.cs
namespace IT.Interactables.Pullable
{
    public class RetractingLever : PullableBase
    {
        public override void Release(IInteractionContext context) => Retract();
    }
}
