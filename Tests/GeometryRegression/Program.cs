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
  Check(AdditionalShapes.Checkerboard(new(0,0,0),new(10000,0,10000)).Count()<=512*512,"geometry span bound");
  Console.WriteLine($"PASS {checks} geometry invariants against actual primitives and game value types");
 }
}
namespace Merthsoft.DesignatorShapes {public static class DesignatorShapes {public static int Thickness=1;public static int Rotation;public static bool FillCorners=true;}}
