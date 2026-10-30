using System;
using System.Linq;
using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class VoyageGame {
  int fieldNotice=-1;float fieldNoticeUntil;string fieldNoticeText="";float lossFlash;long lastLostValue;
  void FocusPlot(int i){camX=model.PlotPoint(i).x;camY=.5f;zoom=1;ClampCamera();fieldNoticeUntil=0;}
  void EventV10(string e){
   if(e.StartsWith("field-ready:")){fieldNotice=int.Parse(e.Split(':')[1]);fieldNoticeUntil=Time.unscaledTime+12;fieldNoticeText=VoyageModel.CropNames[S.plots[fieldNotice].crop]+"が実った";}
   if(e.StartsWith("field-rare:")){fieldNotice=int.Parse(e.Split(':')[1]);fieldNoticeUntil=Time.unscaledTime+12;fieldNoticeText="畑に珍しい仲間";}
   if(e=="scatter"){lossFlash=Time.unscaledTime+1.6f;lastLostValue=S.lostValue;}
   if(e=="demon-clear"){S.prayer=0;praying=false;}
  }
  void RegionalHint(){
   if(panel!=""||S.mode!=0)return;
   if(fieldNotice>=0&&fieldNoticeUntil>Time.unscaledTime){Vector2 point=model.PlotPoint(fieldNotice);float sx=(point.x-camX)*zoom*W+W*.5f;if(sx<130||sx>W-90){Rect hint=new Rect(20,195,200,40);if(Button(hint,"← "+fieldNoticeText,true,false,14))FocusPlot(fieldNotice);}}
   if(model.DemonActive){float sx=(model.DemonPoint.x-camX)*zoom*W+W*.5f;if(sx<60||sx>W-60){if(Button(new Rect(W*.5f-95,88,190,34),sx<60?"← 悪魔の位置へ":"悪魔の位置へ →",true,false,14)){camX=model.DemonPoint.x;ClampCamera();}}}
  }
  void AreaRing(Vector2 at,float range,Color color,float thickness=3){Matrix4x4 old=GUI.matrix;float factor=H*1.7778f/(W*.49f);GUI.matrix=old*Matrix4x4.TRS(new Vector3(at.x,at.y,0),Quaternion.identity,new Vector3(1,factor,1))*Matrix4x4.Translate(new Vector3(-at.x,-at.y,0));Ring(at,range*W/1.7778f,color,thickness);GUI.matrix=old;}
  void WorldFeedbackV10(){
   // Three depth positions per physical lane, with one small light per occupied berth.
   int slots=model.DockCapacity;
   if(selectedWorker>=0&&selectedWorker<S.workers.Count&&panel==""){var w=S.workers[selectedWorker];bool guiding=w.manual>0||(w.role==0&&S.manualUntil>S.elapsed);if(guiding){var wp=w.role==0?new Vector2(S.guideX,S.guideY):new Vector2(w.x,w.y);AreaRing(World(wp.x,wp.y),model.WorkerRadius(w),new Color(teal.r,teal.g,teal.b,.25f),2);}}
   if(model.DockAssist>1){var a=VoyageModel.RampFoot(S.levels[2],0);var at=World(a.x,a.y);Text(new Rect(at.x-65,at.y+24,130,22),"案内で乗船加速",13,teal,TextAnchor.MiddleCenter,true);}
   for(int i=0;i<slots;i++){float lane=Mathf.Lerp(-.78f,.78f,(float)i/(slots-1));var foot=VoyageModel.RampFoot(S.levels[2],lane*.86f);Vector2 at=World(foot.x,foot.y);bool busy=S.active.Any(p=>p.admitted&&p.berth==i);Oval(new Rect(at.x-3,at.y+5,6,3),busy?gold:teal);if(i>0&&i%3==0){float border=lane-.78f/(slots-1);var a=VoyageModel.RampFoot(S.levels[2],border*.86f);var b=VoyageModel.Door(S.levels[2],border*.86f);Stroke(World(a.x,a.y),World(b.x,b.y),new Color(.37f,.24f,.18f,.28f),1);}}
   if(S.shelterTime>0){Vector2 at=World(S.shieldX,S.shieldY);AreaRing(at,model.SkillRadius,new Color(.29f,.75f,.70f,.65f),4);Icon(at-new Vector2(0,70),5,new Color(.72f,.97f,.87f,.9f),.8f);}
   foreach(var p in S.active){if(p.age<0||p.admitted||p.heading||p.fieldVisited||p.roamBlend!=.5f)continue;Vector2 at=World(p.x,p.y);for(int k=0;k<2;k++){float t=Mathf.Repeat(S.elapsed*.6f+k*.5f+p.id*.17f,1);Oval(new Rect(at.x-5,at.y-20-t*25,4,4),new Color(.63f,.79f,.36f,(1-t)*.75f));}}
   if(model.BossWinding||S.attackPulse>0){Vector2 at=World(model.BossAttackPoint.x,model.BossAttackPoint.y);float radius=model.BossRadius*W/1.7778f;float charge=model.BossWinding?1-S.attackClock/1.5f:1;Oval(new Rect(at.x-radius,at.y-model.BossRadius*H,radius*2,model.BossRadius*H*2),new Color(.57f,.11f,.22f,.13f+charge*.18f));AreaRing(at,model.BossRadius,new Color(.85f,.25f,.23f,.9f),4);if(model.BossWinding)AreaRing(at,model.BossRadius*charge,new Color(.96f,.62f,.40f,.6f),2);else AreaRing(at,model.BossRadius*(1.65f-S.attackPulse),new Color(.47f,.10f,.26f,S.attackPulse),8);}
   if(S.bossStun>0){Vector2 at=World(S.demonX,S.demonY);for(int i=0;i<3;i++){float a=S.elapsed*3+i*2.094f;Oval(new Rect(at.x+Mathf.Cos(a)*39,at.y-176+Mathf.Sin(a)*9,9,9),paper);}}
   if(lossFlash>Time.unscaledTime){Vector2 at=World(model.BossAttackPoint.x,model.BossAttackPoint.y);Text(new Rect(at.x-110,at.y-70,220,30),"未収容の貢献を失った",16,rust,TextAnchor.MiddleCenter,true);}
  }
 }
}
