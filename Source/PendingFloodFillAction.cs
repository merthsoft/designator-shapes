using System;

namespace Merthsoft.DesignatorShapes;

// One decision per captured transaction, including repeated dialog callbacks.
public sealed class PendingFloodFillAction
{
    private readonly Func<bool> contextValid;
    private readonly Action apply;
    private bool decided;

    public PendingFloodFillAction(Func<bool> contextValid, Action apply)
    {
        this.contextValid = contextValid;
        this.apply = apply;
    }

    public void Confirm()
    {
        if (decided) return;
        decided = true;
        if (contextValid()) apply();
    }

    public void Cancel() => decided = true;
}
