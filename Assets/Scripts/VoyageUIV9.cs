using System;
using System.Linq;
using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class VoyageGame {
  Texture2D title9;Vector2 codexDown;bool codexDragging;float codexWheel;
  void TitleBackdrop(){if(Event.current.type!=EventType.Repaint)return;if(!title9)title9=Resources.Load<Texture2D>("Voyage/title-v9");if(title9)GUI.DrawTexture(new Rect(0,0,W,H),title9,ScaleMode.ScaleAndCrop);}
  void Title(){inputBlocked=restart;Text(new Rect(W*.05f,H*.18f,W*.43f,42),"動物たちの方舟",26,teal);Text(new Rect(W*.048f,H*.25f,W*.47f,65),"ANIMAL ARCHIVE",40,ink,TextAnchor.MiddleLeft,true);if(Button(new Rect(W*.05f,H*.40f,228,58),hasSave?"航海を続ける":"はじめる",true,true,23))Begin();if(hasSave&&Button(new Rect(W*.05f,H*.50f,228,45),"新しい航海",true,false,18))restart=true;if(Button(new Rect(24,H-65,145,42),"音を確かめる",true,false,17)){screen="play";paused=false;panel="settings";auditionFromTitle=true;}}
  // Small vector icons match the game's contour language and remain legible without emoji fonts.
  void Icon(Vector2 c,int kind,Color col,float s=1){
   if(kind==0){for(int i=0;i<3;i++)Fill(new Rect(c.x-13*s+i*10*s,c.y+(4-i*5)*s,6*s,(8+i*5)*s),col);Stroke(c+new Vector2(-14,-7)*s,c+new Vector2(12,-17)*s,col,2);}
   else if(kind==1){Oval(new Rect(c.x-5*s,c.y-15*s,10*s,10*s),col);Stroke(c+new Vector2(0,-3)*s,c+new Vector2(0,13)*s,col,3);Stroke(c+new Vector2(-11,5)*s,c+new Vector2(11,5)*s,col,3);}
   else if(kind==2){Stroke(c+new Vector2(0,15)*s,c+new Vector2(0,-8)*s,col,3);Oval(new Rect(c.x-13*s,c.y-12*s,13*s,8*s),col);Oval(new Rect(c.x,c.y-17*s,13*s,8*s),col);}
   else if(kind==3){Stroke(c+new Vector2(-14,-12)*s,c+new Vector2(-14,12)*s,col,2);Stroke(c+new Vector2(14,-12)*s,c+new Vector2(14,12)*s,col,2);Stroke(c+new Vector2(-14,12)*s,c+new Vector2(0,16)*s,col,2);Stroke(c+new Vector2(0,16)*s,c+new Vector2(14,12)*s,col,2);Stroke(c+new Vector2(0,-10)*s,c+new Vector2(0,16)*s,col,2);Stroke(c+new Vector2(-14,-12)*s,c+new Vector2(0,-8)*s,col,2);Stroke(c+new Vector2(0,-8)*s,c+new Vector2(14,-12)*s,col,2);}
   else if(kind==4){for(int i=0;i<3;i++){Vector2 p=c+new Vector2(-11+i*9,-10)*s;Stroke(p,p+new Vector2(8,10)*s,col,3);Stroke(p+new Vector2(8,10)*s,p+new Vector2(0,20)*s,col,3);}}
   else if(kind==5){Vector2[] p={new Vector2(-13,-12),new Vector2(0,-17),new Vector2(13,-12),new Vector2(10,7),new Vector2(0,16),new Vector2(-10,7),new Vector2(-13,-12)};for(int i=1;i<p.Length;i++)Stroke(c+p[i-1]*s,c+p[i]*s,col,3);}
   else {Stroke(c+Vector2.up*18*s,c+Vector2.down*18*s,col,4);Stroke(c+Vector2.left*12*s,c+Vector2.right*12*s,col,3);for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f+Mathf.PI*.25f;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Stroke(c+d*18*s,c+d*24*s,col,2);}}
  }
  bool IconButton(Rect r,int icon,string label,bool enabled,bool accent=false,float cooldown=0){bool hit=Button(r,"",enabled,accent);Color col=enabled?(accent?paper:teal):muted;Icon(new Vector2(r.center.x,r.y+24),icon,col,.85f);Text(new Rect(r.x,r.y+44,r.width,21),cooldown>0?Mathf.CeilToInt(cooldown)+"s":label,13,col,TextAnchor.MiddleCenter);return hit;}
  void Hud(){
   Round(new Rect(20,18,229,58),new Color(paper.r,paper.g,paper.b,.87f),6);Text(new Rect(33,21,201,27),S.available.ToString("N0")+" pt",20,ink,TextAnchor.MiddleLeft,true);Text(new Rect(33,49,200,18),observedPoints.ToString("N0")+" pt/分",12,teal);
   Text(new Rect(W-392,19,310,25),S.freeplay?"自由収容":S.mode==1?"最後の仲間を誘導中":model.CanSail?"出航できます":S.total.ToString("N0")+" / "+cfg.finalTotal.ToString("N0")+" pt",18,ink,TextAnchor.MiddleRight,true);
   Fill(new Rect(W-282,53,200,3),new Color(teal.r,teal.g,teal.b,.18f));Fill(new Rect(W-282,53,200*Mathf.Clamp01((float)S.total/cfg.finalTotal),3),teal);
   if(Button(new Rect(W-62,17,42,42),"Ⅱ",true,false,20)){paused=true;worldHeld=false;Save();}if(S.mode!=0)return;
   string[] nav={"成長","仲間","畑","手帳"},dest={"growth","staff","land","book"};bool[] dots={model.GrowthDot,model.StaffDot,model.FacilityDot,model.NewCodex};
   for(int i=0;i<4;i++){Rect r=new Rect(20+i*76,H-89,66,68);if(IconButton(r,i,nav[i],true,panel==dest[i]))Toggle(dest[i]);if(dots[i])Dot(r);}
   if(panel==""){
    Text(new Rect(24,81,220,22),"自動 "+S.workers.Count(w=>w.rest<=0)+"人 · 休憩 "+S.workers.Count(w=>w.rest>0)+"人",12,teal);
    if(Button(new Rect(20,114,34,34),"−",true,false,20))SetZoom(zoom-.15f);if(Button(new Rect(60,114,34,34),"+",true,false,20))SetZoom(zoom+.15f);
    if(S.area>0&&Button(new Rect(20,155,74,32),camX<.2f?"方舟 →":"← 畑",true,false,14)){camX=camX<.2f?.5f:S.area==2?-.44f:-.08f;zoom=1;ClampCamera();}
   }
   if(panel!=""&&panel!="growth"&&panel!="staff"&&panel!="land")return;
   if(selectedWorker>=0&&selectedWorker<S.workers.Count){
    Text(new Rect(W-192,H-120,172,23),VoyageModel.WorkerNames[selectedWorker]+" · "+model.WorkerStatus(selectedWorker),13,teal,TextAnchor.MiddleRight);
    if(selectedWorker==2){if(IconButton(new Rect(W-94,H-89,70,68),2,"手入れ",true,true)){model.TendGarden();ProcessEvents();Save();}}
    else {
     if(IconButton(new Rect(W-174,H-89,70,68),4,"誘導",S.guideCooldown<=0,true,S.guideCooldown)){model.CallGuide(false,selectedWorker);ProcessEvents();Save();}
     if(IconButton(new Rect(W-94,H-89,70,68),5,"守護",S.levels[3]>0&&S.shelterCooldown<=0,true,S.shelterCooldown)){model.Shelter(selectedWorker);ProcessEvents();Save();}
    }
   }
   if(model.DemonActive||S.demon==1){
    if(model.DemonActive){Rect pr=new Rect(W-260,H-89,74,68);Round(pr,teal,7);Icon(new Vector2(pr.center.x,pr.y+23),6,paper,.8f);Text(new Rect(pr.x,pr.y+45,pr.width,20),S.prayerCooldown>0?Mathf.CeilToInt(S.prayerCooldown)+"s":"祈る",13,paper,TextAnchor.MiddleCenter);Fill(new Rect(pr.x,pr.yMax-3,pr.width*Mathf.Clamp01(S.prayer/.8f),3),gold);if((Event.current.type==EventType.MouseDown||Event.current.type==EventType.MouseDrag)&&pr.Contains(mouse)){praying=true;worldHeld=false;Event.current.Use();}}
    Round(new Rect(W*.5f-166,16,332,63),ink,5);Text(new Rect(W*.5f-155,19,310,27),S.demon==1?"黒い影が接近 · 守護を準備":S.bossStun>0?"聖光でひるんでいる！":model.BossWinding?"範囲攻撃 · 祈りで止めよう":"悪魔襲来 · 祈るを長押し",16,paper,TextAnchor.MiddleCenter,true);Fill(new Rect(W*.5f-145,57,290*Mathf.Clamp01(S.demonHp/180),5),rust);Text(new Rect(W*.5f-145,65,290,12),"BOSS",9,paper,TextAnchor.MiddleCenter);
   }else if(model.CanSail&&Button(new Rect(W-354,H-76,167,48),"出航",true,true,21)){model.RequestSail();ProcessEvents();Save();}
   RegionalHint();
   string tip=S.tutorial<2?(selectedWorker<0?"人をタップで選択 → 地面をタップで案内":"地面をタップで案内 · 長押しで呼びかけ"):S.lesson==0?"群れの近くで「誘導」。長押しの案内も効果的":S.area>0&&S.plots[0].crop<0?"畑に種をまくと、好みの動物が集まります":"";
   if(tip!=""&&panel==""){Round(new Rect(W*.5f-240,H-135,480,31),new Color(paper.r,paper.g,paper.b,.85f),5);Text(new Rect(W*.5f-230,H-135,460,31),tip,14,teal,TextAnchor.MiddleCenter);}
  }
  void WorldInputV8(){
   if(inputBlocked||S.awayAward>0)return;bool open=panel==""||((panel=="growth"||panel=="staff"||panel=="land")&&mouse.x>430);Event e=Event.current;
   if(e.type==EventType.MouseUp&&(dragging||pointerDown)&&(!open||mouse.y<105||mouse.y>H-94)){pointerDown=dragging=worldHeld=false;return;}
   if(!open||mouse.y<105||mouse.y>H-94||mouse.x<100&&mouse.y<190)return;
   if(panel==""&&S.lesson==0&&new Rect(W*.5f-240,H-135,480,31).Contains(mouse))return;
   if(narrativeTime>0&&panel==""&&new Rect(Mathf.Max(510,W*.41f),126,W-Mathf.Max(510,W*.41f)-28,104).Contains(mouse))return;
   if(e.type==EventType.ScrollWheel){SetZoom(zoom-e.delta.y*.04f);e.Use();return;}
   if(e.type==EventType.MouseDown){
    var world=ScreenWorld(mouse);for(int i=S.workers.Count-1;i>=0;i--){var w=S.workers[i];Vector2 p=i==0?new Vector2(S.guideX,S.guideY):new Vector2(w.x,w.y);if(new Rect(p.x-34/W,p.y-94/H,68/W,106/H).Contains(world)){selectedWorker=i;worldHeld=pointerDown=false;sound.Play(i==1?7:5);model.CommandWorker(i,p.x,p.y);e.Use();return;}}
    for(int i=0;i<S.area;i++)if(Vector2.Distance(world,model.PlotPoint(i))<.075f){panel="land";worldHeld=false;e.Use();return;}
    pointerDown=true;dragging=false;pointerStart=priorPointer=mouse;heldAt=world;pointerTime=Time.unscaledTime;worldHeld=false;e.Use();
   }else if(e.type==EventType.MouseDrag&&pointerDown){if(Vector2.Distance(mouse,pointerStart)>12)dragging=true;if(dragging){camX-=(mouse.x-priorPointer.x)/(W*zoom);camY-=(mouse.y-priorPointer.y)/(H*zoom);ClampCamera();worldHeld=false;}priorPointer=mouse;e.Use();}
   else if(e.type==EventType.MouseUp&&pointerDown){if(!dragging){var p=ScreenWorld(mouse);if(selectedWorker>=0){model.CommandWorker(selectedWorker,p.x,p.y);ProcessEvents();}else Tell("まず案内人をタップして選ぼう");}pointerDown=worldHeld=false;e.Use();}
   else if(pointerDown&&!dragging&&Time.unscaledTime-pointerTime>.18f){heldAt=ScreenWorld(pointerStart);worldHeld=true;}
  }
  void DrawWorker(Worker w){
   Vector2 p=World(w.role==0?S.guideX:w.x,w.role==0?S.guideY:w.y);
   if(w.role==0)DrawGuide();else{float bob=reduceMotion||w.rest>0?0:Mathf.Sin(S.elapsed*7+w.role)*1.5f;Oval(new Rect(p.x-18,p.y-4,36,9),new Color(.2f,.1f,.2f,.22f));Sprite8(w.role==2?1:0,new Rect(p.x-45,p.y-94+bob,90,94));}
   if(selectedWorker==w.role){Ring(p,23,teal,2);Text(new Rect(p.x-40,p.y-111,80,19),VoyageModel.WorkerNames[w.role],13,teal,TextAnchor.MiddleCenter,true);}
   if(w.rest>0){Round(new Rect(p.x-36,p.y-132,72,30),paper,4);Text(new Rect(p.x-30,p.y-132,60,23),"休憩 "+Mathf.CeilToInt(w.rest)+"s",12,teal,TextAnchor.MiddleCenter);Fill(new Rect(p.x-30,p.y-106,60*(1-w.rest/6),3),gold);}
   if(w.pulse>0)Ring(p,24+(1-w.pulse)*60,new Color(.98f,.8f,.4f,w.pulse*.75f),2);
   for(int k=0;k<w.level;k++)Oval(new Rect(p.x+17+k*7,p.y-73,5,5),gold);
  }
  void CodexGesture(){if(selected>=0||inputBlocked)return;Event e=Event.current;if(e.type==EventType.ScrollWheel){codexWheel+=e.delta.y;if(Mathf.Abs(codexWheel)>1){codexPage=codexWheel>0?1:0;codexWheel=0;}e.Use();}else if(e.type==EventType.MouseDown){codexDown=mouse;codexDragging=true;}else if(e.type==EventType.MouseUp&&codexDragging){codexDragging=false;Vector2 d=mouse-codexDown;if(d.magnitude>35){codexPage=(Mathf.Abs(d.x)>Mathf.Abs(d.y)?d.x:d.y)<0?1:0;e.Use();}}}
  void FlowFeedback(){
   Vector2 foot=VoyageModel.RampFoot(S.levels[2],0);float radius=model.AttractionRadius;
   for(int i=0;i<7;i++){float t=Mathf.Repeat(S.elapsed*.30f+i/7f,1),a=1.1f+i*.28f;Vector2 outer=foot+new Vector2(-Mathf.Sin(a)*radius/1.7778f,Mathf.Cos(a)*radius);Vector2 at=Vector2.Lerp(outer,foot,t);Vector2 dir=(World(foot.x,foot.y)-World(outer.x,outer.y)).normalized;Stroke(World(at.x,at.y),World(at.x,at.y)+dir*8,new Color(.98f,.86f,.51f,Mathf.Sin(t*Mathf.PI)*.65f),2);}
   foreach(var p in S.active){if(p.admitted||p.calm<=0||p.age<0)continue;var at=VoyageModel.Position(p);Vector2 v=World(at.x,at.y);Vector2 d=(World(foot.x,foot.y)-v).normalized;Stroke(v-d*15,v-d*7,new Color(teal.r,teal.g,teal.b,.40f),2);}
   if(S.demon==1||model.DemonActive){Vector2 at=World(model.DemonPoint.x,model.DemonPoint.y);float pulse=.15f+Mathf.Sin(S.elapsed*3)*.04f;Fill(new Rect(-W,0,3*W,5),new Color(.42f,.10f,.24f,pulse*3));if(S.demonHit>0){Ring(at-new Vector2(0,72),30+(1-S.demonHit/.55f)*70,new Color(1,.94f,.65f,S.demonHit*1.8f),5);Text(new Rect(at.x-55,at.y-160-S.demonHit*35,110,35),"−"+(28+S.levels[3]),26,paper,TextAnchor.MiddleCenter,true);}}
  }
 }
}
