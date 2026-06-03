using UnityEngine;

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

    private void Open(IInteractionContext context)
    {
        _isOpen = true;
        OpenAnimation();
        _effects ??= GetComponents<IOpenEffect>();   // every effect on this object
        foreach (var effect in _effects)
            effect.OnOpen(context);
    }

    protected virtual void OpenAnimation() { }
    // CorrectKey / TryUseKey as you already have them
}