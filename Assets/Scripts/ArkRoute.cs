using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace AnimalArchive {
 [Serializable] public class ArkBook {public ArkStage[] stages;public ArkLine[] prologue;}
 [Serializable] public class ArkLine {public string speaker,text,mood;}
 [Serializable] public class ArkAnimal {public int id;public float x,z,gx,gz;public Vector2 Start=>new Vector2(x,z);public Vector2 Gate=>new Vector2(gx,gz);}
 [Serializable] public class ArkBlock {public float x,z,w,d;public string kind;public Rect Bounds=>new Rect(x-w/2,z-d/2,w,d);}
 [Serializable] public class ArkStep {public int animal;public float x,z;public bool exit;}
 [Serializable] public class ArkStage {public string title,place,brief;public int chapter;public ArkAnimal[] animals;public ArkBlock[] blocks;public int[] rewards;public ArkLine[] before,after;public bool lever;public float sx,sz;public int keeper=-1;public ArkStep[] solution;}
 // Visibility-graph navigation in continuous world coordinates; no gameplay grid.
 public static class ArkRoute {
  public const float Radius=.45f;
  public static Rect Expand(Rect r,float n)=>new Rect(r.xMin-n,r.yMin-n,r.width+2*n,r.height+2*n);
  public static bool Inside(Vector2 p)=>p.x>=-4.65f&&p.x<=4.65f&&p.y>=-4.9f&&p.y<=5.15f;
  public static bool SegmentClear(Vector2 a,Vector2 b,IList<Rect> boxes){
   foreach(var r in boxes){float lo=0,hi=1;Vector2 d=b-a;
    if(Clip(-d.x,a.x-r.xMin,ref lo,ref hi)&&Clip(d.x,r.xMax-a.x,ref lo,ref hi)&&Clip(-d.y,a.y-r.yMin,ref lo,ref hi)&&Clip(d.y,r.yMax-a.y,ref lo,ref hi)&&hi>=lo)return false;
   }return true;
  }
  static bool Clip(float p,float q,ref float lo,ref float hi){if(Mathf.Abs(p)<.00001f)return q>=0;float t=q/p;if(p<0){if(t>hi)return false;lo=Mathf.Max(lo,t);}else{if(t<lo)return false;hi=Mathf.Min(hi,t);}return true;}
  public static List<Vector2> Find(Vector2 a,Vector2 b,IList<Rect> boxes){
   if(!Inside(a)||!Inside(b)||boxes.Any(r=>r.Contains(a)||r.Contains(b)))return null;
   var nodes=new List<Vector2>{a,b};const float e=.025f;
   foreach(var r in boxes){var corners=new[]{new Vector2(r.xMin-e,r.yMin-e),new Vector2(r.xMax+e,r.yMin-e),new Vector2(r.xMax+e,r.yMax+e),new Vector2(r.xMin-e,r.yMax+e)};foreach(var p in corners)if(Inside(p)&&!boxes.Any(x=>x.Contains(p)))nodes.Add(p);}
   int n=nodes.Count;var dist=Enumerable.Repeat(float.PositiveInfinity,n).ToArray();var prev=Enumerable.Repeat(-1,n).ToArray();var used=new bool[n];dist[0]=0;
   for(int k=0;k<n;k++){int u=-1;for(int i=0;i<n;i++)if(!used[i]&&(u<0||dist[i]<dist[u]))u=i;if(u<0||float.IsInfinity(dist[u]))break;if(u==1)break;used[u]=true;
    for(int v=0;v<n;v++)if(!used[v]&&SegmentClear(nodes[u],nodes[v],boxes)){float d=dist[u]+Vector2.Distance(nodes[u],nodes[v]);if(d<dist[v]){dist[v]=d;prev[v]=u;}}
   }if(float.IsInfinity(dist[1]))return null;var path=new List<Vector2>();for(int i=1;i>=0;i=prev[i]){path.Add(nodes[i]);if(i==0)break;}path.Reverse();return path;
  }
 }
 public class ArkRun {
  public ArkStage stage;public Vector2[] positions;public bool[] rescued;public bool opened;public int moves;
  struct Snapshot{public Vector2[] p;public bool[] r;public bool o;public int m;}
  Stack<Snapshot> history=new Stack<Snapshot>();
  public ArkRun(ArkStage s){stage=s;positions=s.animals.Select(a=>a.Start).ToArray();rescued=new bool[s.animals.Length];opened=!s.lever;}
  public int Remaining=>rescued.Count(r=>!r);public bool CanUndo=>history.Count>0;
  public List<Rect> Obstacles(int animal){var list=stage.blocks.Where(b=>b.kind!="barrier"||!opened).Select(b=>ArkRoute.Expand(b.Bounds,ArkRoute.Radius)).ToList();for(int i=0;i<positions.Length;i++)if(i!=animal&&!rescued[i])list.Add(new Rect(positions[i].x-.8f,positions[i].y-.8f,1.6f,1.6f));return list;}
  public List<Vector2> Plan(int a,Vector2 to){if(a<0||a>=positions.Length||rescued[a])return null;return ArkRoute.Find(positions[a],to,Obstacles(a));}
  public bool Commit(int a,List<Vector2> path,bool exit){if(path==null||path.Count<2||rescued[a]||Vector2.Distance(path[0],positions[a])>.01f)return false;var boxes=Obstacles(a);for(int i=1;i<path.Count;i++)if(!ArkRoute.Inside(path[i])||!ArkRoute.SegmentClear(path[i-1],path[i],boxes))return false;var end=path[path.Count-1];if(exit&&Vector2.Distance(end,stage.animals[a].Gate)>.05f)return false;
   history.Push(new Snapshot{p=(Vector2[])positions.Clone(),r=(bool[])rescued.Clone(),o=opened,m=moves});positions[a]=end;rescued[a]=exit;moves++;
   if(stage.lever&&Vector2.Distance(end,new Vector2(stage.sx,stage.sz))<.65f&&(stage.keeper<0||stage.animals[a].id==stage.keeper))opened=true;return true;
  }
  public bool Undo(){if(history.Count==0)return false;var s=history.Pop();positions=s.p;rescued=s.r;opened=s.o;moves=s.m;return true;}
 }
}
