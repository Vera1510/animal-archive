using System;
using UnityEngine;
namespace AnimalArchive.Voyage {
 public partial class VoyageGame {
  Texture2D facilities,shipRest,guideAtlas,stormHarbor,shipExpanded;RenderTexture foreground;bool auditionFromTitle,worldHeld;Vector2 heldAt;float buildFlash;int buildKind;string audition="好きな音をタップして試聴できます";
  void InitV5(){harbor=Resources.Load<Texture2D>("Voyage/harbor-v6");stormHarbor=Resources.Load<Texture2D>("Voyage/storm-v6");ship=Resources.Load<Texture2D>("Voyage/ark-v6");shipRest=ship;shipExpanded=Resources.Load<Texture2D>("Voyage/ark-expanded-v6");guideAtlas=Resources.Load<Texture2D>("Voyage/human-v6");}
  void SetTheme(){sound.Theme(S.mode>=2?4:(model.CrisisActive||model.DemonActive)?2:S.crisis==2?3:S.total>=100?1:0);}
  void EventV5(string e){
   if(e.StartsWith("phase:"))SetTheme();
   if(e=="guide-empty")Tell("近くに動物がいません。群れのそばへ移動しよう。");if(e.StartsWith("guide-hit:"))Tell(e.Split(':')[1]+"匹が加速 · 合図は8秒以上続きます");
   if(e.StartsWith("upgrade:")){buildKind=int.Parse(e.Split(':')[1]);buildFlash=Time.unscaledTime+2;sound.Play(2);S.tutorial=Math.Max(2,S.tutorial);}
   if(e=="tap")sound.Play(0);
   if(e=="guide"||e=="guide:auto")sound.Play(9);
   if(e=="shelter"){sound.Play(8);Tell("光の輪の中を守ります · 加速効果はありません。");}
   if(e=="crisis"){panel="";selected=-1;S.readEvents|=8;Tell("風の輪へ移動して「誘導」。逃げ惑う群れを助けよう。");SetTheme();sound.Play(6);}
   if(e=="beacon")sound.Play(8);
   if(e=="crisis-clear"){S.readEvents|=16;Tell("渡れた。次は、もっと大きな群れを迎えよう。");SetTheme();sound.Play(12);}
   if(e=="protected")sound.Play(8);
   if(e=="afterwave"){Tell("返し波。守りの合図で落ち着かせよう。");sound.Play(13);}
  }
  Vector2 World(float x,float y)=>new Vector2(x*W,y*H);
  readonly System.Collections.Generic.Dictionary<int,Texture2D> ringTextures=new System.Collections.Generic.Dictionary<int,Texture2D>();
  void Ring(Vector2 p,float radius,Color c,float thick=2){
   if(Event.current.type!=EventType.Repaint||radius<=0||c.a<=0)return;
   int key=Mathf.Clamp(Mathf.RoundToInt(thick/Mathf.Max(1,radius)*128),1,30);
   if(!ringTextures.TryGetValue(key,out var texture)){texture=new Texture2D(256,128,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;var pixels=new Color32[256*128];float inner=1-key/128f;for(int y=0;y<128;y++)for(int x=0;x<256;x++){float nx=(x-127.5f)/126f,ny=(y-63.5f)/62f;float d=Mathf.Sqrt(nx*nx+ny*ny);float alpha=Mathf.Clamp01((1-d)*100)*Mathf.Clamp01((d-inner)*100);pixels[y*256+x]=new Color32(255,255,255,(byte)(alpha*255));}texture.SetPixels32(pixels);texture.Apply(false,true);ringTextures[key]=texture;}
   Color old=GUI.color;GUI.color=c;GUI.DrawTexture(new Rect(p.x-radius,p.y-radius*.49f,radius*2,radius*.98f),texture);GUI.color=old;
  }
  void Stroke(Vector2 a,Vector2 b,Color c,float thick){Matrix4x4 old=GUI.matrix;GUI.matrix=old*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);Fill(new Rect(0,0,(b-a).magnitude,thick),c);GUI.matrix=old;}
  void RampPoints(out Vector2 a,out Vector2 b,out Vector2 d){Vector2 va=VoyageModel.Door(S.levels[2],-1),vb=VoyageModel.Door(S.levels[2],1),vd=VoyageModel.RampFoot(S.levels[2],-1);a=World(va.x,va.y)+new Vector2(0,BoatBob());b=World(vb.x,vb.y)+new Vector2(0,BoatBob());d=World(vd.x,vd.y);}
  void Ramp(){if(screen!="title"&&S.mode>=2&&!S.freeplay)return;RampPoints(out var a,out var b,out var d);Matrix4x4 old=GUI.matrix,affine=Matrix4x4.identity;affine.SetColumn(0,new Vector4(b.x-a.x,b.y-a.y,0,0));affine.SetColumn(1,new Vector4(d.x-a.x,d.y-a.y,0,0));affine.SetColumn(3,new Vector4(a.x,a.y,0,1));GUI.matrix=old*affine;Fill(new Rect(0,0,1,1),new Color(.64f,.40f,.24f));for(int k=0;k<16;k++)Fill(new Rect(.018f,k/16f,.964f,.049f),new Color(.86f,.65f,.38f));GUI.matrix=old;
   Stroke(a+Vector2.down*11,d+Vector2.down*11,teal,4);for(int i=0;i<=4;i++){Vector2 p=Vector2.Lerp(a,d,i/4f);Stroke(p,p+Vector2.down*13,ink,3);}
  }
  void WorldBack(){Ramp();if(screen=="title")return;{Vector2 b=World(.15f,.55f);float h=42+S.levels[0]*5;Stroke(b+new Vector2(-18,0),b+new Vector2(-18,-h),ink,5);Stroke(b+new Vector2(18,0),b+new Vector2(18,-h),ink,5);Stroke(b+new Vector2(-22,-h),b+new Vector2(22,-h),ink,5);for(int i=0;i<Math.Min(3,1+S.levels[0]/2);i++){Vector2 at=b+new Vector2(-11+i*11,-h+10);Stroke(at+Vector2.down*9,at,ink,2);Oval(new Rect(at.x-5,at.y,10,11),gold);}}
   if(S.levels[1]>0){for(int i=0;i<Math.Min(4,S.levels[1]);i++){Vector2 p=World(.29f+i*.035f,.39f);Stroke(p,p+Vector2.down*34,ink,3);Fill(new Rect(p.x,p.y-34,19,13),i%2==0?rust:teal);}}
   if(model.CrisisActive||S.protection){for(int i=0;i<3;i++){Vector2 p=World(VoyageModel.Beacons[i].x,VoyageModel.Beacons[i].y);bool fixedUp=(S.anchors&(1<<i))!=0;Ring(p,34,fixedUp?new Color(.26f,.62f,.58f,.45f):new Color(.84f,.37f,.21f,.8f),2);if(!fixedUp){for(int j=0;j<3;j++)Stroke(p+new Vector2(-20+j*14,-6),p+new Vector2(-10+j*14,-18),paper,3);}else if(S.protection){Stroke(p,p+Vector2.down*28,teal,4);Oval(new Rect(p.x-6,p.y-34,12,12),gold);}}}
  }
  void DrawGate(){} // The boarding opening belongs to the ship; no disconnected arch.
  void DrawGuide(){if(screen=="title"||S.mode>=2)return;Vector2 p=World(S.guideX,S.guideY);bool moving=Mathf.Abs(S.targetX-S.guideX)+Mathf.Abs(S.targetY-S.guideY)>.008f;Oval(new Rect(p.x-20,p.y-5,40,10),new Color(.22f,.14f,.28f,.22f));int frame=S.tapPulse>0||S.guidePulse>0?3:moving?1+(int)(S.elapsed*6)%2:0;if(guideAtlas)GUI.DrawTextureWithTexCoords(new Rect(p.x-34,p.y-84,68,88),guideAtlas,new Rect(frame*.25f,0,.25f,1),true);if(moving)Ring(World(S.targetX,S.targetY),14,new Color(teal.r,teal.g,teal.b,.4f));
   if(S.guidePulse>0){float a=1-S.guidePulse/1.6f;Ring(p,35+a*model.GuideRadius*W,new Color(.9f,.63f,.24f,1-a),3);}if(S.tapPulse>0)Ring(World(heldAt.x,heldAt.y),15+(1-S.tapPulse/.35f)*50,new Color(.95f,.68f,.22f,S.tapPulse/.35f),3);
   if(S.shelterTime>0)Ring(p,160,new Color(.27f,.69f,.64f,.35f),4);
  }
  void WorldFront(){SkillArea();if(S.mode<2||S.freeplay||screen=="title"){RampPoints(out var a,out var b,out var d);Vector2 end=d+b-a;Stroke(b+Vector2.down*11,end+Vector2.down*11,teal,4);for(int i=0;i<=4;i++){Vector2 p=Vector2.Lerp(b,end,i/4f);Stroke(p,p+Vector2.down*13,ink,3);}}
   if(screen=="play"&&(worldHeld||Mathf.Abs(S.guideX-S.targetX)>.015f||Mathf.Abs(S.guideY-S.targetY)>.015f)){Vector2[] edge={World(.04f,.43f),World(.66f,.43f),World(.66f,.64f),World(.87f,.72f),World(.87f,.88f)};for(int i=1;i<edge.Length;i++)for(int j=0;j<12;j+=2)Stroke(Vector2.Lerp(edge[i-1],edge[i],j/12f),Vector2.Lerp(edge[i-1],edge[i],(j+1)/12f),new Color(.16f,.36f,.37f,.28f),2);Ring(World(S.targetX,S.targetY),17,teal,2);}
   if(S.tutorial==0&&screen=="play"&&order.Count>0){Vector2 p=positions[order[0].id];Ring(p,26+Mathf.Sin(Time.unscaledTime*3)*5,new Color(.34f,.19f,.36f,.6f),2);}
if(model.CrisisActive&&!reduceMotion){for(int i=0;i<24;i++){float x=Mathf.Repeat(i*.139f+Time.unscaledTime*.11f,1)*W,y=Mathf.Repeat(i*.177f+Time.unscaledTime*.06f,1)*H;Stroke(new Vector2(x,y),new Vector2(x-22,y-8),new Color(.87f,.88f,.93f,.25f),1);}}
   if(buildFlash>Time.unscaledTime){Vector2 p=buildKind==2?World(.69f,.61f):buildKind==3?World(S.guideX,S.guideY):World(.27f,.4f);float t=(buildFlash-Time.unscaledTime)/2;Ring(p,30+(1-t)*90,new Color(.94f,.59f,.17f,t),3);}
  }
  void BookPanel(){PanelBase("航海の手帳",310);string[] labels={"図鑑  "+model.Discovered+" / 12","航海記","遊び方","音と動き"};string[] targets={"codex","log","help","settings"};float cw=(W-126)/2;for(int i=0;i<4;i++){Rect r=new Rect(51+i%2*(cw+24),H-345+i/2*99,cw,82);if(Button(r,labels[i],true,false,25))Toggle(targets[i]);if(i==0&&model.NewCodex)Dot(r);}Text(new Rect(53,H-147,W-106,30),"累計 "+S.total.ToString("N0")+"  ·  "+TimeText(S.elapsed)+"  ·  "+saveStatus,19,muted);}
  void Settings(){PanelBase("音の試聴",470);float top=H-512,cw=(W-154)/5;string[] names={"登船","集結地の成長","珍しい動物","案内人の合図","聖光","ナギの声","トウマの声","逆潮・襲来","夜明け","出航"};int[] ids={1,2,3,9,8,5,7,6,12,4};for(int i=0;i<10;i++)if(Button(new Rect(51+i%5*(cw+13),top+i/5*76,cw,64),names[i],!sound.Muted,false,22)){sound.Play(ids[i]);audition=names[i];}string[] themes={"朝の集結地","にぎわう集結地","逆潮","嵐のあと","出航"};float mw=(W-142)/5;for(int i=0;i<5;i++)if(Button(new Rect(51+i*(mw+10),top+168,mw,61),themes[i],!sound.Muted,sound.CurrentTheme==i,20)){sound.Theme(i);audition=themes[i];}Text(new Rect(54,top+236,W-108,40),sound.Muted?"「音を鳴らす」で試聴できます":audition,22,teal,TextAnchor.MiddleCenter);if(Button(new Rect(51,top+290,225,61),sound.Muted?"音を鳴らす":"すべて消音",true,true,22))sound.Toggle();if(Button(new Rect(291,top+290,202,61),"BGM "+Mathf.RoundToInt(sound.MusicVolume*100)+"%",true,false,21))sound.Volume(sound.MusicVolume>.5f?.18f:sound.MusicVolume>.25f?.65f:.42f);if(Button(new Rect(508,top+290,245,61),reduceMotion?"ゆらぎを戻す":"ゆらぎを少なく",true,false,21)){reduceMotion=!reduceMotion;PlayerPrefs.SetInt("voyage.reduceMotion",reduceMotion?1:0);}if(Button(new Rect(W-310,top+290,257,61),"試聴を終える",true,true,22)){SetTheme();panel="";if(auditionFromTitle){screen="title";auditionFromTitle=false;}}}
 }
}
