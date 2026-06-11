using System;


namespace IT.Items
{
    using IT.Interactables;

    public class PlankItem : ItemBase
    {
        Action _disposeOfItem = null;

        public override void Use(IInteractionContext context)
        {
            ItemFinishedCallback = context.EndInteraction;
            _disposeOfItem = context.Items.DisposeOfCurrentItem;
            Action();
        }
        
        void Action()
        {
            PutAway();
        }
        
        public override void PutAway()
        {
            _disposeOfItem?.Invoke();
            ItemFinishedCallback?.Invoke(null);
        }
    }
}
    
