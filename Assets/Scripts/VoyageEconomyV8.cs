using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AnimalArchive.Voyage {
 [Serializable] public class Worker {public int role,level;public float x=.36f,y=.67f,tx=.36f,ty=.67f,work,rest,cooldown,pulse,manual,commandCooldown;}
 [Serializable] public class CropPlot {public int crop=-1;public float growth,buff;public int harvests;public bool matureNotice;}
 public partial class Passenger {public float roam,roamBlend,wanderClock;}
 public partial class VoyageState {public int v8,area,seenSpecies,unlocks;public List<Worker> workers=new List<Worker>();public CropPlot[] plots=new[]{new CropPlot(),new CropPlot()};public long lost,lastUtc,offlineTotal;public int demon;public float demonTime,demonHp=100,attackClock,prayer,prayerCooldown,holyPulse,ward,manualUntil;public int demonLost;public long awayAward;public int awaySeconds;}
 public partial class VoyageModel {
  public static readonly string[] WorkerNames={"ナギ","トウマ","ハル"};
  public static readonly string[] CropNames={"クローバー","木の実","月の花"};
  public readonly List<Vector3> scattered=new List<Vector3>();
  public float LandMin=>state.area==0?.02f:state.area==1?-.38f:-.78f;
  public bool DemonActive=>state.demon==2;
  public bool NewCodex=>(state.discovered&~state.seenSpecies)!=0;
  public bool GrowthDot=>Enumerable.Range(0,4).Any(i=>CanBuy(i));
  public bool StaffDot=>CanHire||state.workers.Any(w=>w.level<2&&state.available>=WorkerCost(w.role));
  public bool FacilityDot=>CanExpand||state.plots.Take(state.area).Any(p=>p.crop<0||p.growth>=1&&p.buff<=0);
  public bool CanBuy(int i)=>state.mode==0&&Cost(i)>=0&&Unlocked(i)&&state.available>=Cost(i);
  public long ExpandCost=>state.area==0?100:state.area==1?650:-1;
  public bool CanExpand=>state.mode==0&&state.area<2&&state.total>=(state.area==0?160:1600)&&state.available>=ExpandCost;
  public long HireCost=>state.workers.Count==1?80:320;
  public bool HireUnlocked=>state.workers.Count<3&&state.total>=(state.workers.Count==1?80:650)&&(state.workers.Count==1||state.area>0);
  public bool CanHire=>state.mode==0&&HireUnlocked&&state.available>=HireCost;
  public long WorkerCost(int role)=>state.workers[role].level>=2?-1:(role+1)*120*(state.workers[role].level+1);
  void InitV8(){state.autoEnabled=false;state.guideAuto=false;if(state.workers==null)state.workers=new List<Worker>();if(state.workers.Count==0)state.workers.Add(new Worker{role=0,x=state.guideX,y=state.guideY});if(state.plots==null||state.plots.Length!=2)state.plots=new[]{new CropPlot(),new CropPlot()};if(state.v8==0){state.v8=8;state.lastUtc=0;if(state.completed)state.demon=3;}}
  bool Spend(long cost){if(cost<0||state.available<cost||state.mode!=0)return false;state.available-=cost;state.spent+=cost;return true;}
  public bool Hire(){if(!CanHire)return false;int role=state.workers.Count;if(!Spend(HireCost))return false;state.workers.Add(new Worker{role=role,x=RestPoint.x,y=RestPoint.y,tx=.4f,ty=.64f,cooldown=1});events.Add("hire:"+role);return true;}
  public bool Train(int role){if(role<0||role>=state.workers.Count||!Spend(WorkerCost(role)))return false;state.workers[role].level++;events.Add("train:"+role);return true;}
  public bool Expand(){if(!CanExpand||!Spend(ExpandCost))return false;state.area++;Plant(state.area-1,BestCrop(state.area-1));events.Add("expand:"+state.area);return true;}
  public Vector2 RestPoint=>new Vector2(state.area>0?-.13f:.20f,.43f);
  public Vector2 PlotPoint(int i)=>new Vector2(i==0?-.23f:-.60f,.64f);
  public Vector2 LandTarget(float x,float y){x=Mathf.Clamp(x,LandMin,.86f);y=Mathf.Clamp(y,.43f,.80f);if(x>.66f)y=Mathf.Max(y,.55f+(x-.66f)*.48f);return new Vector2(x,y);}
  public bool Plant(int plot,int crop){if(state.mode!=0||plot<0||plot>=state.area||crop<0||crop>Math.Min(2,state.area))return false;var p=state.plots[plot];if(p.crop==crop)return false;p.crop=crop;p.growth=0;p.buff=0;p.matureNotice=false;events.Add("plant:"+crop);return true;}
  public bool Harvest(int plot,bool automatic=false){if(state.mode!=0||plot<0||plot>=state.area)return false;var p=state.plots[plot];if(p.crop<0||p.growth<1||p.buff>0)return false;p.buff=automatic?24:35;p.harvests++;events.Add("harvest:"+plot);return true;}
  public void Water(int plot){if(plot<0||plot>=state.area||state.mode!=0)return;state.plots[plot].growth=Mathf.Min(1,state.plots[plot].growth+.16f);events.Add("water");}
  public bool TendGarden(){if(state.workers.Count<3||state.area==0)return false;var w=state.workers[2];int plot=state.area==2&&Distance(new Vector2(w.x,w.y),PlotPoint(1))<Distance(new Vector2(w.x,w.y),PlotPoint(0))?1:0;var at=PlotPoint(plot);CommandWorker(2,at.x,at.y);if(Distance(new Vector2(w.x,w.y),at)<.13f){Water(plot);Harvest(plot);w.pulse=1;}return true;}
  public float CropWeight(int species){float mul=1;foreach(var p in state.plots)if(p.buff>0){if(p.crop==0&&(species==1||species==3||species==9))mul+=2;if(p.crop==1&&(species==2||species==6||species==11))mul+=2;if(p.crop==2&&(species==4||species==10))mul+=3;}return mul;}
  public static float Response(int species)=>species==1||species==3||species==9?3:species==0||species==2?1:2;
  void ConfigurePassenger(Passenger p){InitFlow(p,true);p.roam=p.species==4?0:Random01()<.55f?2+Random01()*5:0;p.wanderClock=p.animOffset;if(state.area>0&&config.animals[p.species].rarity==2){int f=p.species==10?1:0;if(f<state.area&&state.plots[f].growth>=.65f&&LikesCrop(p.species,state.plots[f].crop)){var at=PlotPoint(f);p.x=at.x-.06f;p.y=at.y+.04f;p.tx=p.x;p.ty=p.y;events.Add("field-rare:"+f);}}if(config.animals[p.species].rarity==2&&state.elapsed>1)events.Add("rare:"+p.species);}
  float RoamSpeed(Passenger p,float dt){p.wanderClock+=dt;bool wandering=p.age>=0&&p.age<p.duration*.34f&&p.roam>0&&state.mode==0;if(wandering)p.roam=Mathf.Max(0,p.roam-dt*(p.calm>0?Response(p.species)*2:1));p.roamBlend=Mathf.MoveTowards(p.roamBlend,wandering?1:0,dt*2);return wandering&&p.calm<=0?.08f:1;}
  public bool CommandWorker(int role,float x,float y){if(role<0||role>=state.workers.Count||state.mode!=0||!float.IsFinite(x)||!float.IsFinite(y))return false;if(role==0)return Tap(x,y);var w=state.workers[role];if(w.commandCooldown>0)return false;w.commandCooldown=.14f;var at=LandTarget(x,y);w.tx=at.x;w.ty=at.y;if(w.manual<=0)w.cooldown=0;w.manual=6;w.rest=0;w.pulse=.3f;w.work=Mathf.Min(w.work,18);state.tutorial=Math.Max(1,state.tutorial);events.Add("worker-command");return true;}
  void CalmAt(Vector2 at,float range,float seconds){foreach(var p in state.active)if(p.age>=0&&Distance(at,Position(p))<range){p.calm=Mathf.Max(p.calm,seconds);p.roam=Mathf.Max(0,p.roam-seconds);}}
  public string WorkerStatus(int role){var w=state.workers[role];return w.rest>0?"休憩 "+Mathf.CeilToInt(w.rest)+"秒":w.manual>0||(role==0&&state.manualUntil>state.elapsed)?"指示中":role==2?"畑を手入れ":"自動で案内";}
  public bool Pray(float dt){if(!DemonActive||state.mode!=0||state.prayerCooldown>0||dt<=0)return false;state.targetX=state.guideX;state.targetY=state.guideY;state.manualUntil=state.elapsed+3;state.prayer+=Mathf.Min(dt,.25f);state.ward=1.2f;if(state.prayer<.8f)return false;state.prayer=0;state.prayerCooldown=2;state.holyPulse=1;state.demonHit=.55f;state.bossStun=1.1f;state.attackClock=Mathf.Max(state.attackClock,3.3f);var push=(DemonPoint-new Vector2(state.guideX,state.guideY)).normalized*.06f;var pushed=LandTarget(state.demonX+push.x,state.demonY+push.y);state.demonX=Mathf.Min(.57f,pushed.x);state.demonY=pushed.y;state.ward=5;state.demonHp=Mathf.Max(0,state.demonHp-28-state.levels[3]);events.Add("holy");if(state.demonHp<=0)ResolveDemon();return true;}
  void ResolveDemon(){state.demon=3;state.demonHp=0;state.holyPulse=2;state.ward=8;events.Add("demon-clear");}
  void Scatter(Passenger p){var pos=Position(p);scattered.Add(new Vector3(pos.x,pos.y,p.species));state.active.Remove(p);state.lostValue+=p.reward;state.lost++;CompactLedger();state.demonLost++;events.Add("scatter");}
  public Vector2 DemonPoint=>new Vector2(state.demonX,state.demonY);
  void TickV8(float dt){if(state.mode>=2){state.prayer=state.holyPulse=state.ward=0;return;}state.prayerCooldown=Mathf.Max(0,state.prayerCooldown-dt);state.holyPulse=Mathf.Max(0,state.holyPulse-dt);state.ward=Mathf.Max(0,state.ward-dt);
   if(state.area<2&&state.total>=(state.area==0?160:1600)&&(state.unlocks&(16<<state.area))==0){state.unlocks|=16<<state.area;events.Add("area-ready");}
   if(HireUnlocked&&(state.unlocks&(1<<state.workers.Count))==0){state.unlocks|=1<<state.workers.Count;events.Add("hire-ready");}
   for(int i=0;i<state.area;i++){var p=state.plots[i];if(p.crop<0)continue;if(p.buff>0){p.buff=Mathf.Max(0,p.buff-dt);if(p.buff==0){p.growth=0;p.matureNotice=false;}}else p.growth=Mathf.Min(1,p.growth+dt/18);if(p.growth>=1&&!p.matureNotice){p.matureNotice=true;events.Add("field-ready:"+i);}if(state.workers.Count<3&&p.growth>=1&&p.buff<=0)Harvest(i,true);}
   for(int i=0;i<state.workers.Count;i++){var w=state.workers[i];w.commandCooldown=Mathf.Max(0,w.commandCooldown-dt);w.manual=Mathf.Max(0,w.manual-dt);w.pulse=Mathf.Max(0,w.pulse-dt);w.cooldown=Mathf.Max(0,w.cooldown-dt);if(i==0){w.x=state.guideX;w.y=state.guideY;}
    bool commanded=w.manual>0||(i==0&&state.manualUntil>state.elapsed);if(w.rest>0&&!commanded){w.rest=Mathf.Max(0,w.rest-dt);w.tx=RestPoint.x+i*.04f;w.ty=RestPoint.y+.025f;if(w.rest==0){w.work=0;events.Add("work:"+i);}}
    else {w.work+=dt;if(w.work>28+w.level*8&&!commanded){w.rest=6;events.Add("rest:"+i);}else if(!commanded&&w.cooldown<=0){if(i==2&&state.area>0){int plot=(int)(state.elapsed/12)%state.area;var at=PlotPoint(plot);w.tx=at.x;w.ty=at.y-.035f;if(Distance(new Vector2(w.x,w.y),at)<.12f){var field=state.plots[plot];int crop=BestCrop(plot);if(field.crop<0||(field.buff<=0&&field.growth<.25f&&field.crop!=crop))Plant(plot,crop);field.growth=Mathf.Min(1,field.growth+.22f);Harvest(plot,true);w.pulse=1;w.cooldown=4;}}
      else {var target=state.active.Where(p=>p.age>=0&&!p.admitted).OrderBy(p=>p.queued?100:Distance(new Vector2(w.x,w.y),Position(p))).FirstOrDefault();if(target!=null){var at=LandTarget(Position(target).x,Position(target).y);w.tx=at.x;w.ty=at.y;if(Distance(new Vector2(w.x,w.y),at)<WorkerRadius(w)){CalmAt(new Vector2(w.x,w.y),WorkerRadius(w),1.8f);w.pulse=1;w.cooldown=7-w.level;if(i==1&&DemonActive)Shelter(i,true);}}}}}
    if(!commanded){var target=LandTarget(w.tx,w.ty);if(i==0){state.targetX=target.x;state.targetY=target.y;}else{var at=new Vector2(w.x,w.y);var d=target-at;float dist=Distance(at,target);at+=d*Mathf.Min(1,dt*.17f/Mathf.Max(.001f,dist));w.x=at.x;w.y=at.y;}}else if(i>0){var at=Vector2.MoveTowards(new Vector2(w.x,w.y),new Vector2(w.tx,w.ty),dt*.28f);w.x=at.x;w.y=at.y;if(w.cooldown<=0){CalmAt(at,.34f+w.level*.025f,4);w.cooldown=.8f;w.pulse=1;}}
   }
   if(state.mode==0)TickDemonV9(dt);
  }
  struct IncomeSample {public float at;public long value;}
  readonly Queue<IncomeSample> recentIncome=new Queue<IncomeSample>();
  void RecordIncome(long value){recentIncome.Enqueue(new IncomeSample{at=state.elapsed,value=value});while(recentIncome.Count>0&&recentIncome.Peek().at<state.elapsed-30)recentIncome.Dequeue();}
  public float PassiveRate(){
   while(recentIncome.Count>0&&recentIncome.Peek().at<state.elapsed-30)recentIncome.Dequeue();
   float observed=recentIncome.Count>0?(float)recentIncome.Sum(s=>s.value)/Mathf.Clamp(state.elapsed,10,30):0;
   // A reloaded session uses its earned average until fresh arrivals are available.
   if(recentIncome.Count==0&&state.boarded>0)observed=(float)Math.Max(0,state.total-state.offlineTotal)/Mathf.Max(30,state.elapsed);
   float sum=0,value=0;for(int i=0;i<config.animals.Length;i++){float w=Weight(i);sum+=w;value+=w*Value(i);}float arrivals=Batch/Interval*(.30f+state.workers.Count*.07f);
   float ceiling=Mathf.Min(arrivals,DockCapacity/RampSeconds)*(value/Mathf.Max(1,sum));
   // Staff continue earning during an open background session, even before the first arrival.
   return Mathf.Min(ceiling,Mathf.Max(ceiling*.55f,observed*.65f));
  }
  // Runtime-only session clock. Deserializing/reopening never creates away income.
  long backgroundStarted;float backgroundRate;
  public void BeginBackground(long utc){if(utc>0&&backgroundStarted==0){backgroundStarted=utc;backgroundRate=PassiveRate();}}
  public long ApplyAway(long utc){long previous=backgroundStarted;backgroundStarted=0;state.lastUtc=0;if(previous<=0||utc<=previous||state.mode!=0||state.completed&&!state.freeplay)return 0;long secs=Math.Min(600,utc-previous);if(secs<5)return 0;long reward=(long)Math.Floor(backgroundRate*secs);state.available+=reward;state.total+=reward;state.offlineTotal+=reward;state.awayAward=reward;state.awaySeconds=(int)secs;UpdatePhase();return reward;}
  static bool ValidateV8(VoyageState s){if(s.v8==0)return true;return s.area>=0&&s.area<=2&&s.lost>=0&&s.demon>=0&&s.demon<=3&&s.lastUtc>=0&&s.workers!=null&&s.workers.Count>=1&&s.workers.Count<=3&&s.workers.All(w=>w.level>=0&&w.level<=2&&float.IsFinite(w.x)&&float.IsFinite(w.y)&&float.IsFinite(w.rest))&&s.plots!=null&&s.plots.Length==2&&s.plots.All(p=>p!=null&&p.crop>=-1&&p.crop<=2&&float.IsFinite(p.growth)&&float.IsFinite(p.buff));}
 }
}
