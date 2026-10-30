using UnityEngine;
namespace AnimalArchive {
 public class ArkSound:MonoBehaviour {
  AudioSource[] voices=new AudioSource[6],music=new AudioSource[2];AudioSource air,weather;float weatherTarget;
  AudioClip[,] tones=new AudioClip[14,4];int[] last=new int[14];float[] next=new float[14];int voice,active,mood=-1;float mix=1,stepClock;public bool Muted;public bool Suspended,Background;public float MusicVolume=.42f;public int CurrentTheme=>mood;
  static readonly string[] Names={"touch","boarding","build","rare","sailing","nagi","undertow","touma","beacon","guide","step","automatic","dawn","afterwave"};
  void Awake(){Muted=PlayerPrefs.GetInt("archive.audio.muted",0)==1;MusicVolume=PlayerPrefs.GetFloat("archive.music.volume",.42f);for(int i=0;i<6;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].volume=.5f;}for(int i=0;i<2;i++){music[i]=gameObject.AddComponent<AudioSource>();music[i].loop=true;music[i].volume=0;}for(int k=0;k<14;k++)for(int v=0;v<4;v++)tones[k,v]=Resources.Load<AudioClip>("AudioV5/"+Names[k]+"-"+v);air=gameObject.AddComponent<AudioSource>();air.clip=Resources.Load<AudioClip>("AudioV5/harbor-air");air.loop=true;air.volume=.35f;air.Play();weather=gameObject.AddComponent<AudioSource>();weather.clip=Resources.Load<AudioClip>("AudioV5/storm-air-v6");weather.loop=true;weather.volume=0;weather.Play();}
  public void Theme(int n){if(mood==n)return;mood=n;active=1-active;music[active].clip=Resources.Load<AudioClip>("AudioV5/theme-"+n);music[active].Play();mix=0;}
  public void Play(int k){if(Muted||Background||k<0||k>=14||Time.unscaledTime<next[k])return;next[k]=Time.unscaledTime+(k==0?.25f:k==1?.65f:k==10?.35f:k==5||k==7?.07f:.15f);int v=(last[k]+Random.Range(1,4))%4;last[k]=v;var source=voices[voice++%6];source.Stop();source.clip=tones[k,v];source.panStereo=k==10?Random.Range(-.35f,.35f):0;source.volume=k==0?.22f:k==5||k==7?.24f:k==10?.16f:k==4?.8f:.48f;source.Play();}
  public void Activity(int count,bool crisis){if(Suspended||count==0)return;stepClock-=Time.unscaledDeltaTime;if(stepClock<=0){stepClock=Mathf.Lerp(2.2f,.4f,Mathf.Clamp01(count/45f));Play(10);}air.pitch=crisis?.86f:1;weatherTarget=crisis?.65f:0;}
  public void Toggle(){Muted=!Muted;PlayerPrefs.SetInt("archive.audio.muted",Muted?1:0);PlayerPrefs.Save();if(Muted)foreach(var s in voices)s.Stop();}
  public void Volume(float v){MusicVolume=Mathf.Clamp01(v);PlayerPrefs.SetFloat("archive.music.volume",MusicVolume);PlayerPrefs.Save();}
  void Update(){mix=Mathf.Min(1,mix+Time.unscaledDeltaTime*.4f);music[active].volume=Muted||Background?0:MusicVolume*mix;music[1-active].volume=Muted||Background?0:MusicVolume*(1-mix);air.volume=Muted||Suspended?0:.25f;weather.volume=Muted||Suspended?0:Mathf.MoveTowards(weather.volume,weatherTarget,Time.unscaledDeltaTime*.2f);if(mix==1&&music[1-active].isPlaying)music[1-active].Stop();}
 }
}
