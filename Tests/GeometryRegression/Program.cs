using Verse;
using Merthsoft.DesignatorShapes.Shapes;
using State=Merthsoft.DesignatorShapes.DesignatorShapes;
class Program {
 static int checks;
 static void Check(bool ok,string label){checks++;if(!ok)throw new Exception(label);}
 static void Main(){
  for(int w=1;w<=20;w++)for(int h=1;h<=20;h++){
   var s=new IntVec3(10,0,20);var t=new IntVec3(9+w,0,19+h);
   var a=AdditionalShapes.Checkerboard(s,t).ToHashSet();var b=AdditionalShapes.CheckerboardInverse(s,t).ToHashSet();
   Check(!a.Overlaps(b)&&a.Count+b.Count==w*h,"checker phases partition bounds");
   Check(a.SetEquals(AdditionalShapes.Checkerboard(t,s)),"checker reverse bounds same anchor");
   Check(a.All(p=>((p.x-s.x+p.z-s.z)&1)==0),"default checker retains single-cell parity");
   var filled=AdditionalShapes.OctagonFilled(s,t).ToHashSet();var outline=AdditionalShapes.Octagon(s,t).ToHashSet();
   Check(outline.IsSubsetOf(filled),"octagon outline within fill");
   Check(filled.All(p=>filled.Contains(new IntVec3(s.x+t.x-p.x,0,p.z))&&filled.Contains(new IntVec3(p.x,0,s.z+t.z-p.z))),"octagon reflections");
   for(int r=0;r<4;r++){
    State.Rotation=r;
    foreach(var shape in new[]{AdditionalShapes.L(s,t),AdditionalShapes.LInverse(s,t),AdditionalShapes.QuarterArc(s,t),AdditionalShapes.SemicircleArc(s,t)}){
     var cells=shape.ToHashSet();Check(cells.Count>0&&cells.All(p=>p.x>=s.x&&p.x<=t.x&&p.z>=s.z&&p.z<=t.z),"rotated shape inside bounds");
     var reached=new HashSet<IntVec3>();var queue=new Queue<IntVec3>();queue.Enqueue(cells.First());while(queue.Count>0){var p=queue.Dequeue();if(!reached.Add(p))continue;for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++){var n=new IntVec3(p.x+dx,0,p.z+dz);if(cells.Contains(n)&&!reached.Contains(n))queue.Enqueue(n);}}
     Check(reached.SetEquals(cells),"arc/corner connected");
    }
   }
  }
  foreach(int tw in new[]{1,2,3,7,512}) foreach(int th in new[]{1,2,5,512})
  for(int w=1;w<=13;w++) for(int h=1;h<=13;h++) {
   Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.CheckerTileSize=new(tw,0,th);
   var s=new IntVec3(-9,3,-12);var t=new IntVec3(s.x+w-1,3,s.z+h-1);
   var a=AdditionalShapes.Checkerboard(s,t).ToHashSet();var b=AdditionalShapes.CheckerboardInverse(s,t).ToHashSet();
   Check(!a.Overlaps(b)&&a.Count+b.Count==w*h,"sized checker phases partition footprint");
   Check(a.SetEquals(AdditionalShapes.Checkerboard(t,s)),"sized checker reverse drag anchor");
   Check(a.All(p=>p.y==3)&&a.Contains(s),"sized checker preserves plane and first phase");
   for(int z=0;z<h;z++) for(int x=0;x<w;x++)
    Check(a.Contains(new(s.x+x,3,s.z+z))==(((x/tw+z/th)&1)==0),"tile dimensions and clipped edge tiles");
  }
  Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.CheckerTileSize=new(1,0,1);
  for(int radius=0;radius<=80;radius++){
   var center=new IntVec3(80,0,23);var edge=new IntVec3(center.x+radius,0,center.z-radius/2);
   var filled=AdditionalShapes.MidpointOctagonFilled(center,edge).ToHashSet();
   var outline=AdditionalShapes.MidpointOctagon(center,edge).ToHashSet();
   Check(filled.Contains(center),"midpoint octagon contains center");
   Check(outline.IsSubsetOf(filled),"midpoint octagon outline inside fill");
   Check(filled.Min(p=>p.x)==center.x-radius&&filled.Max(p=>p.x)==center.x+radius&&filled.Min(p=>p.z)==center.z-radius&&filled.Max(p=>p.z)==center.z+radius,"midpoint radius and exact center");
   Check(filled.All(p=>filled.Contains(new IntVec3(2*center.x-p.x,0,p.z))&&filled.Contains(new IntVec3(p.x,0,2*center.z-p.z))),"midpoint reflection symmetry");
   Check(filled.SetEquals(AdditionalShapes.MidpointOctagonFilled(center,new(center.x-radius/2,0,center.z+radius))),"midpoint radius independent of drag quadrant");
   Check(filled.SetEquals(AdditionalShapes.OctagonFilled(new(center.x-radius,0,center.z-radius),new(center.x+radius,0,center.z+radius))),"midpoint matches corner-box counterpart");
  }
  var limited=AdditionalShapes.MidpointOctagonFilled(new(0,0,0),new(10000,0,-10000)).ToHashSet();
  Check(limited.Min(p=>p.x)==-255&&limited.Max(p=>p.x)==255&&limited.Count<=511*511,"midpoint cap preserves symmetric odd diameter");
  Check(AdditionalShapes.Checkerboard(new(0,0,0),new(10000,0,10000)).Count()<=512*512,"geometry span bound");
  for(int w=1;w<=12;w++)for(int h=1;h<=12;h++)for(int xs=1;xs<=7;xs++)for(int zs=1;zs<=7;zs++)foreach(bool inverse in new[]{false,true}){
   var s=new IntVec3(-7,0,-11);var t=new IntVec3(s.x+w-1,0,s.z+h-1);
   var integrated=(inverse?Primitives.InverseGrid(s,t,xs,zs,1):Primitives.Grid(s,t,xs,zs,1)).ToHashSet();
   var extracted=Merthsoft.DesignatorShapes.Grids.Geometry.Draw(t,s,xs,zs,inverse).ToHashSet();
   Check(integrated.SetEquals(extracted),"standalone grid extraction preserves original anchoring and edges");
  }
  Console.WriteLine($"PASS {checks} geometry invariants against actual primitives and game value types");
 }
}
namespace Merthsoft.DesignatorShapes {public static class DesignatorShapes {public static int Thickness=1;public static int Rotation;public static bool FillCorners=true;}}

namespace Merthsoft.DesignatorShapes.Ui { public static class ShapeControlsWindow { public static IntVec3 CheckerTileSize=new(1,0,1); } }
