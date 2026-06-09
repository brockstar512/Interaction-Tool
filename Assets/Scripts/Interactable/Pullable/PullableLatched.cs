
public class PullableLatched : Pullable
{
    private bool _locked = false;
    private bool IsFullyPulled() => _distance >= maxDistance - 0.001f;

    
    public override bool Interact(IInteractionContext context)
    {
        if (_locked) return false;
        
        return base.Interact(context);
    }
    
    public override float Pull(float requested)
    {
        if (_locked) return 0f;
        return base.Pull(requested);
    }
    
    public override void Release(IInteractionContext context)
    {
         if (_locked) return;
         if (IsFullyPulled())
         {
             // only locks if pulled all the way
             _locked = true;
             return;
         }

         base.Release(context);
    }
}
