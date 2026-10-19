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
   Check(a.Select(p=>new IntVec3(s.x+t.x-p.x,p.y,s.z+t.z-p.z)).ToHashSet().SetEquals(AdditionalShapes.Checkerboard(t,s)),"checker reversed drag mirrors around its own start");
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
   Check(a.Select(p=>new IntVec3(s.x+t.x-p.x,p.y,s.z+t.z-p.z)).ToHashSet().SetEquals(AdditionalShapes.Checkerboard(t,s)),"sized checker reversed drag mirrors around its own start");
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
  // Grid phase follows the first click; extending any edge cannot shift existing cells.
  for(int w=1;w<=12;w++)for(int h=1;h<=12;h++)for(int xs=1;xs<=7;xs++)for(int zs=1;zs<=7;zs++)
   foreach(int dx in new[]{-1,1})foreach(int dz in new[]{-1,1}) {
    var s=new IntVec3(-7,3,-11);var t=new IntVec3(s.x+dx*(w-1),3,s.z+dz*(h-1));
    var grid=Primitives.Grid(s,t,xs,zs,1).ToHashSet();
    var inverse=Primitives.InverseGrid(s,t,xs,zs,1).ToHashSet();
    Check(grid.Contains(s)&&!inverse.Contains(s),"first click is grid intersection / inverse channel");
    Check(!grid.Overlaps(inverse)&&grid.Count+inverse.Count==w*h,"grid and inverse partition bounds at thickness 1");
    for(int x=0;x<w;x++)for(int z=0;z<h;z++) {
     var cell=new IntVec3(s.x+dx*x,3,s.z+dz*z);
     Check(grid.Contains(cell)==(x%xs==0||z%zs==0),"grid spacing from first click in all quadrants");
    }
    var extendedGrid=Primitives.Grid(s,new(t.x+dx*2,3,t.z+dz*2),xs,zs,1).ToHashSet();
    Check(grid.SetEquals(extendedGrid.Where(p=>Math.Abs(p.x-s.x)<w&&Math.Abs(p.z-s.z)<h)),"grid phase remains stable as drag grows");
    var extended=Primitives.InverseGrid(s,new(t.x+dx*2,3,t.z+dz*2),xs,zs,1).ToHashSet();
    Check(inverse.SetEquals(extended.Where(p=>Math.Abs(p.x-s.x)<w&&Math.Abs(p.z-s.z)<h)),"inverse partial edge blocks remain stable as drag grows");
   }
  foreach(int thickness in new[]{2,3})foreach(int dx in new[]{-1,1})foreach(int dz in new[]{-1,1}) {
   var s=new IntVec3(-3,2,-9);var t=new IntVec3(s.x+dx*11,2,s.z+dz*9);
   var cells=Primitives.InverseGrid(s,t,5,4,thickness).ToHashSet();
   for(int x=0;x<=11;x++)for(int z=0;z<=9;z++)
    Check(cells.Contains(new(s.x+dx*x,2,s.z+dz*z))==(x%5>=thickness&&z%4>=thickness),"inverse channel thickness follows drag direction");
  }
  // Parallel stripes: all quadrants, partial bands, empty gaps and single-axis drags.
  for(int w=1;w<=12;w++)for(int h=1;h<=12;h++)
   for(int thickness=1;thickness<=4;thickness++)for(int skip=0;skip<=4;skip++)
    foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1})foreach(bool horizontal in new[]{false,true}){
     var start=new IntVec3(7,3,-9);var end=new IntVec3(start.x+sx*(w-1),3,start.z+sz*(h-1));
     var drawn=Primitives.ParallelLines(start,end,thickness,skip,horizontal).ToList();
     var expected=new HashSet<IntVec3>();
     for(int z=0;z<h;z++)for(int x=0;x<w;x++)
      if((horizontal?z:x)%(thickness+skip)<thickness)expected.Add(new(start.x+sx*x,3,start.z+sz*z));
     Check(drawn.Count==expected.Count&&expected.SetEquals(drawn),"parallel stripe width, gaps, bounds, plane and no duplicates");
     Check(drawn.Contains(start),"parallel stripes always begin at drag start");
     var extended=Primitives.ParallelLines(start,new(start.x+sx*(w+2),3,start.z+sz*(h+2)),thickness,skip,horizontal).ToHashSet();
     Check(drawn.All(extended.Contains),"extending bounds preserves stripe anchor");
    }
  Check(Primitives.ParallelLinesHorizontal(new(0,0,0),new(8,0,2),false),"wide drag chooses horizontal");
  Check(!Primitives.ParallelLinesHorizontal(new(0,0,0),new(-2,0,-8),true),"tall reverse drag chooses vertical");
  foreach(bool previous in new[]{false,true})
   Check(Primitives.ParallelLinesHorizontal(new(0,0,0),new(-5,0,5),previous)==previous,"square drag retains last direction");
  Check(Primitives.ParallelLines(new(0,0,0),new(10000,0,10000),1,0,true).Count()==512*512,"parallel span bounded");
  Check(Primitives.ParallelLines(new(0,0,0),new(4,0,4),0,-1,true).Count()==25,"invalid stripe values clamp without division by zero");
  Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.ParallelLineSize=new(2,0,3);
  State.Rotation=0;
  var origin=new IntVec3(0,0,0);var wide=new IntVec3(9,0,6);var tall=new IntVec3(6,0,9);var square=new IntVec3(6,0,6);
  Check(BasicShapes.ParallelLines(origin,wide).ToHashSet().SetEquals(Primitives.ParallelLines(origin,wide,2,3,true)),"production wrapper reads independent stripe inputs");
  Check(BasicShapes.ParallelLines(origin,square).ToHashSet().SetEquals(Primitives.ParallelLines(origin,square,2,3,true)),"production wrapper retains horizontal tie");
  State.Rotation=1;
  Check(BasicShapes.ParallelLines(origin,wide).ToHashSet().SetEquals(Primitives.ParallelLines(origin,wide,2,3,false)),"rotation swaps horizontal stripes to vertical");
  Check(BasicShapes.ParallelLines(origin,tall).ToHashSet().SetEquals(Primitives.ParallelLines(origin,tall,2,3,true)),"rotation swaps vertical stripes to horizontal");
  State.Rotation=0;
  Check(BasicShapes.ParallelLines(origin,square).ToHashSet().SetEquals(Primitives.ParallelLines(origin,square,2,3,false)),"production wrapper retains vertical tie");
  for(int w=1;w<=12;w++)for(int h=1;h<=12;h++)
   foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1})
    foreach(int tileWidth in new[]{1,2,3})foreach(int tileHeight in new[]{1,2,4}){
     Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.CheckerTileSize=new(tileWidth,0,tileHeight);
     var start=new IntVec3(40,3,60);var end=new IntVec3(start.x+sx*(w-1),3,start.z+sz*(h-1));
     var selected=AdditionalShapes.Checkerboard(start,end).ToHashSet();
     var skipped=AdditionalShapes.CheckerboardInverse(start,end).ToHashSet();
     Check(selected.Contains(start)&&!skipped.Contains(start),"first clicked cell selects checker phase in every quadrant");
     Check(!selected.Overlaps(skipped)&&selected.Count+skipped.Count==w*h,"both phases partition reverse bounds");
     for(int z=0;z<h;z++)for(int x=0;x<w;x++){
      var cell=new IntVec3(start.x+sx*x,3,start.z+sz*z);
      Check(selected.Contains(cell)==(((x/tileWidth+z/tileHeight)&1)==0),"tile boundaries measured from first click");
     }
     var extended=AdditionalShapes.Checkerboard(start,new(start.x+sx*(w+2),3,start.z+sz*(h+2))).ToHashSet();
     Check(selected.All(extended.Contains),"extending drag preserves existing checker pattern");
    }
  Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.CheckerTileSize=new(1,0,1);
  var reverseLimit=AdditionalShapes.Checkerboard(new(1000,0,1000),new(-10000,0,-10000)).ToHashSet();
  Check(reverseLimit.Contains(new(1000,0,1000))&&reverseLimit.All(p=>p.x>=489&&p.z>=489),"checker span cap remains anchored to first click");
  State.Thickness=1;
  for(int width=3;width<=40;width++)for(int height=3;height<=40;height++){
   var start=new IntVec3(-12,3,7);var end=new IntVec3(start.x+width-1,3,start.z+height-1);
   State.FillCorners=false;var thin=AdditionalShapes.Octagon(start,end).ToHashSet();
   State.FillCorners=true;var joined=AdditionalShapes.Octagon(start,end).ToHashSet();
   Check(joined.SetEquals(joined.Select(p=>new IntVec3(start.x+end.x-p.x,p.y,p.z))),"octagon corner filling is horizontally symmetric");
   Check(joined.SetEquals(joined.Select(p=>new IntVec3(p.x,p.y,start.z+end.z-p.z))),"octagon corner filling is vertically symmetric");
   Check(thin.IsSubsetOf(joined),"corner filling preserves the thin outline");
   var reached=new HashSet<IntVec3>();var queue=new Queue<IntVec3>();queue.Enqueue(joined.First());
   while(queue.Count>0){var p=queue.Dequeue();if(!reached.Add(p))continue;
    foreach(var next in new[]{new IntVec3(p.x+1,p.y,p.z),new(p.x-1,p.y,p.z),new(p.x,p.y,p.z+1),new(p.x,p.y,p.z-1)})
     if(joined.Contains(next)&&!reached.Contains(next))queue.Enqueue(next);
   }
   Check(reached.SetEquals(joined),"all octagon edges are cardinally connected with corner filling");
   Check(joined.IsSubsetOf(AdditionalShapes.OctagonFilled(start,end)),"corner filling stays inside the octagon");
  }
  State.Thickness=1;
  for(int width=1;width<=24;width++)for(int height=1;height<=24;height++)foreach(int radius in new[]{0,1,2,3,6,99}){
   Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.RoundedCornerRadius=radius;
   var start=new IntVec3(-9,3,11);var end=new IntVec3(start.x+width-1,3,start.z+height-1);
   var filled=AdditionalShapes.RoundedRectangleFilled(start,end).ToHashSet();
   State.FillCorners=false;var thin=AdditionalShapes.RoundedRectangle(start,end).ToHashSet();
   State.FillCorners=true;var joined=AdditionalShapes.RoundedRectangle(start,end).ToHashSet();
   Check(filled.Count>0&&filled.All(p=>p.x>=start.x&&p.x<=end.x&&p.z>=start.z&&p.z<=end.z&&p.y==3),"rounded fill bounded and on correct plane");
   Check(thin.IsSubsetOf(joined)&&joined.IsSubsetOf(filled),"rounded corner filling preserves thin outline and stays inside fill");
   foreach(var cells in new[]{filled,thin,joined}){
    Check(cells.SetEquals(cells.Select(p=>new IntVec3(start.x+end.x-p.x,p.y,p.z)))&&cells.SetEquals(cells.Select(p=>new IntVec3(p.x,p.y,start.z+end.z-p.z))),"rounded shape reflection symmetry");
   }
   Check(filled.SetEquals(AdditionalShapes.RoundedRectangleFilled(end,start)),"rounded shape reverse drag consistency");
   var reached=new HashSet<IntVec3>();var queue=new Queue<IntVec3>();queue.Enqueue(joined.First());
   while(queue.Count>0){var p=queue.Dequeue();if(!reached.Add(p))continue;
    foreach(var next in new[]{new IntVec3(p.x+1,p.y,p.z),new(p.x-1,p.y,p.z),new(p.x,p.y,p.z+1),new(p.x,p.y,p.z-1)})
     if(joined.Contains(next)&&!reached.Contains(next))queue.Enqueue(next);
   }
   Check(reached.SetEquals(joined),"rounded outline cardinally connected with corner filling");
   if(radius==0)Check(filled.Count==width*height&&joined.Count==width*height-Math.Max(0,width-2)*Math.Max(0,height-2),"radius zero is an ordinary rectangle");
   int clamped=Math.Min(radius,Math.Min((width-1)/2,(height-1)/2));
   Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.RoundedCornerRadius=clamped;
   Check(filled.SetEquals(AdditionalShapes.RoundedRectangleFilled(start,end)),"rounded radius clamps to available dimensions");
  }
  Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.RoundedCornerRadius=5;
  foreach(int thickness in new[]{2,3,4,5}){
   State.Thickness=1;var start=new IntVec3(0,0,0);var end=new IntVec3(19,0,13);
   var thin=AdditionalShapes.RoundedRectangle(start,end).ToHashSet();
   State.Thickness=thickness;var thick=AdditionalShapes.RoundedRectangle(start,end).ToHashSet();
   Check(thin.IsSubsetOf(thick)&&thick.IsSubsetOf(AdditionalShapes.RoundedRectangleFilled(start,end)),"rounded positive thickness preserves outline and clips to fill");
  }
  State.Thickness=1;
  Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.RoundedCornerRadius=10000;
  Check(AdditionalShapes.RoundedRectangleFilled(new(0,0,0),new(10000,0,10000)).Count()<=512*512,"rounded rectangle sampling bounded");
  var box=new[]{new IntVec3(0,3,0),new(6,3,0),new(6,3,4),new(0,3,4)};
  var boxFill=Primitives.Polygon(box,true,1,false).ToHashSet();
  Check(boxFill.Count==35&&boxFill.All(p=>p.x>=0&&p.x<=6&&p.z>=0&&p.z<=4&&p.y==3),"polygon rectangle exact inclusive fill");
  Check(Primitives.Polygon(box,false,1,false).Count()==20,"polygon rectangle exact outline");
  var concave=new[]{new IntVec3(0,0,0),new(6,0,0),new(6,0,2),new(2,0,2),new(2,0,6),new(0,0,6)};
  var lFill=Primitives.Polygon(concave,true,1,false).ToHashSet();
  Check(lFill.Count==33&&lFill.All(p=>p.x<=2||p.z<=2),"concave polygon excludes notch");
  var crossing=new[]{new IntVec3(0,0,0),new(6,0,6),new(0,0,6),new(6,0,0)};
  Check(Primitives.Polygon(crossing,true,1,false).Count()==31,"crossing polygon uses even-odd fill");
  foreach(var vertices in new[]{box,concave,crossing})foreach(bool filled in new[]{false,true})foreach(bool corners in new[]{false,true}){
   var cells=Primitives.Polygon(vertices,filled,1,corners).ToHashSet();
   Check(cells.SetEquals(Primitives.Polygon(vertices.Reverse().ToArray(),filled,1,corners)),"polygon vertex order does not change geometry");
   for(int shift=1;shift<vertices.Length;shift++)
    Check(cells.SetEquals(Primitives.Polygon(vertices.Skip(shift).Concat(vertices.Take(shift)).ToArray(),filled,1,corners)),"polygon first vertex does not alter geometry");
   Check(vertices.All(cells.Contains),"polygon retains every corner");
  }
  var poly=new PolygonStroke();
  Check(!poly.CanFinish&&!poly.Finish(true,1,true).Any(),"empty polygon cannot commit");
  Check(poly.TryAdd(new(2,0,3))&&poly.TryAdd(new(8,0,3))&&!poly.CanFinish,"two corners cannot commit");
  Check(!poly.TryAdd(new(8,0,3))&&poly.Vertices.Count==2,"duplicate corner ignored");
  var version=poly.Version;var previewCells=poly.Preview(new(8,0,8),true,1,true).ToHashSet();
  Check(poly.Version==version&&poly.Vertices.Count==2&&!poly.CanFinish&&previewCells.Count>0,"hover preview does not add or commit a corner");
  Check(poly.TryAdd(new(8,0,8))&&poly.CanFinish,"third non-collinear corner permits closing");
  var snapshot=poly.Finish(true,1,true).ToHashSet();poly.RemoveLast();
  Check(!poly.CanFinish&&poly.Vertices.Count==2&&snapshot.Count>0,"backspace removes corner without mutating snapshot");
  poly.Clear();Check(poly.Vertices.Count==0&&!poly.Preview(new(8,0,8),true,1,true).Any(),"cancel clears pending polygon and preview");
  for(int x=0;x<128;x++)Check(poly.TryAdd(new(x,0,0)),"polygon accepts bounded vertex count");
  Check(!poly.TryAdd(new(128,0,1))&&!poly.CanFinish,"vertex cap and collinear polygon rejection");
  poly.Clear();poly.TryAdd(new(0,0,0));Check(!poly.TryAdd(new(512,0,0))&&poly.TryAdd(new(511,0,0)),"polygon span cap rejects oversized input");
  Check(!Primitives.Polygon(new[]{new IntVec3(0,0,0),new(10000,0,0),new(0,0,10000)},true,1,true).Any(),"polygon rasterizer rejects oversized bounds");
  State.Thickness=1;
  for(int radius=0;radius<=60;radius++)for(int rotation=0;rotation<4;rotation++)foreach(bool corners in new[]{false,true}){
   State.Rotation=rotation;State.FillCorners=corners;
   var center=new IntVec3(80,3,90);var edge=new IntVec3(center.x+radius,3,center.z-radius/2);
   var outline=BasicShapes.MidpointPentagon(center,edge).ToHashSet();
   var filled=BasicShapes.MidpointPentagonFilled(center,edge).ToHashSet();
   Check(outline.SetEquals(Primitives.Pentagon(center.x-radius,3,center.z-radius,center.x+radius,3,center.z+radius,false,rotation,1,corners)),"midpoint pentagon outline reuses existing pentagon drawing");
   Check(filled.SetEquals(Primitives.Pentagon(center.x-radius,3,center.z-radius,center.x+radius,3,center.z+radius,true,rotation,1,corners)),"midpoint pentagon fill reuses existing pentagon drawing");
   Check(filled.Contains(center)&&outline.IsSubsetOf(filled),"midpoint pentagon contains center and outline");
   Check(filled.All(p=>p.x>=center.x-radius&&p.x<=center.x+radius&&p.z>=center.z-radius&&p.z<=center.z+radius&&p.y==3),"midpoint pentagon bounded around click center");
   Check(filled.SetEquals(BasicShapes.MidpointPentagonFilled(center,new(center.x-radius/2,3,center.z-radius))),"midpoint pentagon radius independent of drag quadrant");
  }
  State.Rotation=0;
  Check(BasicShapes.MidpointPentagonFilled(new(0,0,0),new(10000,0,-10000)).All(p=>p.x>=-255&&p.x<=255&&p.z>=-255&&p.z<=255),"midpoint pentagon radius cap");
  Console.WriteLine($"PASS {checks} geometry invariants against actual primitives and game value types");
 }
}
namespace Merthsoft.DesignatorShapes {public static class DesignatorShapes {public static int Thickness=1;public static int Rotation;public static bool FillCorners=true;}}

namespace Merthsoft.DesignatorShapes.Ui { public static class ShapeControlsWindow { public static IntVec3 CheckerTileSize=new(1,0,1); public static IntVec3 ParallelLineSize=new(1,0,3); public static int RoundedCornerRadius=3; } }
