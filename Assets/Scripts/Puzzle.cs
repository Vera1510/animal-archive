using System;
using System.Collections.Generic;
using System.Linq;

namespace AnimalArchive {
 [Serializable] public class AnimalDef { public string id,kind; public int x,y,portrait; }
 [Serializable] public class GateDef { public string kind,target; public int x,y; }
 [Serializable] public class CellDef { public int x,y; }
 [Serializable] public class LevelDef {
  public string id,title,subtitle,intent,hint,goal,completion; public int width,height;
  public bool identityMarkers,linkDiagonals; public int linkDistance=1;
  public float mapX,mapY,mapWidth=370,mapHeight=73;
  public AnimalDef[] animals; public GateDef[] gates; public CellDef[] walls;
  public int chapter; public int[] rewards; public string before,after;
 }
 [Serializable] public class LevelBook { public LevelDef[] levels; }
 [Serializable] public class Line { public string speaker,text,mood; }
 [Serializable] public class Conversation { public string id,place; public Line[] lines; }
 [Serializable] public class StoryBook { public Conversation[] conversations; }
 public struct Pos : IEquatable<Pos> {
  public int x,y; public Pos(int x,int y){this.x=x;this.y=y;}
  public bool gone => x<0; public bool Equals(Pos b)=>x==b.x&&y==b.y;
  public override bool Equals(object b)=>b is Pos p&&Equals(p);
  public override int GetHashCode()=>x*31+y;
 }
 public class Move {
  public bool valid; public string reason; public int[] group; public Pos[] after; public int[] exited;
 }
 // Pure deterministic rules. No rendering, time, randomness or scene references.
 public class Puzzle {
  public readonly LevelDef level; public Pos[] positions; public int moves;
  readonly Stack<Pos[]> history=new Stack<Pos[]>();
  public Puzzle(LevelDef level){this.level=level;Reset();}
  public void Reset(){positions=level.animals.Select(a=>new Pos(a.x,a.y)).ToArray();history.Clear();moves=0;}
  public int Remaining=>positions.Count(p=>!p.gone);
  public bool CanUndo=>history.Count>0;
  public bool Undo(){if(!CanUndo)return false;positions=history.Pop();moves--;return true;}
  public int[] Group(int index){
   if(index<0||index>=positions.Length||positions[index].gone)return Array.Empty<int>();
   var found=new List<int>{index};
   if(level.animals[index].kind!="sheep")return found.ToArray();
   for(int n=0;n<found.Count;n++)for(int j=0;j<positions.Length;j++){
    if(positions[j].gone||level.animals[j].kind!="sheep"||found.Contains(j))continue;
    var a=positions[found[n]];var b=positions[j];
    int distance=level.linkDiagonals?Math.Max(Math.Abs(a.x-b.x),Math.Abs(a.y-b.y)):Math.Abs(a.x-b.x)+Math.Abs(a.y-b.y);
    if(distance>0&&distance<=Math.Max(1,level.linkDistance))found.Add(j);
   }
   return found.ToArray();
  }
  public Move Preview(int index,int dx,int dy){
   var group=Group(index);var m=new Move{group=group,after=(Pos[])positions.Clone(),exited=Array.Empty<int>(),reason="動物を選んでください。"};
   if(group.Length==0||Math.Abs(dx)+Math.Abs(dy)!=1)return m;
   var exits=new List<int>();
   foreach(int i in group){
    var p=new Pos(positions[i].x+dx,positions[i].y+dy);
    if(p.x<0||p.x>=level.width||p.y<0||p.y>=level.height||level.walls.Any(w=>w.x==p.x&&w.y==p.y)){m.reason=group.Length>1?"仲間の行き先に壁があります。群れは動けません。":"そこは壁です。別の道を探しましょう。";return m;}
    for(int j=0;j<positions.Length;j++)if(!group.Contains(j)&&positions[j].Equals(p)){m.reason="別の動物が道をふさいでいます。";return m;}
    var gate=level.gates.FirstOrDefault(g=>g.x==p.x&&g.y==p.y);
    if(gate!=null&&(gate.kind!=level.animals[i].kind||(!string.IsNullOrEmpty(gate.target)&&gate.target!=level.animals[i].id))){m.reason="この通路は、別の動物の出口です。";return m;}
    if(gate!=null){m.after[i]=new Pos(-1,-1);exits.Add(i);}else m.after[i]=p;
   }
   m.valid=true;m.exited=exits.ToArray();m.reason=group.Length>1?"つながった羊が、同じ方向へ一歩。":"一歩進みます。";return m;
  }
  public Move Act(int index,int dx,int dy){var m=Preview(index,dx,dy);if(m.valid){history.Push((Pos[])positions.Clone());positions=m.after;moves++;}return m;}
  // Every intermediate tile is checked; a drag is ONE undo transaction.
  public Move Trace(int index,int dx,int dy,int steps){
   var sim=new Puzzle(level);sim.positions=(Pos[])positions.Clone();
   var result=new Move{after=(Pos[])positions.Clone(),group=Group(index),exited=Array.Empty<int>(),reason="道を指でなぞってください。"};
   if(Math.Abs(dx)+Math.Abs(dy)!=1)return result;
   for(int n=0;n<Math.Min(steps,Math.Max(level.width,level.height));n++){
    var m=sim.Preview(index,dx,dy);if(!m.valid){if(!result.valid)result.reason=m.reason;break;}
    sim.positions=m.after;result=m;
    if(m.exited.Length>0)break;
   }return result;
  }
  public Move Slide(int index,int dx,int dy,int steps){var m=Trace(index,dx,dy,steps);if(m.valid){history.Push((Pos[])positions.Clone());positions=m.after;moves++;}return m;}
  public int[] FirstSolutionAction(int limit=30000){
   var queue=new Queue<(Pos[],int[])>();queue.Enqueue(((Pos[])positions.Clone(),null));var seen=new HashSet<string>{Key()};
   int[] xs={-1,0,0,1},ys={0,-1,1,0};
   while(queue.Count>0&&seen.Count<limit){var node=queue.Dequeue();var p=new Puzzle(level);p.positions=node.Item1;if(p.Remaining==0)return node.Item2;
    for(int a=0;a<positions.Length;a++)for(int d=0;d<4;d++)for(int s=1;s<=Math.Max(level.width,level.height);s++){
     var m=p.Trace(a,xs[d],ys[d],s);if(!m.valid)break;var key=string.Join(";",m.after.Select(v=>v.x+","+v.y));if(seen.Add(key))queue.Enqueue((m.after,node.Item2??new[]{a,xs[d],ys[d],s}));if(m.exited.Length>0)break;
    }
   }return null;
  }
  public string Key()=>string.Join(";",positions.Select(p=>p.x+","+p.y));
 }
}
