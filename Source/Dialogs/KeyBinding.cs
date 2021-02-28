using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Dialogs
{
    internal class KeyBinding : Window
    {
        protected Vector2 windowSize = new(400f, 200f);

        public override Vector2 InitialSize => windowSize;

        protected int KeyIndex { get; }

        public KeyBinding(int keyIndex)
            => KeyIndex = keyIndex;

        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(inRect, "PressAnyKeyOrEsc".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
            {
                if (Event.current.keyCode != KeyCode.Escape)
                    DesignatorShapes.Settings.Keys[KeyIndex] = Event.current.keyCode;
                Close(true);
                Event.current.Use();
            }
        }
    }
}
