using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes
{
    [StaticConstructorOnStartup]
    public static class Icons
    {
        public static Texture2D Undo { get; private set; }

        public static Texture2D Redo { get; private set; }

        public static Texture2D UndoEnabled { get; private set; }

        public static Texture2D RedoEnabled { get; private set; }

        public static Texture2D UndoDisabled { get; private set; }

        public static Texture2D RedoDisabled { get; private set; }

        public static Texture2D GetIcon(string path) => ContentFinder<Texture2D>.Get(path, true);

        static Icons() => LongEventHandler.ExecuteWhenFinished(() =>
        {
            Undo = GetIcon("UI/Commands/undo");
            Redo = GetIcon("UI/Commands/redo");

            UndoEnabled = GetIcon("UI/Commands/Smol/undo");
            RedoEnabled = GetIcon("UI/Commands/Smol/redo");

            UndoDisabled = GetIcon("UI/Commands/Smol/undo_disabled");
            RedoDisabled = GetIcon("UI/Commands/Smol/redo_disabled");
        });
    }
}
