using System;
using System.Linq;
using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class Passenger {
  // Persist actual world motion. Age is retained only for delayed spawn / boarding progress.
  public int berth=-1;public float waitTime,pause; public int motion; public float x,y,vx,vy,tx,ty,decision,life,visit,boarding; public bool fieldVisited,heading,queued;
 }
 public partial class VoyageState {public int v10,bossCycle;public long lostValue;public float bossStun,bossX,bossY,shieldX,shieldY;public float demonX=.08f,demonY=.30f,demonHit,attackPulse,drainTime;public long demonTarget;}
 public partial class VoyageModel {
  public float AttractionRadius=>.23f+state.levels[2]*.028f;
  public float GuideRadius=>.36f+state.levels[3]*.045f;
  public float WorkerRadius(Worker w)=>.24f+w.level*.025f;
  public int DockCapacity=>3*(1+state.levels[2]);
  public int DockOccupied=>state.active.Count(p=>p.admitted&&p.berth>=0);
  public float DockRate(int level)=>60*3*(1+level)/(2.6f/(1+level*.12f));
  public bool Protected(Passenger p)=>state.shelterTime>0&&Distance(Position(p),new Vector2(state.shieldX,state.shieldY))<SkillRadius||state.ward>0&&Distance(Position(p),new Vector2(state.guideX,state.guideY))<SkillRadius;
  void InitV10(){if(state.v10==10)return;state.v10=10;state.lastUtc=0;state.awayAward=0;int slot=0;foreach(var p in state.active){p.berth=-1;if(p.admitted&&slot<DockCapacity){p.berth=slot++;p.lane=LaneFor(p.berth);}else if(p.admitted){p.admitted=false;p.boarding=0;p.age=0;var at=LandTarget(p.x,.68f);p.x=at.x;p.y=at.y;}}if(state.demon==2)state.demonHp=180;}
  float LaneFor(int slot)=>DockCapacity==1?0:Mathf.Lerp(-.78f,.78f,(float)slot/(DockCapacity-1));
  int dockMask;
  void BeginFlowFrame(){dockMask=0;foreach(var p in state.active)if(p.admitted&&p.berth>=0)dockMask|=1<<p.berth;}
  int FreeBerth(Vector2 at){int best=-1;float distance=float.MaxValue;for(int i=0;i<DockCapacity;i++){if((dockMask&(1<<i))!=0)continue;float d=Distance(at,RampFoot(state.levels[2],LaneFor(i)*.86f));if(d<distance){best=i;distance=d;}}return best;}
  Vector2 WanderTarget(){return LandTarget(LandMin+.06f+Random01()*(.61f-LandMin),.47f+Random01()*.29f);}

  public float DockAssist {get {var foot=RampFoot(state.levels[2],0);foreach(var w in state.workers){bool manual=w.manual>0||(w.role==0&&state.manualUntil>state.elapsed);var at=w.role==0?new Vector2(state.guideX,state.guideY):new Vector2(w.x,w.y);if(manual&&Distance(at,foot)<GuideRadius)return 1.7f;}return 1;}}
  public float RampSeconds=>2.6f/(1+state.levels[2]*.12f);
  public static bool LikesCrop(int species,int crop)=>crop==0?(species==1||species==3||species==9):crop==1?(species==0||species==2||species==6||species==11):(species==4||species==5||species==10);
  public bool HasPlant(int crop)=>state.plots.Take(state.area).Any(p=>p.crop==crop&&p.growth>=.65f);
  public bool PlantRequirement(int species)=>species==10?HasPlant(2):species==11?HasPlant(1):true;
  public string PlantHint(int species)=>species==10?"月の花を育てると現れる":species==11?"木の実の木を育てると現れる":"";
  void InitFlow(Passenger p,bool fresh){
   if(p.motion>0)return;
   Vector2 at=fresh?LandTarget(LandMin+.08f+Random01()*(.52f-LandMin),.48f+Random01()*.27f):Position(p);
   p.x=at.x;p.y=at.y;p.motion=9;p.life=fresh?0:Mathf.Max(0,p.age);p.decision=.2f+Random01()*2;
   p.tx=p.x;p.ty=p.y;p.roam=3+Random01()*5;
   if(!fresh&&p.age>=p.duration*.78f){p.admitted=true;p.boarding=Mathf.InverseLerp(p.duration*.78f,p.duration,p.age);}
   else p.age=Mathf.Min(0,p.age);
  }
  int FieldFor(Passenger p){int best=-1;float distance=float.MaxValue;for(int i=0;i<state.area;i++){var plot=state.plots[i];if(plot.crop<0||plot.growth<.45f||!LikesCrop(p.species,plot.crop))continue;float d=Distance(Position(p),PlotPoint(i));if(d<distance){best=i;distance=d;}}return best;}
  int BestCrop(int plot){
   // Keep complementary habitats. Late rarities require actual living trees/flowers.
   if(state.area==2)return plot==1?2:1;
   int grazers=state.active.Count(p=>LikesCrop(p.species,0)),forest=state.active.Count(p=>LikesCrop(p.species,1));
   return state.total>=1800||forest>grazers?1:0;
  }
  void TickFlow(Passenger p,float dt){
   InitFlow(p,false);if(p.age<0){p.age=Mathf.Min(0,p.age+dt);return;}
   p.life+=dt;p.wanderClock+=dt;p.calm=Mathf.Max(0,p.calm-dt);p.pause=Mathf.Max(0,p.pause-dt);
   var at=new Vector2(p.x,p.y);var center=RampFoot(p.dockLevel,0);
   if(p.berth>=0)p.lane=LaneFor(p.berth);
   var foot=RampFoot(p.dockLevel,p.lane*.86f);
   if(p.admitted){p.boarding=Mathf.Min(1,p.boarding+dt/RampSeconds*(state.mode==1?5:DockAssist));at=Vector2.Lerp(foot,Door(p.dockLevel,p.lane*.86f)+new Vector2(.006f,-.015f),p.boarding);p.x=at.x;p.y=at.y;p.vx=p.vy=0;p.age=p.duration*(.78f+.22f*p.boarding);return;}
   float response=Response(p.species);
   foreach(var w in state.workers){if(w.rest>0)continue;Vector2 wp=w.role==0?new Vector2(state.guideX,state.guideY):new Vector2(w.x,w.y);if(Distance(at,wp)<WorkerRadius(w))p.calm=Mathf.Max(p.calm,.9f+response*.5f);}
   bool danger=Disturbed(p)&&p.calm<=0;p.fright=Mathf.MoveTowards(p.fright,danger?1:0,dt*2);
   bool captured=Distance(at,center)<AttractionRadius;
   int field=FieldFor(p);bool feeding=field>=0&&!p.fieldVisited&&p.calm<=0&&!captured&&state.mode==0;
   // Reaching the dock is a persistent intention; a temporary speed boost may expire.
   p.queued|=p.calm>0||captured||p.heading||p.berth>=0||p.waitTime>0||state.mode==1;
   p.heading=p.queued;p.berth=-1;
   float speed=.067f*SpeciesSpeed(p.species)*Efficiency;
   if(p.heading){
    p.pause=0;p.roamBlend=0;speed*=p.calm>0?2.8f:1.3f;
    int slot=FreeBerth(at);
    float lane=slot>=0?LaneFor(slot):LaneFor((int)(p.id%DockCapacity));
    foot=RampFoot(p.dockLevel,lane*.86f);
    // Land animals never reserve bridge capacity before physically reaching its foot.
    if(slot>=0&&Distance(at,foot)<.055f){p.berth=slot;dockMask|=1<<slot;p.lane=lane;p.admitted=true;p.boarding=0;p.x=foot.x;p.y=foot.y;p.age=p.duration*.78f;p.vx=p.vy=0;return;}
    var target=foot;
    if(slot<0){int ahead=0;float d=Distance(at,center);foreach(var other in state.active)if(other!=p&&!other.admitted&&other.queued&&Distance(Position(other),center)<d)ahead++;int row=Math.Min(4,ahead/Math.Max(3,DockCapacity));target+=new Vector2(-.014f*(row+1),.021f*(row+1));p.waitTime+=dt;}else p.waitTime=0;
    p.tx=target.x;p.ty=target.y;
   }else if(feeding){var fp=PlotPoint(field)+new Vector2(Mathf.Sin(p.id*2.4f)*.070f,Mathf.Cos(p.id)*.065f);p.tx=fp.x;p.ty=fp.y;if(Distance(at,fp)<.09f){p.visit+=dt;speed*=.18f;if(p.visit>3.5f){p.fieldVisited=true;p.decision=0;p.pause=.8f;}}p.roamBlend=.5f;}
   else {p.decision-=dt;var aim0=new Vector2(p.tx,p.ty);if(p.decision<=0||Distance(at,aim0)<.025f){var target=WanderTarget();p.tx=target.x;p.ty=target.y;p.decision=5+Random01()*6;p.pause=.15f+Random01()*.6f;}speed*=p.pause>0?0:.75f;p.roamBlend=1;}
   if(danger){speed*=StormSpeed(p.species);if(p.species==1||p.species==3||p.species==6)p.ty+=Mathf.Sin(p.life*3+p.id)*.018f;}
   if(state.mode==1)speed=Mathf.Max(.8f,speed*4);
   Vector2 aim=new Vector2(p.tx,p.ty),direction=aim-at;
   if(direction.sqrMagnitude>.00001f)direction/=Mathf.Max(.001f,Distance(at,aim));
   Vector2 separate=Vector2.zero;
   foreach(var other in state.active){if(other==p||other.admitted||other.motion==0||other.age<0)continue;var op=new Vector2(other.x,other.y);Vector2 diff=at-op;float d=Distance(at,op);if(d<.075f){if(d<.001f)diff=new Vector2(Mathf.Sin(p.id),Mathf.Cos(p.id))*.001f;separate+=diff/Mathf.Max(.001f,d)*(1-d/.075f);}}
   // Avoidance may spread a moving crowd sideways, but must not cancel forward motion.
   Vector2 sideways=Vector2.ClampMagnitude(separate,1);
   if(direction.sqrMagnitude>.00001f){float backwards=Vector2.Dot(sideways,direction);if(backwards<0)sideways-=direction*(backwards/direction.sqrMagnitude);}
   Vector2 desired=direction*speed+sideways*Mathf.Min(.025f,speed*.3f);
   Vector2 vel=Vector2.Lerp(new Vector2(p.vx,p.vy),desired,Mathf.Min(1,dt*6));
   if(Distance(at,aim)<.014f)vel*=.45f;
   var next=at+vel*dt;
   if(p.berth>=0&&Distance(at,foot)<=Mathf.Max(.026f,Distance(at,next)+.012f)){p.admitted=true;p.boarding=0;p.x=foot.x;p.y=foot.y;p.age=p.duration*.78f;p.vx=p.vy=0;return;}
   next=LandTarget(next.x,next.y);p.vx=vel.x;p.vy=vel.y;p.x=next.x;p.y=next.y;p.age=Mathf.Min(p.duration*.70f,p.life*.1f);
  }
  public float BossRadius=>.24f;
  public Vector2 BossAttackPoint=>new Vector2(state.bossX,state.bossY);
  public bool BossWinding=>DemonActive&&state.attackClock<=1.5f&&state.bossStun<=0;
  void TickDemonV9(float dt){
   state.demonHit=Mathf.Max(0,state.demonHit-dt);state.attackPulse=Mathf.Max(0,state.attackPulse-dt);state.bossStun=Mathf.Max(0,state.bossStun-dt);
   if(state.demon==0&&state.crisis==2&&state.total>=50000&&!state.completed){state.demon=1;state.demonTime=0;state.demonHp=180;state.demonX=.08f;state.demonY=.29f;events.Add("demon-warning");}
   if(state.demon==1){state.demonTime+=dt;var entry=Vector2.Lerp(new Vector2(.08f,.29f),new Vector2(.20f,.52f),Mathf.SmoothStep(0,1,state.demonTime/6));state.demonX=entry.x;state.demonY=entry.y;if(state.demonTime>=6){state.demon=2;state.demonTime=0;state.attackClock=4.8f;events.Add("demon");}}
   if(!DemonActive)return;
   state.demonTime+=dt;if(state.demonTime>=65){ResolveDemon();return;}if(state.bossStun>0)return;
   state.attackClock-=dt;
   if(state.attackClock>1.5f){
    var victim=state.active.Where(p=>p.age>=0&&!p.admitted&&p.x<.62f).OrderBy(p=>Distance(Position(p),DemonPoint)).FirstOrDefault();
    Vector2 dest;
    if(state.attackClock>3.5f){float[] sites={.16f,.50f,Mathf.Max(LandMin+.12f,-.24f),.35f};dest=new Vector2(sites[state.bossCycle%4],state.bossCycle%2==0?.70f:.50f);}
    else dest=victim!=null?Position(victim):new Vector2(.35f,.62f);
    dest.x=Mathf.Clamp(dest.x,LandMin+.08f,.57f);var step=Vector2.MoveTowards(DemonPoint,dest,dt*.23f);state.demonX=step.x;state.demonY=step.y;
    state.bossX=state.demonX;state.bossY=state.demonY;
   }
   if(state.attackClock<=0){state.attackClock=5.6f;state.bossCycle++;state.attackPulse=.65f;events.Add("demon-attack");var victims=state.active.Where(p=>p.age>=0&&!p.admitted&&!Protected(p)&&Distance(Position(p),BossAttackPoint)<BossRadius).OrderBy(p=>Distance(Position(p),BossAttackPoint)).ToArray();foreach(var p in victims)Scatter(p);}
  }
 }
}
