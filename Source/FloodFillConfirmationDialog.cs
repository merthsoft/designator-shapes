using System;
using Verse;
namespace Merthsoft.DesignatorShapes;
// Window.PreOpen deselects tools on Dialog layer. Preserve base layout/search setup
// without its selection reset, then restore Dialog before insertion and rendering.
public sealed class FloodFillConfirmationDialog : Dialog_MessageBox
{
    public FloodFillConfirmationDialog(TaggedString text, Action confirm, Action cancel)
        : base(text, buttonAText: "Merthsoft_DesignatorShapes_ConfirmApply".Translate(), buttonAAction: confirm,
            buttonBText: "Merthsoft_DesignatorShapes_ConfirmKeep".Translate(), buttonBAction: cancel,
            buttonADestructive: true, cancelAction: cancel)
    { }
    public override void PreOpen()
    {
        var original = layer;
        try { layer = WindowLayer.GameUI; base.PreOpen(); }
        finally { layer = original; }
    }
}
