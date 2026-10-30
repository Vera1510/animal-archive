using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AnimalArchive.Voyage {
 [Serializable] public class Species {public string name,species,lore;public int rarity,value,tier,atlas,row;public float weight;}
 [Serializable] public class Growth {public string id,name,description;public int[] costs;public int[] tiers;}
 [Serializable] public class VoyageConfig {public int version=6;public Species[] animals;public Growth[] upgrades;public float baseInterval,frequencyFactor,travelMin,travelMax,valueStep,uncommonStep,rareStep;public int maxActive,automationAt,discoveryAt,middleAt,twilightAt,lanternCost,finalTotal,finalBoarded;public string[] stages;}
 [Serializable] public partial class Passenger {public long id;public int species,reward;public float age,duration,startX,startY,bendX,bendY,lane,animOffset;public bool invitation;public float calm,fright;public int dockLevel;public bool admitted;public float pathWidth=.026f;[NonSerialized] public Vector2[] route;[NonSerialized] public float[] lengths;[NonSerialized] public float routeLength;}
 [Serializable] public partial class VoyageState {
  public int version=6;public uint rng=132941;public long available,total,spent,boarded,nextId,paidPrefix;public List<long> paidTail=new List<long>();public List<Passenger> active=new List<Passenger>();public int[] levels=new int[4];public int discovered,readEvents,phase;public float elapsed,spawnClock=.6f,autoClock;public bool autoEnabled,lantern,invitationSent,completed,freeplay;public int autoFocus=4;public long autoSpent;public int autoPurchases;public int mode;public float sailTime;public long endTotal,endBoarded;public int endDiscovered;public float endTime;public int peakActive;public float guideX=.43f,guideY=.68f,targetX=.43f,targetY=.68f,guideCooldown,guidePulse,crisisTime,waveTime;public int guideUses,calmed,anchors,crisis;public bool guideSeen,guideAuto,protection;public float patrolClock;public float tapCooldown,tapPulse,gateClock,shelterCooldown,shelterTime;public int taps,skillUses,tutorial,lesson,narrated;
 }
 public struct Arrival {public long id;public int species,value;public bool first,invitation;}
 public partial class VoyageModel {
  public readonly VoyageConfig config;public VoyageState state;public readonly List<Arrival> arrivals=new List<Arrival>();public readonly List<string> events=new List<string>();
  public VoyageModel(VoyageConfig cfg,VoyageState saved=null){config=cfg;state=saved??new VoyageState();InitV8();InitV10();CompactLedger();foreach(var p in state.active)p.dockLevel=state.levels[2];if(saved==null){for(int i=0;i<3;i++){var p=Spawn(i==2?3:i);p.startX=.24f+i*.05f;p.startY=.63f+i*.04f;p.x=p.startX;p.y=p.startY;p.tx=p.x;p.ty=p.y;}state.spawnClock=2.8f;}}
  public float Interval=>config.baseInterval*Mathf.Pow(config.frequencyFactor,state.levels[0]);public int Batch=>1+state.levels[1]+(state.crisis==2?2:0)+(state.demon==3?1:0);public float Multiplier=>ValueScale(state.levels[2])*(state.crisis==2?8:1)*(state.demon==3?1.4f:1);
  public float ValueScale(int level)=>(1+config.valueStep*Mathf.Min(level,2))*Mathf.Pow(2,Mathf.Max(0,level-2));
  public bool GuideUnlocked=>true;public bool GuideAutoReady=>false;public bool CrisisActive=>state.crisis==1;public float Efficiency=>1+state.levels[3]*.08f;public int Lanes=>DockCapacity;public float DockWidth=>.026f+Math.Min(5,state.levels[2])*.018f;public float SkillRadius=>.24f+state.levels[3]*.045f;public float SkillCooldown=>Mathf.Max(5,12-state.levels[3]);public bool StormWarning=>state.crisis==0&&state.total>=3500;
  public static readonly Vector2[] Beacons={new Vector2(.40f,.59f),new Vector2(.57f,.71f),new Vector2(.67f,.57f)};
  public int Tier=>state.lantern||state.total>=12000?3:state.total>=1800?2:state.total>=100?1:0;
  public bool AutoUnlocked=>state.workers.Count>1;
  public bool TwilightReady=>state.crisis==2&&!state.lantern;
  public bool CanSail=>!state.completed&&state.mode==0&&state.total>=config.finalTotal&&state.demon==3;
  public long Reserve=>!state.lantern&&state.total>=config.twilightAt*.6f?config.lanternCost:120;
  public int Discovered=>Enumerable.Range(0,config.animals.Length).Count(i=>(state.discovered&(1<<i))!=0);
  public bool Known(int i)=>(state.discovered&(1<<i))!=0;public bool Eligible(int i)=>config.animals[i].tier<=Tier&&PlantRequirement(i);
  public long Cost(int i)=>state.levels[i]>=config.upgrades[i].costs.Length?-1:config.upgrades[i].costs[state.levels[i]];
  public bool Unlocked(int i){int lv=state.levels[i];var u=config.upgrades[i];return lv<u.costs.Length&&(u.tiers==null||u.tiers.Length<=lv||Tier>=u.tiers[lv]);}
  public bool Buy(int i,bool automatic=false){if(i<0||i>=4||state.mode!=0||automatic)return false;long cost=Cost(i);if(cost<0||!Unlocked(i)||state.available<cost||(automatic&&(!state.autoEnabled||!AutoUnlocked||state.available-cost<Reserve)))return false;state.available-=cost;state.spent+=cost;state.levels[i]++;if(i==2)foreach(var p in state.active){p.pathWidth=DockWidth;p.dockLevel=state.levels[2];p.route=null;}if(automatic){state.autoSpent+=cost;state.autoPurchases++;}events.Add("upgrade:"+i+(automatic?":auto":""));return true;}
  public bool LightLantern(){if(!TwilightReady||state.available<config.lanternCost||state.mode!=0)return false;state.available-=config.lanternCost;state.spent+=config.lanternCost;state.lantern=true;state.protection=true;events.Add("lantern");UpdatePhase();return true;}
  public bool SetAuto(bool on,int focus){state.autoEnabled=false;return false;}
  public bool RequestSail(){if(!CanSail)return false;state.mode=1;state.autoEnabled=false;events.Add("draining");return true;}
  public bool Continue(){if(!state.completed||state.mode!=3)return false;state.freeplay=true;state.mode=0;state.spawnClock=.8f;events.Add("freeplay");return true;}
  float Random01(){uint x=state.rng;if(x==0)x=132941;x^=x<<13;x^=x>>17;x^=x<<5;state.rng=x;return (x&0xFFFFFF)/16777216f;}
  public float Weight(int i){var a=config.animals[i];if(!Eligible(i))return 0;return CropWeight(i)*a.weight*(a.rarity==0?1:a.rarity==1?1+config.uncommonStep*(state.levels[1]+Tier):1+config.rareStep*(state.levels[1]+Tier));}
  public float Probability(int i){float sum=0;for(int j=0;j<config.animals.Length;j++)sum+=Weight(j);return sum>0?Weight(i)/sum:0;}
  public int Choose(){float sum=0;for(int i=0;i<config.animals.Length;i++)sum+=Weight(i);float roll=Random01()*sum;for(int i=0;i<config.animals.Length;i++){roll-=Weight(i);if(roll<0)return i;}return 0;}
  public int Value(int i)=>Mathf.RoundToInt(config.animals[i].value*Multiplier);
  Passenger Spawn(int species,bool invitation=false,float delay=0,bool bottom=false,int slot=0,float spread=0){float sx=LandMin-.10f-(slot/3)*.028f;float sy=.52f+spread*.16f+(slot%3)*.025f;var p=new Passenger{id=++state.nextId,species=species,reward=Value(species),age=-delay,duration=config.travelMin+Random01()*(config.travelMax-config.travelMin),startX=sx,startY=sy,bendX=.25f+Random01()*.30f,bendY=.52f+Random01()*.24f,pathWidth=DockWidth,dockLevel=state.levels[2],lane=(Random01()-.5f)*2,animOffset=Random01()*4,invitation=invitation};ConfigurePassenger(p);state.active.Add(p);return p;}
  public bool Settle(Passenger p){if(p==null||p.age<p.duration||!state.active.Contains(p))return false;if(p.berth>=0)dockMask&=~(1<<p.berth);state.active.Remove(p);bool first=!Known(p.species);state.available+=p.reward;state.total+=p.reward;state.boarded++;CompactLedger();RecordIncome(p.reward);state.discovered|=1<<p.species;arrivals.Add(new Arrival{id=p.id,species=p.species,value=p.reward,first=first,invitation=p.invitation});return true;}
  // Keep visible-play time independent of frame rate, without a large offline catch-up.
  public void AdvanceFrame(float seconds){if(seconds<=0||float.IsNaN(seconds)||float.IsInfinity(seconds))return;float remaining=Mathf.Min(seconds,2f);while(remaining>0){float step=Mathf.Min(.1f,remaining);Tick(step);remaining-=step;}}
  public void Tick(float dt){if(dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;dt=Mathf.Min(dt,.25f);if(state.mode==3)return;state.elapsed+=dt;TickGuide(dt);TickV8(dt);
   if(state.mode==1)state.drainTime+=dt;
   BeginFlowFrame();for(int i=0;i<state.active.Count;i++){var p=state.active[i];TickFlow(p,dt);if(p.age>=p.duration&&Settle(p))i--;}
   if(state.mode==0){state.spawnClock-=dt;if(state.spawnClock<=0){int count=Math.Max(0,Math.Min(Batch,ActiveLimit-state.active.Count));bool bottom=Random01()>.52f;float spread=Random01();for(int k=0;k<count;k++)Spawn(Choose(),false,k*.26f,bottom,k,spread);state.spawnClock+=Interval;if(state.spawnClock<=0)state.spawnClock=Interval;}
    if(Tier>=1&&!state.invitationSent&&state.active.Count<ActiveLimit){state.invitationSent=true;Spawn(4,true);events.Add("invitation");}
    
   }else if(state.mode==1&&state.active.Count==0){state.mode=2;state.sailTime=0;events.Add("sailing");}else if(state.mode==2){state.sailTime+=dt;if(state.sailTime>=8){state.mode=3;state.completed=true;state.endTime=state.elapsed;state.endTotal=state.total;state.endBoarded=state.boarded;state.endDiscovered=Discovered;events.Add("ending");}}
   state.peakActive=Math.Max(state.peakActive,state.active.Count);UpdatePhase();
  }
  static readonly float[] Speeds={1.1f,.85f,1.2f,1.4f,1.25f,1.12f,1.3f,.92f,.65f,.95f,.88f,1.35f};public static float SpeciesSpeed(int species)=>Speeds[species];
  public static float StormSpeed(int species)=>species==5||species==7||species==10?1:species==4?.85f:species==2||species==11?.9f:species==8?.8f:species==1||species==3||species==6?.48f:.65f;
  public static string Trait(int i)=>new[]{"軽い足取り · 風で少し慎重に","群れでゆっくり · 強風が苦手","速い足取り · 風に強い","跳ねて進む · 驚くと寄り道","空から飛来 · 風でもほぼ一定","水辺の得意な子 · 嵐でも平気","大きな跳躍 · 驚くと寄り道","短い歩幅 · 水しぶきに平気","ゆっくり着実 · あまり慌てない","ゆったり歩く · 合図で安心","低い姿勢 · 嵐でも平気","速い足取り · 風に強い"}[i];
  // Door vertices are calibrated against the actual image sill, not a separate imaginary arch.
  public static Rect BoatRect(int level){float w=.30f+Math.Min(5,level)*.030f,h=w*1.185185f;return new Rect(.8f-w*.43f,.45f-h*.83f,w,h);}
  public static Vector2 Door(int level,float lane){Rect r=BoatRect(level);Vector2 a=level==0?new Vector2(.335f,.786f):new Vector2(.233f,.780f),b=level==0?new Vector2(.510f,.832f):new Vector2(.653f,.867f);Vector2 uv=Vector2.Lerp(a,b,(lane+1)*.5f);return new Vector2(r.x+uv.x*r.width,r.y+uv.y*r.height);}
  public static Vector2 RampFoot(int level,float lane)=>Door(level,lane)+new Vector2(-.11f,.16f);
  public static Vector2 Position(Passenger p){
   if(p.motion>0)return new Vector2(p.x,p.y);
   float t=Mathf.Clamp01(p.age/p.duration);float lane=p.lane*.86f;Vector2 foot=RampFoot(p.dockLevel,lane),door=Door(p.dockLevel,lane);
   if(t>=.78f)return Vector2.Lerp(foot,door+new Vector2(.006f,-.015f),(t-.78f)/.22f);
   Vector2 start=new Vector2(p.startX,p.startY),bend=new Vector2(p.bendX,p.bendY);float u=t/.78f;
   Vector2 pos=(1-u)*(1-u)*start+2*(1-u)*u*bend+u*u*foot;
   // Timid animals make continuous side excursions only on land, never off the bridge.
   float excursion=p.fright*(p.species==1||p.species==3||p.species==6?.04f:.008f)*Mathf.Sin(u*Mathf.PI);
   pos+=new Vector2(Mathf.Sin(p.age*2+p.id)*excursion,Mathf.Cos(p.age*1.7f+p.id)*excursion*.65f);pos+=new Vector2(Mathf.Sin(p.wanderClock*1.2f+p.id)*.026f,Mathf.Cos(p.wanderClock*.8f+p.id)*.019f)*p.roamBlend;return pos;
  }
  static float Distance(Vector2 a,Vector2 b){Vector2 d=a-b;d.x*=1.7778f;return d.magnitude;}
  public bool Disturbed(Passenger p){if(StormSpeed(p.species)>=1||Protected(p)||(!CrisisActive&&state.waveTime<=0))return false;Vector2 pos=Position(p);for(int i=0;i<3;i++)if((!CrisisActive||(state.anchors&(1<<i))==0)&&Distance(pos,Beacons[i])<.27f)return true;return false;}
  public static Vector2 GuideTarget(float x,float y){x=Mathf.Clamp(x,.04f,.87f);y=Mathf.Clamp(y,.43f,.88f);if(x>.66f)y=Mathf.Max(y,.64f+(x-.66f)*.38f);return new Vector2(x,y);}
  public bool MoveGuide(float x,float y){if(!GuideUnlocked||state.mode!=0||!float.IsFinite(x)||!float.IsFinite(y))return false;Vector2 target=LandTarget(x,y);state.manualUntil=state.elapsed+4;if(state.workers.Count>0){state.workers[0].rest=0;state.workers[0].work=Mathf.Min(state.workers[0].work,18);}state.targetX=target.x;state.targetY=target.y;state.patrolClock=-8;return true;}
  public bool CallGuide(bool automatic=false,int role=0){if(!GuideUnlocked||state.mode!=0||state.guideCooldown>0)return false;state.guideCooldown=automatic?12:SkillCooldown;state.guidePulse=role==0?1.6f:0;role=Mathf.Clamp(role,0,state.workers.Count-1);var worker=state.workers[role];worker.pulse=1;Vector2 at=role==0?new Vector2(state.guideX,state.guideY):new Vector2(worker.x,worker.y);int count=0;foreach(var p in state.active)if(p.age>=0&&Distance(at,Position(p))<GuideRadius){p.calm=8+state.levels[3];p.roam=0;count++;}state.calmed+=count;if(count==0&&!CrisisActive){state.guideCooldown=0;state.guidePulse=.5f;events.Add("guide-empty");return false;}events.Add("guide-hit:"+count);if(!automatic){state.guideUses++;state.skillUses++;state.lesson=Math.Max(state.lesson,1);}
   if(CrisisActive&&!automatic)for(int i=0;i<3;i++)if(Distance(at,Beacons[i])<SkillRadius*.8f&&(state.anchors&(1<<i))==0){state.anchors|=1<<i;events.Add("beacon");}
   events.Add(automatic?"guide:auto":"guide");if(state.anchors==7&&CrisisActive)ResolveCrisis();return true;
  }
  public bool Tap(float x,float y){if(state.mode!=0||state.tapCooldown>0||!float.IsFinite(x)||!float.IsFinite(y))return false;state.tapCooldown=.14f;state.tapPulse=.35f;state.taps++;state.tutorial=Math.Max(1,state.tutorial);MoveGuide(x,y);Vector2 at=new Vector2(state.guideX,state.guideY);int affected=0;foreach(var p in state.active)if(p.age>=0&&Distance(at,Position(p))<.34f){p.calm=Mathf.Max(p.calm,2.4f+state.levels[3]*.35f);p.roam=Mathf.Max(0,p.roam-Response(p.species));affected++;}state.calmed+=affected;events.Add("tap");return true;}
  public bool Shelter(int role=0,bool automatic=false){if(state.mode!=0||state.shelterCooldown>0||state.levels[3]<1)return false;role=Mathf.Clamp(role,0,state.workers.Count-1);var w=state.workers[role];state.shieldX=role==0?state.guideX:w.x;state.shieldY=role==0?state.guideY:w.y;state.shelterCooldown=Mathf.Max(15,28-state.levels[3]*2);state.shelterTime=7+state.levels[3];if(!automatic){state.skillUses++;state.lesson=Math.Max(2,state.lesson);}events.Add("shelter");return true;}
  public bool BuyGuideAuto(){if(!GuideAutoReady||state.available<1500||state.mode!=0)return false;state.available-=1500;state.spent+=1500;state.guideAuto=true;events.Add("guide-automatic");return true;}
  void ResolveCrisis(){state.crisis=2;state.crisisTime=0;state.anchors=7;state.waveTime=0;events.Add("crisis-clear");}
  void TickGuide(float dt){
   if(!state.guideSeen)state.guideSeen=true;state.tapCooldown=Mathf.Max(0,state.tapCooldown-dt);state.tapPulse=Mathf.Max(0,state.tapPulse-dt);state.shelterCooldown=Mathf.Max(0,state.shelterCooldown-dt);state.shelterTime=Mathf.Max(0,state.shelterTime-dt);
   state.guideCooldown=Mathf.Max(0,state.guideCooldown-dt);state.guidePulse=Mathf.Max(0,state.guidePulse-dt);
   Vector2 pos=new Vector2(state.guideX,state.guideY),target=new Vector2(state.targetX,state.targetY);Vector2 d=target-pos;float distance=Distance(pos,target);if(distance>0)pos+=d*Mathf.Min(1,dt*.22f/distance);state.guideX=pos.x;state.guideY=pos.y;
   if(state.mode!=0)return;
   if(state.crisis==0&&state.total>=5000){state.crisis=1;state.crisisTime=0;state.anchors=0;events.Add("crisis");}
   if(CrisisActive){state.crisisTime+=dt;int natural=Mathf.Min(3,(int)(state.crisisTime/30));for(int i=0;i<natural;i++)state.anchors|=1<<i;if(state.anchors==7)ResolveCrisis();}
   if(state.crisis==2){state.crisisTime+=dt;state.waveTime=Mathf.Max(0,state.waveTime-dt);if(state.crisisTime>=55){state.crisisTime=0;state.waveTime=state.protection?2:14;events.Add(state.protection?"protected":"afterwave");}}
  }
  void UpdatePhase(){int phase=state.completed?6:CanSail?5:state.crisis==2?4:CrisisActive?3:AutoUnlocked?2:Tier>=1?1:0;if(phase>state.phase){state.phase=phase;events.Add("phase:"+phase);}}
  public static bool Validate(VoyageConfig cfg,VoyageState s,out string reason){reason="";if(s==null||s.version!=6||s.levels==null||s.levels.Length!=4||s.active==null||s.paidTail==null){reason="schema";return false;}if(s.available<0||s.spent<0||s.total!=s.available+s.spent||s.boarded<0||s.nextId<0||s.paidPrefix<0||s.ledgerV10<0||s.ledgerV10>1||s.boarded+s.lost!=s.paidPrefix+s.paidTail.Count||s.ledgerV10==1&&s.paidTail.Count!=0){reason="ledger";return false;}var paid=new HashSet<long>(s.paidTail);if(paid.Count!=s.paidTail.Count||paid.Any(x=>x<=s.paidPrefix||x>s.nextId)){reason="duplicate payment";return false;}var active=new HashSet<long>();foreach(var p in s.active){if(p.id<=0||s.ledgerV10==0&&p.id<=s.paidPrefix||p.id>s.nextId||paid.Contains(p.id)||!active.Add(p.id)||p.species<0||p.species>=cfg.animals.Length||p.duration<=0||float.IsNaN(p.duration)||float.IsInfinity(p.duration)||float.IsNaN(p.age)||float.IsInfinity(p.age)||p.age>=p.duration||p.reward<0){reason="passenger";return false;}}
   if(s.boarded+s.lost+s.active.Count!=s.nextId||s.mode<0||s.mode>3||s.elapsed<0||float.IsNaN(s.elapsed)||float.IsInfinity(s.elapsed)||s.active.Count>cfg.maxActive){reason="lifecycle";return false;}if(s.autoFocus<0||s.autoFocus>4||s.phase<0||s.phase>6||s.discovered<0||s.discovered>=(1<<cfg.animals.Length)||s.autoSpent<0||s.autoSpent>s.spent||s.autoPurchases<0){reason="flags";return false;}for(int i=0;i<4;i++)if(s.levels[i]<0||s.levels[i]>cfg.upgrades[i].costs.Length){reason="upgrade";return false;}if(s.crisis<0||s.crisis>2||s.anchors<0||s.anchors>7||!float.IsFinite(s.guideX)||!float.IsFinite(s.guideY)||!float.IsFinite(s.crisisTime)||s.guideUses<0||s.taps<0||!float.IsFinite(s.gateClock)||!float.IsFinite(s.shelterTime)){reason="guide/crisis";return false;}if(s.active.Any(p=>p.motion>0&&(!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.vx)||!float.IsFinite(p.vy)||!float.IsFinite(p.life)||!float.IsFinite(p.boarding)))){reason="motion";return false;}if(!ValidateV8(s)){reason="v8";return false;}if(s.mode==3&&!s.completed){reason="ending";return false;}return true;
  }
 }
}

