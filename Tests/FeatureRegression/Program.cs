using System.Reflection;
using System.Runtime.CompilerServices;
using Merthsoft.DesignatorShapes;
using Merthsoft.DesignatorShapes.Shapes;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using Verse;
using UnityEngine;
class Program{
 static int checks;
 static void Check(bool ok,string text){checks++;if(!ok)throw new Exception(text);}
 static void Main(){
  var stroke=new FreeformStroke();stroke.Add(new(80,0,20),200,100,1);
  Check(stroke.Cells.SequenceEqual(new[]{new IntVec3(80,0,20)}),"asymmetric initial click");
  stroke.Add(new(84,0,20),200,100,1);
  Check(stroke.Cells.Count()==5&&stroke.Cells.Contains(new(84,0,20)),"first endpoint retained");
  stroke.Clear();stroke.Add(new(40,0,50),100,100,5);stroke.Add(new(45,0,55),100,100,5);
  Check(stroke.Cells.Min(p=>p.x)==38&&stroke.Cells.Max(p=>p.x)==47&&stroke.Cells.Min(p=>p.z)==48&&stroke.Cells.Max(p=>p.z)==57,"thick diagonal correctly centered");
  var count=stroke.Cells.Count();for(int i=0;i<1000;i++)stroke.Add(new(45,0,55),100,100,5);
  Check(stroke.Cells.Count()==count&&stroke.SampleCount==6,"repeated samples do not grow state");
  stroke.Add(new(45,0,55),100,100,1);Check(stroke.Cells.Count()==6,"thickness change restamps entire path");
  stroke.Clear();stroke.Add(new(40,0,50),100,100,1);stroke.Add(new(41,0,50),100,100,1);
  Check(stroke.Cells.Count()==2&&!stroke.Cells.Contains(new(45,0,55)),"same-origin fresh stroke has no stale cells");
  stroke.Add(new(-100,0,1000),100,100,50);Check(stroke.Cells.All(p=>p.x>=0&&p.z>=0&&p.x<100&&p.z<100),"brush bounded to map");
  var asm=typeof(FreeformStroke).Assembly;var wrapper=typeof(FreeformLine);var flags=BindingFlags.Static|BindingFlags.NonPublic;
  foreach(var hook in new[]{("StartFreeformStroke","Prefix"),("EndFreeformStroke","Postfix"),("ChangeFreeformStyle","Prefix"),("NewFreeformGame","Prefix"),("LoadFreeformGame","Prefix")}){
   var stored=(FreeformStroke)wrapper.GetField("stroke",flags)!.GetValue(null)!;stored.Add(new(5,0,7),20,20,3);
   asm.GetType("Merthsoft.DesignatorShapes.Patches."+hook.Item1)!.GetMethod(hook.Item2,flags)!.Invoke(null,null);
   Check(!stored.Cells.Any(),"production lifecycle reset "+hook.Item1);
  }
  var mapContext=new object();var toolContext=new object();var styleContext=new object();var point=new IntVec3(5,0,7);
  stroke.Clear();Check(stroke.BeginContext(mapContext,toolContext,styleContext,point),"context begins");stroke.Add(point,30,30,3);
  Check(!stroke.BeginContext(mapContext,toolContext,styleContext,point)&&stroke.Cells.Any(),"same context retains stroke");
  Check(stroke.BeginContext(new object(),toolContext,styleContext,point)&&!stroke.Cells.Any(),"map change resets path");stroke.Add(point,30,30,3);
  Check(stroke.BeginContext(mapContext,new object(),styleContext,point)&&!stroke.Cells.Any(),"tool change resets path");stroke.Add(point,30,30,3);
  Check(stroke.BeginContext(mapContext,toolContext,new object(),point)&&!stroke.Cells.Any(),"style change resets path");stroke.Add(point,30,30,3);
  Check(stroke.BeginContext(mapContext,toolContext,styleContext,new(6,0,7))&&!stroke.Cells.Any(),"origin change resets path");
  int mutations=0;var pending=new PendingFloodFillAction(()=>true,()=>mutations++);pending.Cancel();pending.Confirm();Check(mutations==0,"cancel atomic");
  pending=new(()=>true,()=>mutations++);pending.Confirm();pending.Confirm();Check(mutations==1,"confirm exactly once");
  pending=new(()=>false,()=>mutations++);pending.Confirm();Check(mutations==1,"changed context cannot apply");
  var store=new Dictionary<string,string>();for(int i=0;i<1000;i++)RememberedShapeStore.Put(store,"def:"+i,"Rectangle");
  Check(store.Count==512,"memory bounded");RememberedShapeStore.Put(store,"def:0","Line");Check(store["def:0"]=="Line","existing memory remains updateable at capacity");
  Check(RememberedShapeStore.Key(typeof(Designator_Build),"Wall")!=RememberedShapeStore.Key(typeof(Designator_Build),"Floor"),"build defs have distinct memory");
  var memory=asm.GetType("Merthsoft.DesignatorShapes.ShapeMemory")!;
  var emptyBuild=(Designator_Build)RuntimeHelpers.GetUninitializedObject(typeof(Designator_Build));
  Check(memory.GetMethod("KeyFor",flags)!.Invoke(null,new object[]{emptyBuild})!=null,"null placing def is safe");
  Check(!ShapeEligibility.IsDrawable(new(){defName="Undo",drawMethodName="Merthsoft.DesignatorShapes.HistoryManager.Undo"}),"undo not drawable");
  Check(!ShapeEligibility.IsDrawable(new(){defName="Redo",drawMethodName="Merthsoft.DesignatorShapes.HistoryManager.Redo"}),"redo not drawable");
  Check(ShapeEligibility.IsDrawable(new(){defName="Checkerboard",drawMethodName="Merthsoft.DesignatorShapes.Shapes.AdditionalShapes.Checkerboard"}),"real geometry delegate eligible");
  var nativeFill=typeof(Merthsoft.DesignatorShapes.FloodFill.Fill);
  foreach(bool show in new[]{false,true}){Check(!InputOwnership.CustomOwnsRoute(show,nativeFill),"native standalone owns fill irrespective of panel");Check(InputOwnership.IsStandaloneFill(nativeFill),"actual standalone worker recognized");}
  Check(InputOwnership.CustomOwnsRoute(true,typeof(DrawStyle_FilledRectangle)),"custom geometry owns ordinary style when expanded");
  Check(!InputOwnership.CustomOwnsRoute(false,typeof(DrawStyle_FilledRectangle)),"collapsed geometry yields");
  var classifier=asm.GetType("Merthsoft.DesignatorShapes.FloodFillConfirmation")!.GetMethod("IsDestructive",flags)!;
  foreach(var type in new[]{typeof(Designator_Cancel),typeof(Designator_Plan_Remove),typeof(Designator_MineVein),typeof(Designator_DeconstructConduit),typeof(Designator_PlantsCut),typeof(Designator_RemoveFloor)})Check((bool)classifier.Invoke(null,new[]{RuntimeHelpers.GetUninitializedObject(type)})!,"actual destructive type "+type.Name);
  foreach(var type in new[]{typeof(Designator_PlantsHarvest),typeof(Designator_PlantsHarvestWood),typeof(Designator_Build)})Check(!(bool)classifier.Invoke(null,new[]{RuntimeHelpers.GetUninitializedObject(type)})!,"non-destructive type excluded "+type.Name);
  var settings=new DesignatorSettings();Check(!settings.RememberShapePerDesignator&&!settings.CancelStrokeKeepsTool&&!settings.EnableCustomFloodFillBarriers&&!settings.ConfirmDestructiveFloodFill&&!settings.RestoreAltToggle,"new options default off");
  settings.HideCompletelyOnAltToggle=true;settings.RestoreAltToggle=true;settings.InterfaceToggleKey=KeyCode.None;Check(!settings.HideCollapsedInterface,"unbound toggle cannot hide recovery bar");
  Console.WriteLine($"PASS {checks} feature checks using production DLL and actual game types (no in-game GUI test)");
 }
}
