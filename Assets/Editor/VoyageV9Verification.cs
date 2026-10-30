using System;
using System.IO;
using System.Linq;
using UnityEngine;
using AnimalArchive.Voyage;
public static class VoyageV9Verification {
 static int n;static void Check(bool ok,string why){n++;if(!ok)throw new Exception("V9 FAILED: "+why);}
 public static void Run(){n=0;var cfg=JsonUtility.FromJson<VoyageConfig>(File.ReadAllText("Assets/Resources/Voyage/config.json"));
  var m=new VoyageModel(cfg);var p=m.state.active[0];var start=VoyageModel.Position(p);for(int i=0;i<35;i++)m.Tick(.1f);Check(Vector2.Distance(start,VoyageModel.Position(p))>0,"actual continuous world motion");Check(!p.admitted&&p.age<p.duration*.78f,"wandering not secretly boarding");
  var serialized=VoyageSave.Encode(m.state);var reload=new VoyageModel(cfg,VoyageSave.Decode(cfg,serialized));for(int i=0;i<70;i++){m.Tick(.1f);reload.Tick(.1f);}Check(JsonUtility.ToJson(m.state)==JsonUtility.ToJson(reload.state),"motion replay survives reload");
  var old=new VoyageModel(cfg);var oldp=old.state.active[0];oldp.motion=0;oldp.age=oldp.duration*.84f;Vector2 prior=VoyageModel.Position(oldp);old.Tick(.01f);Check(oldp.motion==9&&Vector2.Distance(prior,VoyageModel.Position(oldp))<.02f,"legacy bridge save migrates without jumping");
  var garden=new VoyageModel(cfg,new VoyageState{total=50000,available=50000,boarded=200,nextId=200,paidPrefix=200});garden.Expand();garden.Expand();garden.Hire();garden.Hire();garden.state.lantern=true;
  Check(!garden.Eligible(10)&&!garden.Eligible(11),"rare species require plants");for(int i=0;i<1500;i++){garden.Tick(.1f);garden.events.Clear();garden.arrivals.Clear();}
  Check(garden.state.plots[0].crop==1&&garden.state.plots[1].crop==2,"gardener plants complementary habitats");Check(garden.state.plots.All(f=>f.harvests>0),"gardener maintains both fields");
  garden.state.plots[0].growth=garden.state.plots[1].growth=1;Check(garden.Eligible(10)&&garden.Eligible(11),"grown trees and moonflowers enable rare species");
  var attract=new VoyageModel(cfg);attract.state.area=1;attract.Plant(0,0);attract.state.plots[0].growth=1;var grazer=attract.state.active[1];grazer.x=-.06f;grazer.y=.64f;grazer.tx=-.06f;grazer.ty=.64f;grazer.calm=0;grazer.life=0;attract.state.workers[0].rest=6;for(int i=0;i<12;i++)attract.Tick(.1f);Check(grazer.tx<-.15f&&grazer.x<-.06f,"crop draws compatible animal away from ark toward actual plot");
  attract.state.guideX=grazer.x;attract.state.guideY=grazer.y;attract.state.targetX=grazer.x;attract.state.targetY=grazer.y;attract.state.workers[0].rest=0;attract.Tick(.1f);Check(grazer.heading&&grazer.tx>.5f,"nearby employee redirects animal toward ark");
  float oldRadius=garden.AttractionRadius,oldTime=garden.RampSeconds;garden.Buy(2);Check(garden.AttractionRadius>oldRadius&&garden.RampSeconds<oldTime,"entrance has real attraction and transit improvement");
  var combat=new VoyageModel(cfg);combat.state.crisis=2;combat.state.demon=1;combat.state.demonTime=0;var entry=combat.DemonPoint;for(int i=0;i<30;i++)combat.Tick(.1f);Check(Vector2.Distance(entry,combat.DemonPoint)>.05f&&combat.state.demon==1,"visible gradual demon approach");for(int i=0;i<31;i++)combat.Tick(.1f);Check(combat.DemonActive,"approach enters combat");
  combat.state.workers[0].rest=6;foreach(var a in combat.state.active){a.x=.2f;a.y=.6f;a.calm=0;a.admitted=false;a.heading=false;a.life=0;a.tx=.2f;a.ty=.6f;}combat.state.demonX=.2f;combat.state.demonY=.6f;combat.state.bossX=.2f;combat.state.bossY=.6f;combat.state.attackClock=.01f;long cash=combat.state.total;combat.Tick(.1f);Check(combat.state.lost>0&&combat.state.total==cash,"attack kills nearby unboarded animals without paying");Check(VoyageModel.Validate(cfg,combat.state,out var why),"death ledger "+why);float hp=combat.state.demonHp;for(int i=0;i<9;i++)combat.Pray(.1f);Check(combat.state.demonHp<hp&&combat.state.demonHit>0,"holy hit damage and response");
  var drain=new VoyageModel(cfg);drain.state.mode=1;foreach(var a in drain.state.active){a.x=-.7f;a.y=.75f;a.species=8;a.motion=9;a.age=0;}drain.state.area=2;float workerX=drain.state.guideX;for(int i=0;i<100;i++)drain.Tick(.1f);Check(drain.state.active.Count==0&&drain.state.mode>=2,"distant slow animals drain in ten seconds");Check(drain.state.guideX!=workerX,"employees continue final guidance");
  var off=new VoyageModel(cfg);off.BeginBackground(1000);int count=off.state.active.Count;Vector2 pos=VoyageModel.Position(off.state.active[0]);off.ApplyAway(1100);Check(off.state.active.Count==count&&VoyageModel.Position(off.state.active[0])==pos,"offline income does not simulate animals");
  string report="V9 model verification / "+DateTime.UtcNow.ToString("o")+"\nPASS "+n+" checks\nMotion, persistence, legacy migration, spatial plant requirements, gardener autonomy, entrance upgrades, demon approach/death/hit, bounded final boarding, low-cost offline settlement.\n";Directory.CreateDirectory("../Evidence/V102");File.WriteAllText("../Evidence/V102/flow-verification.txt",report);Debug.Log(report);
 }
}
