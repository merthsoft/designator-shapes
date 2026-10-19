using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

public static class CustomPolygon
{
    private static readonly PolygonStroke stroke = new();
    private static readonly List<IntVec3> preview = new();
    private static Map map;
    private static Designator tool;
    private static DesignatorShapeDef shape;
    private static DrawStyleDef style;
    private static int previewVersion = -1, previewThickness;
    private static bool previewCorners;
    private static IntVec3 previewCursor;
    private static DesignatorShapeDef ActiveShape => Find.UIRoot is not UIRoot_Play ? null :
        DesignatorShapes.UseCustomGeometry ? DesignatorShapes.CurrentTool :
        !DesignatorShapes.ShowControls ? PaletteShapeWorker.SelectedShape : null;
    private static bool IsPolygon(DesignatorShapeDef def) => def?.defName is "CustomPolygon" or "CustomPolygonFilled";
    internal static bool Committing { get; private set; }
    private static bool Filled => shape?.defName == "CustomPolygonFilled";
    internal static void Clear() { stroke.Clear();preview.Clear();previewVersion=-1; }
    private static bool EnsureContext()
    {
        var active=ActiveShape;
        if(!IsPolygon(active)||Find.CurrentMap==null||WorldRendererUtility.WorldSelected)
        { Clear();return false; }
        var manager=Find.DesignatorManager;
        if(manager.SelectedDesignator==null||!manager.SelectedDesignator.CanRemainSelected()){Clear();return false;}
        if(map!=Find.CurrentMap||tool!=manager.SelectedDesignator||shape!=active||style!=manager.SelectedStyle)
        {Clear();map=Find.CurrentMap;tool=manager.SelectedDesignator;shape=active;style=manager.SelectedStyle;}
        return true;
    }
    public static IEnumerable<IntVec3> Outline(IntVec3 s,IntVec3 t)
        => stroke.Preview(t,false,DesignatorShapes.Thickness,DesignatorShapes.FillCorners);
    public static IEnumerable<IntVec3> FilledPolygon(IntVec3 s,IntVec3 t)
        => stroke.Preview(t,true,1,DesignatorShapes.FillCorners);
    internal static bool HandleKeyboard()
    {
        var e=Event.current;
        if(e?.type!=EventType.KeyDown||!EnsureContext()||ShapeShortcuts.InputBlocked)return true;
        if(e.control||e.alt||Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl)
            ||Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt))return true;
        if(e.keyCode==KeyCode.Escape)Clear();
        else if(e.keyCode==KeyCode.Backspace)stroke.RemoveLast();
        else if(e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)Commit();
        else return true;
        e.Use();return false;
    }
    internal static bool BeforeInput(DesignatorManager manager)
    {
        if(!EnsureContext()||ShapeShortcuts.InputBlocked)return true;
        if(!HandleKeyboard())return false;
        var e=Event.current;if(e==null)return true;
        if(e.type==EventType.MouseDown&&e.button==1){Clear();e.Use();return false;}
        if(e.type==EventType.MouseDown&&e.button==0)
        {
            var cell=UI.MouseCell();
            if(cell.InBounds(map))
            {
                if(stroke.CanFinish&&cell==stroke.Vertices[0])Commit();
                else if(!stroke.TryAdd(cell)&&!stroke.Vertices.Contains(cell))
                    Messages.Message("Merthsoft_DesignatorShapes_PolygonLimit".Translate(),MessageTypeDefOf.RejectInput,false);
            }
            e.Use();return false;
        }
        if(e.button==0&&e.type is EventType.MouseUp or EventType.MouseDrag){e.Use();return false;}
        return true;
    }
    private static void Commit()
    {
        if(!stroke.CanFinish)
        {Messages.Message("Merthsoft_DesignatorShapes_PolygonNeedCorners".Translate(),MessageTypeDefOf.RejectInput,false);return;}
        var cells=stroke.Finish(Filled,DesignatorShapes.Thickness,DesignatorShapes.FillCorners)
            .Where(p=>p.InBounds(map)&&tool.CanDesignateCell(p).Accepted).ToArray();
        Clear();
        if(cells.Length==0)return;
        HistoryManager.StartBuilding();Committing=true;
        try{tool.DesignateMultiCell(cells);}
        finally{Committing=false;HistoryManager.FinishBuilding();}
    }
    internal static bool UpdatePreview()
    {
        if(!EnsureContext())return true;
        if(ShapeShortcuts.InputBlocked)return false;
        var cursor=UI.MouseCell();
        if(previewVersion!=stroke.Version||previewCursor!=cursor||previewThickness!=DesignatorShapes.Thickness||previewCorners!=DesignatorShapes.FillCorners)
        {
            preview.Clear();preview.AddRange(stroke.Preview(cursor,Filled,DesignatorShapes.Thickness,DesignatorShapes.FillCorners));
            previewVersion=stroke.Version;previewCursor=cursor;previewThickness=DesignatorShapes.Thickness;previewCorners=DesignatorShapes.FillCorners;
        }
        tool.RenderHighlight(preview.Where(p=>p.InBounds(map)&&tool.CanDesignateCell(p).Accepted).ToList());
        if(stroke.Vertices.Count>0)
        {
            GenDraw.DrawFieldEdges(stroke.Vertices.ToList(),Color.cyan);
            GenDraw.DrawFieldEdges(new List<IntVec3>{stroke.Vertices[0]},Color.green);
        }
        return false;
    }
    internal static void DrawControls()
    {
        if(!EnsureContext()||ShapeShortcuts.InputBlocked)return;
        var mouse=UI.MousePositionOnUIInverted;
        Widgets.Label(new Rect(Math.Min(mouse.x+20,UI.screenWidth-370),Math.Min(mouse.y+24,UI.screenHeight-65),360,60),
            "Merthsoft_DesignatorShapes_PolygonControls".Translate(stroke.Vertices.Count));
    }
}

[HarmonyPatch(typeof(DesignationDragger),"DraggerUpdate")]
internal static class CustomPolygonPreview { static bool Prefix()=>CustomPolygon.UpdatePreview(); }
