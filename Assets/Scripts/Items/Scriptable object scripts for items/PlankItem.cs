using System;


namespace Items.Scriptable_object_scripts_for_items
{
    public class PlankItem : Item
    {
        Action _disposeOfItem = null;

        public override void Use(PlayerStateMachineManager stateManager)
        {
            ItemFinishedCallback = stateManager.SwitchStateFromEquippedItem;
            _disposeOfItem = stateManager.itemManager.DisposeOfCurrentItem;

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
    
