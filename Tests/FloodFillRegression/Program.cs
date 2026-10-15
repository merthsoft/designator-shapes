using Merthsoft.DesignatorShapes.Shapes;
using Verse;
static class Program {
 static void Main() {
  Find.CurrentMap = new Map();
  var origin=new IntVec3(1,0,1);
  var baseline=FloodFill.Fill(origin,origin).ToHashSet();
  Check(baseline.Count==9,"empty map fill");
  Find.CurrentMap.thingGrid.Cells[origin]=[new Thing {def=new ThingDef {entityDefToBuild=new TerrainDef()}}];
  Check(FloodFill.Fill(origin,origin).Count()==9,"terrain blueprint does not throw or block fill");
  Find.CurrentMap.thingGrid.Cells[origin]=[new Thing {def=new ThingDef {coversFloor=true}}];
  Check(!FloodFill.Fill(new(0,0,0),new(0,0,0)).Contains(origin),"building covering floor blocks fill");
  Find.CurrentMap.thingGrid.Cells[origin]=[new Thing {def=new ThingDef {entityDefToBuild=new ThingDef {designationCategory=new() {defName="Structure"}}}}];
  Check(!FloodFill.Fill(new(0,0,0),new(0,0,0)).Contains(origin),"structure blueprint blocks fill");
  Check(FloodFill.Fill(origin,new(-1,0,0)).Count()==0,"off-map seed returns empty");
  Find.CurrentMap=new Map();
  var map=Find.CurrentMap;
  var config=Merthsoft.DesignatorShapes.DesignatorShapes.Settings;
  map.Fog.Add(origin);
  Check(FloodFill.Fill(origin,origin).Count()==9,"default preserves fogged traversal");
  config.RestrictFloodFillToRevealedCells=true;
  Check(FloodFill.Fill(origin,origin).Count()==0,"enabled fogged seed rejected");
  Check(FloodFill.Fill(new(0,0,0),new(0,0,0)).Count()==8,"enabled fogged boundary excluded");
  map.Fog.Add(new(1,0,0)); map.Fog.Add(new(1,0,2));
  Check(FloodFill.Fill(new(0,0,0),new(0,0,0)).Count()==3,"fogged bridge cannot connect revealed regions");
  map.Fog.Clear();
  var ore=new ThingDef {building=new(){isNaturalRock=true}};
  map.thingGrid.Cells[origin]=[new RimWorld.Mineable {def=ore}];
  foreach(var direction in GenAdj.CardinalDirections)map.linkGrid.Rock.Add(origin+direction);
  Check(FloodFill.Fill(origin,origin).Count()==0,"unfogged concealed ore seed rejected");
  config.RestrictFloodFillToRevealedCells=false;
  Check(FloodFill.Fill(origin,origin).Count()==1,"default preserves concealed ore scouting");
  config.RestrictFloodFillToRevealedCells=true;
  map.linkGrid.Rock.Remove(origin+IntVec3.West);
  Check(FloodFill.Fill(origin,origin).Count()==1,"legitimately exposed unfogged ore permitted");
  map.Fog.Add(origin+IntVec3.West);
  Check(FloodFill.Fill(origin,origin).Count()==0,"face toward fog does not expose ore");
  Find.CurrentMap=new Map(); map=Find.CurrentMap;
  for(int x=0;x<3;x++) {map.thingGrid.Cells[new(x,0,1)]=[new RimWorld.Mineable {def=ore}];for(int z=0;z<3;z++)map.linkGrid.Rock.Add(new(x,0,z));}
  map.linkGrid.Rock.Remove(new(0,0,2));map.linkGrid.Rock.Remove(new(2,0,2));
  config.RestrictFloodFillToRevealedCells=true;
  Check(FloodFill.Fill(new(0,0,1),new(0,0,1)).Count()==1,"unfogged concealed rock bridge cannot connect exposed regions");
  config.RestrictFloodFillToRevealedCells=false;
  Check(FloodFill.Fill(new(0,0,1),new(0,0,1)).Count()==3,"default preserves full connected ore vein");
  Find.CurrentMap=new Map(); map=Find.CurrentMap;
  map.designationManager.Cells[origin]=[new()];
  map.designationManager.Cells[origin+IntVec3.North]=[new()];
  map.designationManager.Cells[new(0,0,2)]=[new()];
  map.thingGrid.Cells[origin+IntVec3.North]=[new Thing {def=new ThingDef {designationCategory=new(){defName="Structure"}}}];
  Check(FloodFill.Fill(origin,origin).Count()==1,"planned fill cannot cross designated structural wall");
  map.thingGrid.Cells.Clear();
  Check(FloodFill.Fill(origin,origin).Count()==3,"connected plans still fill without physical barrier");
  Find.CurrentMap=new Map(); map=Find.CurrentMap;
  map.designationManager.Cells[origin]=[new()];
  Check(FloodFill.Fill(origin,origin).Count()==1,"designated seed remains stable after undesignated neighbor query");
  map.designationManager.Cells.Clear();map.designationManager.Cells[origin+IntVec3.North]=[new()];
  Check(FloodFill.Fill(origin,origin).Count()==9,"undesignated seed remains stable after designated neighbor query");
  map.designationManager.Cells.Clear();
  map.thingGrid.Cells[origin]=[new RimWorld.Mineable{def=ore}];
  foreach(var direction in GenAdj.CardinalDirections)map.thingGrid.Cells[origin+direction]=[new Thing{def=new ThingDef{fillPercent=1}}];
  config.RestrictFloodFillToRevealedCells=true;
  Check(FloodFill.Fill(origin,origin).Count()==0,"constructed full walls conceal enclosed ore");
  config.RestrictFloodFillToRevealedCells=false;
  Check(FloodFill.Fill(origin,origin).Count()==1,"default still allows ore inside constructed walls");
  config.RestrictFloodFillToRevealedCells=true;
  var resourceOnly=new ThingDef{building=new(){isResourceRock=true}};
  map.thingGrid.Cells[origin]=[new RimWorld.Mineable{def=resourceOnly}];
  Check(FloodFill.Fill(origin,origin).Count()==0,"resource-only ore also requires exposed face");
  map.thingGrid.Cells[origin+IntVec3.West][0].def.fillPercent=0.5f;
  Check(FloodFill.Fill(origin,origin).Count()==1,"partial edifice permits exposed ore");
  Find.CurrentMap=new Map();map=Find.CurrentMap;
  map.thingGrid.Cells[origin]=[new RimWorld.Mineable{def=ore}];
  foreach(var direction in GenAdj.CardinalDirections){map.linkGrid.Rock.Add(origin+direction);map.LinkedWithoutEdifice.Add(origin+direction);}
  Check(FloodFill.Fill(origin,origin).Count()==0,"rock links conceal ore even without full edifices");
  map.linkGrid.Rock.Remove(origin+IntVec3.West);
  Check(FloodFill.Fill(origin,origin).Count()==1,"unlinked open face exposes ore");
  Find.CurrentMap=new Map();map=Find.CurrentMap;config.RestrictFloodFillToRevealedCells=false;
  var barrier=new ThingDef{defName="Conveyor"};
  for(int z=0;z<3;z++)map.thingGrid.Cells[new(1,0,z)]=[new Thing{def=barrier}];
  config.AdditionalFloodFillBarrierDefs.Add("Conveyor");
  Check(FloodFill.Fill(new(0,0,1),new(0,0,1)).Count()==9,"barrier list disabled preserves default");
  config.EnableCustomFloodFillBarriers=true;
  Check(FloodFill.Fill(new(0,0,1),new(0,0,1)).Count()==3,"additional buildings stop ordinary region fill");
  for(int z=0;z<3;z++)map.thingGrid.Cells[new(1,0,z)]=[new Thing{def=new ThingDef{entityDefToBuild=barrier}}];
  Check(FloodFill.Fill(new(0,0,1),new(0,0,1)).Count()==3,"resolved blueprint or frame target stops fill");
  barrier.designationCategory=new(){defName="Structure"};
  Check(FloodFill.Fill(origin,origin).Count()==3,"matching wall seed exempt from added barriers");
  map.thingGrid.Cells.Clear();ore.defName="Ore";config.AdditionalFloodFillBarrierDefs.Add("Ore");
  for(int x=0;x<3;x++)map.thingGrid.Cells[new(x,0,1)]=[new RimWorld.Mineable{def=ore}];
  Check(FloodFill.Fill(origin,origin).Count()==3,"matching ore seed exempt from added barriers");
  Find.CurrentMap=null;
  Check(FloodFill.Fill(origin,origin).Count()==0,"missing map returns empty");
 }
 static void Check(bool success,string name) {if(!success) throw new Exception(name); Console.WriteLine("PASS "+name);}
}
namespace RimWorld {public class Mineable:Verse.Thing {}}
namespace Merthsoft.DesignatorShapes {public static class DesignatorShapes {public static Config Settings=new();} public class Config {public bool EnableCustomFloodFillBarriers=false; public List<string> AdditionalFloodFillBarrierDefs=new(); public int FloodFillCellLimit=100; public bool RestrictFloodFillToRevealedCells=false;}}
namespace Verse {
 public record struct IntVec3(int x,int y,int z) { public static IntVec3 North=>new(0,0,1); public static IntVec3 South=>new(0,0,-1); public static IntVec3 East=>new(1,0,0); public static IntVec3 West=>new(-1,0,0); public static IntVec3 operator +(IntVec3 a,IntVec3 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z); public Thing GetEdifice(Map m)=>m.thingGrid.ThingsListAtFast(this).FirstOrDefault(t=>t.def.fillPercent>0) ?? (m.linkGrid.Rock.Contains(this)&&!m.LinkedWithoutEdifice.Contains(this)?new Thing{def=new ThingDef{fillPercent=1}}:null); public bool Fogged(Map m)=>m.Fog.Contains(this); public bool InBounds(Map m)=>x>=0&&z>=0&&x<3&&z<3; public List<Thing> GetThingList(Map m)=>m.thingGrid.ThingsListAtFast(this); }
 public class Def {public string defName;}
 public class BuildableDef:Def {public Def designationCategory;}
 public class TerrainDef:BuildableDef {public bool IsFloor;}
 public class BuildingProperties {public bool isNaturalRock; public bool isResourceRock;} public class ThingDef:BuildableDef {public float fillPercent; public BuildingProperties building;public BuildableDef entityDefToBuild; public bool coversFloor;}
 public class Thing {public ThingDef def;}
 public class Designation {}
 public static class GenAdj {public static IntVec3[] CardinalDirections=[IntVec3.North,IntVec3.East,IntVec3.South,IntVec3.West];} [Flags] public enum LinkFlags {None=0,Rock=1} public class LinkGrid {public HashSet<IntVec3> Rock=new(); public LinkFlags LinkFlagsAt(IntVec3 c)=>Rock.Contains(c)?LinkFlags.Rock:LinkFlags.None;} public class Map {public HashSet<IntVec3> LinkedWithoutEdifice=new();public HashSet<IntVec3> Fog=new(); public LinkGrid linkGrid=new();public ThingGrid thingGrid=new(); public TerrainGrid terrainGrid=new(); public FertilityGrid fertilityGrid=new(); public DesignationManager designationManager=new();}
 public class ThingGrid {public Dictionary<IntVec3,List<Thing>> Cells=new(); public List<Thing> ThingsListAtFast(IntVec3 c)=>Cells.TryGetValue(c,out var things)?things:[];}
 public class TerrainGrid {public TerrainDef TerrainAt(IntVec3 c) {if(!c.InBounds(Find.CurrentMap))throw new Exception("invalid terrain lookup"); return new();}}
 public class FertilityGrid {public float FertilityAt(IntVec3 c)=>1;}
 public class DesignationManager {public Dictionary<IntVec3,List<Designation>> Cells=new(); private readonly List<Designation> temp=new(); public IEnumerable<Designation> AllDesignationsAt(IntVec3 c){temp.Clear();if(Cells.TryGetValue(c,out var ds))temp.AddRange(ds);return temp;}}
 public class Designator {public Acceptance CanDesignateCell(IntVec3 c)=>new();} public class Acceptance {public bool Accepted=true;}
 public class DesignatorManager {public Designator SelectedDesignator=new();} public class PlaySettings {public bool showFertilityOverlay;}
 public static class Find {public static Map CurrentMap; public static DesignatorManager DesignatorManager=new(); public static PlaySettings PlaySettings=new();}
}
