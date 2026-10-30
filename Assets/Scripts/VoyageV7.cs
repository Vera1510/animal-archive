using System;
using System.Collections.Generic;
using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class VoyageGame {
  Texture2D owlFlight;float drawer,measureStart=-1;readonly Queue<Vector3> measured=new Queue<Vector3>();float observedAnimals,observedPoints;float narrativeTime;string narrativeSpeaker="",narrativeText="";readonly Queue<string[]> narration=new Queue<string[]>();
  float BoatBob()=>reduceMotion?0:Mathf.Sin(Time.unscaledTime*(model.CrisisActive?2:.6f))*(model.CrisisActive?3:.7f);
  void ResetV7(){measured.Clear();measureStart=-1;observedAnimals=observedPoints=0;narration.Clear();narrativeTime=0;}
  void RecordArrival(int value){measured.Enqueue(new Vector3(S.elapsed,1,value));}
  void UpdateV7(){if(model==null)return;if(!owlFlight)owlFlight=Resources.Load<Texture2D>("Voyage/owl-flight-v7");drawer=Mathf.MoveTowards(drawer,panel=="growth"?1:0,Time.unscaledDeltaTime*6);
   if(screen!="play"||background||paused||panel=="settings"||Screen.width<Screen.height)return;if(measureStart<0)measureStart=S.elapsed;while(measured.Count>0&&S.elapsed-measured.Peek().x>10)measured.Dequeue();float n=0,v=0;foreach(var x in measured){n+=x.y;v+=x.z;}float window=Mathf.Min(10,S.elapsed-measureStart);observedAnimals=window>=1?n*60/window:0;observedPoints=window>=1?v*60/window:0;
   int page=(S.narrated&1)==0?0:model.Known(4)&&(S.narrated&2)==0?1:S.crisis==1&&(S.narrated&8)==0?3:S.workers.Count>1&&(S.narrated&4)==0?2:S.protection&&(S.narrated&16)==0?4:model.CanSail&&(S.narrated&32)==0?5:-1;
   if(page>=0){if(page>=3){narration.Clear();narrativeTime=0;}S.narrated|=1<<page;S.readEvents|=1<<page;foreach(var line in stories.pages[page].lines)narration.Enqueue(new[]{line.speaker,line.text});}
   if(panel!="")return; narrativeTime-=Time.unscaledDeltaTime;if(narrativeTime<=0&&narration.Count>0){var l=narration.Dequeue();narrativeSpeaker=l[0];narrativeText=l[1];narrativeTime=7;sound.Play(narrativeSpeaker=="ナギ"?5:7);}
  }
  void FlyingOwl(int frame,Vector2 p,float height,float alpha){ // UV bounds follow generated silhouettes, retaining each wing spread.
   Rect[] uv={new Rect(.016f,.19f,.199f,.70f),new Rect(.227f,.19f,.298f,.49f),new Rect(.535f,.15f,.216f,.52f),new Rect(.767f,.19f,.215f,.58f)};Rect r=uv[frame%4];float h=height*(frame%4==0?1.35f:frame%4==1?.94f:frame%4==2?1:1.112f);float w=h*(owlFlight.width*r.width)/(owlFlight.height*r.height);Color old=GUI.color;GUI.color=new Color(1,1,1,alpha);GUI.DrawTextureWithTexCoords(new Rect(p.x-w*.5f,p.y-h,w,h),owlFlight,r,true);GUI.color=old;
  }
  string Goal()=>S.mode==1?"最後の仲間が乗船中":S.mode==2?"全員と、次の岸へ":S.freeplay?"自由収容":model.DemonActive?"長押しで祈り · 仲間を守ろう":S.demon==1?"黒い影が接近 · 防衛の準備":model.CanSail?"準備完了 · 出航を選ぼう":model.CrisisActive?"風の輪で群れを助ける":"貢献 "+S.total.ToString("N0")+" / "+cfg.finalTotal.ToString("N0");
  string Lesson()=>S.tutorial==0?"① 陸地をタップして移動 · 長押しで近くの動物を招呼":S.lesson==0?"② 群れの近くで「誘導」 · 一帯が速く進みます":S.tutorial==1?"③「成長」を開き、続けて購入できます":model.CrisisActive&&S.lesson<2?"風の輪で誘導 · 怖がる群れには「守護」":"";
  int NearbyCount(){int n=0;foreach(var p in S.active){Vector2 d=VoyageModel.Position(p)-new Vector2(S.guideX,S.guideY);d.x*=1.7778f;if(p.age>=0&&d.magnitude<model.GuideRadius)n++;}return n;}
  void SkillArea(){if(screen!="play"||S.mode!=0||(!worldHeld&&S.guidePulse<=0))return;Vector2 center=World(S.guideX,S.guideY);float r=model.GuideRadius;for(int k=0;k<48;k++){float a=k*Mathf.PI*2/48,b=(k+1)*Mathf.PI*2/48;Stroke(center+new Vector2(Mathf.Cos(a)*r/1.7778f*W,Mathf.Sin(a)*r*H),center+new Vector2(Mathf.Cos(b)*r/1.7778f*W,Mathf.Sin(b)*r*H),new Color(teal.r,teal.g,teal.b,.22f),2);}}
  float Predicted(int upgrade=-1){int f=S.levels[0]+(upgrade==0?1:0),b=S.levels[1]+(upgrade==1?1:0),d=S.levels[2]+(upgrade==2?1:0);float mean=0,weights=0;for(int i=0;i<cfg.animals.Length;i++){var animal=cfg.animals[i];if(!model.Eligible(i))continue;float w=model.CropWeight(i)*animal.weight*(animal.rarity==0?1:animal.rarity==1?1+cfg.uncommonStep*(b+model.Tier):1+cfg.rareStep*(b+model.Tier));mean+=w*animal.value;weights+=w;}mean/=Mathf.Max(.001f,weights);return Mathf.Min((1+b+(S.crisis==2?2:0)+(S.demon==3?1:0))*60/(cfg.baseInterval*Mathf.Pow(cfg.frequencyFactor,f)),model.DockRate(d))*mean*model.ValueScale(d)*(S.crisis==2?8:1)*(S.demon==3?1.4f:1);}
  void GrowthPanel(){float x=20-404*(1-drawer),y=91,cw=400;Round(new Rect(x,y,cw,H-178),paper,10);Text(new Rect(x+18,y+9,320,32),"集結地を育てる",24,ink,TextAnchor.MiddleLeft,true);if(Button(new Rect(x+cw-52,y+6,39,37),"×",true,false,26))panel="";
   Text(new Rect(x+18,y+46,365,30),"平常時の予測  "+Predicted().ToString("N0")+" pt/分",18,teal);float rh=(H-292)/4;
   for(int i=0;i<4;i++){float yy=y+83+i*rh;Text(new Rect(x+18,yy,275,25),cfg.upgrades[i].name+"  Lv."+S.levels[i],20,ink,TextAnchor.MiddleLeft,true);long cost=model.Cost(i);string effect=i==0?"到着間隔 "+model.Interval.ToString("F1")+"秒 → "+(model.Interval*cfg.frequencyFactor).ToString("F1")+"秒":i==1?"一度に "+model.Batch+"匹 → "+(model.Batch+1)+"匹":i==2?"同時登船 "+model.DockCapacity+"匹 → "+(model.DockCapacity+3)+"匹 · 吸引UP":"群れの歩み +8% · 範囲UP · 再使用短縮";
    Text(new Rect(x+18,yy+27,270,23),cost<0?"成長は最大です":effect,15,teal);string gain=i==3?(S.levels[3]==0?"守護：初回開放":"加速の再使用 "+model.SkillCooldown+"秒 → "+Mathf.Max(5,model.SkillCooldown-1)+"秒"):("予測 "+Predicted().ToString("N0")+" → "+Predicted(i).ToString("N0")+" pt/分");Text(new Rect(x+18,yy+51,270,23),cost<0?"":gain,14,muted);
    if(Button(new Rect(x+298,yy+14,86,49),cost<0?"最大":cost.ToString("N0"),cost>=0&&model.Unlocked(i)&&S.available>=cost,true,19)){model.Buy(i);ProcessEvents();Save();}
    Fill(new Rect(x+18,yy+rh-4,cw-36,1),new Color(.2f,.25f,.25f,.10f));
   }Text(new Rect(x+18,H-119,365,25),"予測は入口の上限を反映。誘導・嵐で実測は変動",13,muted);
  }
  void HelpPanel(){PanelBase("遊び方と出航の目標",470);string[] steps={"人をタップで選択、地面をタップで案内。長押しで連続誘導。","「誘導」：周囲が2.2倍速。群れの近くで使おう。","「守護」：選んだ人の周囲を攻撃・強風から守る。加速はしません。","従業員は自動で案内・休憩・復帰。ハルは畑を育て、好みの動物を誘います。","出航条件：累計"+cfg.finalTotal.ToString("N0")+"ptに到達し、悪魔を退ける。","嵐を越え、防護を作ると新しい動物が来ます。全員を待って出航。"};for(int i=0;i<steps.Length;i++)Text(new Rect(55,H-506+i*47,W-110,42),steps[i],21,ink);if(Button(new Rect(55,H-203,270,55),"操作ヒントをもう一度",true,true,20)){S.tutorial=0;S.lesson=0;panel="";}Text(new Rect(346,H-201,W-400,63),"進捗："+S.boarded+"匹 · "+S.total.ToString("N0")+"pt · 防護"+(S.protection?"完成":"未完成"),20,teal);}
  void NarrativeCard(){if(narrativeTime<=0||screen!="play"||panel!=""||paused||story>=0||S.mode!=0)return;float x=Mathf.Max(510,W*.41f);float width=W-x-28;Round(new Rect(x,126,width,104),new Color(paper.r,paper.g,paper.b,.95f),8);Text(new Rect(x+17,130,width-61,25),narrativeSpeaker,17,teal,TextAnchor.MiddleLeft,true);Text(new Rect(x+17,159,width-34,63),narrativeText,19,ink);if(Button(new Rect(x+width-39,131,30,26),"×",true,false,18)){narrativeTime=0;narration.Clear();}}
 }
}
