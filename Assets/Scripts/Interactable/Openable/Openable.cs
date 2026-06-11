using UnityEngine;

namespace IT.Interactables.Doors
{
    using IT.Core.Utilities;
    using IT.Items;

    public abstract class Openable : InteractableBase
    {
        [SerializeField] private Utilities.KeyTypes key;
        private bool _isOpen;
        private IOpenEffect[] _effects;

        public override InteractionKind Kind => InteractionKind.Open;

        public override bool Interact(IInteractionContext context)
        {
            if (_isOpen) return false;
            if (key != Utilities.KeyTypes.None && !TryUseKey(context)) return false;
            Open(context);
            return true;
        }

        public override void Release(IInteractionContext context) { }

        private void Open(IInteractionContext context)
        {
            _isOpen = true;
            OpenAnimation();
            _effects ??= GetComponents<IOpenEffect>();
            foreach (var effect in _effects)
                effect.OnOpen(context);
        }

        private bool TryUseKey(IInteractionContext context)
        {
            if (!CorrectKey(context.Items.GetItem())) return false;
            context.Items.DisposeOfCurrentItem();   // right key → consume it
            return true;
        }

        private bool CorrectKey(IItem item) => item is Key keyItem && keyItem.keyType == key;

        protected virtual void OpenAnimation() { }
    }
}
