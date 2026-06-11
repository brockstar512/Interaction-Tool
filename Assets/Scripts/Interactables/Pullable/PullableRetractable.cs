// Assets/Scripts/Interactable/Pullable/PullableRetractable.cs
namespace IT.Interactables.Pullable
{
    public class PullableRetractable : Pullable
    {
        public override void Release(IInteractionContext context) => Retract();
    }
}
