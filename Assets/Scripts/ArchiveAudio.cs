using System;
using UnityEngine;
namespace AnimalArchive {
 public class ArchiveAudio:MonoBehaviour {
  AudioSource source; AudioClip[,] clips=new AudioClip[7,4];int[] last=new int[7];public bool Muted;
  void Awake(){source=gameObject.AddComponent<AudioSource>();source.volume=.22f;Muted=PlayerPrefs.GetInt("archive.audio.muted",0)==1;
   for(int t=0;t<7;t++)for(int v=0;v<4;v++){int rate=22050;float duration=t==4?.8f:t==5?.09f:.18f;var samples=new float[(int)(rate*duration)];float freq=new[]{460,280,660,880,440,220,130}[t]*(1+v*.075f);for(int i=0;i<samples.Length;i++){float time=i/(float)rate,env=Mathf.Exp(-time*(t==4?5:30))*Mathf.Min(1,time*250);float f=t==4?freq*Mathf.Pow(2,Mathf.Floor(time/.13f)/12f*3):freq; samples[i]=env*(Mathf.Sin(2*Mathf.PI*f*time)*.6f+Mathf.Sin(2*Mathf.PI*f*2.01f*time)*.2f); }clips[t,v]=AudioClip.Create("Archive original "+t+" variant "+v,samples.Length,1,rate,false);clips[t,v].SetData(samples,0);}
  }
  public void Play(int type){if(Muted)return;int v=(last[type]+UnityEngine.Random.Range(1,4))%4;last[type]=v;source.PlayOneShot(clips[type,v],type==5?.3f:1);}
  public void Toggle(){Muted=!Muted;PlayerPrefs.SetInt("archive.audio.muted",Muted?1:0);PlayerPrefs.Save();if(!Muted)Play(0);}
 }
}
