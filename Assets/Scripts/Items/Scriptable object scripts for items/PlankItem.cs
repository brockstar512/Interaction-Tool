using System;


namespace Items.Scriptable_object_scripts_for_items
{
    public class PlankItem : Item
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
    
