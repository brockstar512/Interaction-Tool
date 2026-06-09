// Assets/Scripts/Interactable/Pullable/PullableRetractable.cs
public class PullableRetractable : Pullable
{
    public override void Release(IInteractionContext context) => Retract();
}