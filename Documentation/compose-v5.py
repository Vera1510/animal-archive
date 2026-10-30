import numpy as np,wave,json
from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/Resources/AudioV5';root.mkdir(exist_ok=True)
sr=22050;rng=np.random.default_rng(5417)
def write(name,a):
 a=np.asarray(a);a=np.tanh(a);a=np.stack((a,np.roll(a,71)*.96),axis=1) if a.ndim==1 else a
 with wave.open(str(root/(name+'.wav')),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((np.clip(a,-.92,.92)*32767).astype('<i2').tobytes())
def tone(t,f,decay=5):return (np.sin(2*np.pi*f*t)+.23*np.sin(2*np.pi*f*2.01*t)+.09*np.sin(2*np.pi*f*3.97*t))*np.exp(-t*decay)*np.minimum(t/.008,1)
names=['touch','boarding','build','rare','sailing','nagi','undertow','touma','beacon','guide','step','automatic','dawn','afterwave']
lengths=[.12,.24,.85,1.3,3.6,.095,2.4,.13,1.2,.6,.14,.32,2.4,1.3]
for k,name in enumerate(names):
 for v in range(4):
  t=np.arange(int(sr*lengths[k]))/sr;n=len(t);a=np.zeros(n);shift=2**((v-1.5)*.45/12);noise=rng.uniform(-1,1,n)
  if k in [0,1,10,11]:
   f=[920,270,0,0,0,0,0,0,0,0,150,530][k];a=tone(t,f*shift,25 if k==10 else 18)*(.12 if k==10 else .24)+noise*np.exp(-t*65)*.025
   if k==1:a+=tone(np.maximum(0,t-.055),410*shift,26)*.11*(t>.055)
  elif k in [2,3,8,12]:
   notes={2:[52,59,64],3:[81,88,93],8:[69,76,81],12:[60,64,67,72,76,79]}[k]
   for j,m in enumerate(notes):
    u=t-j*.105;a+=(t>=j*.105)*tone(np.maximum(u,0),440*2**((m-69)/12)*shift,4.5)*(.13 if k==12 else .18)
  elif k==4:
   env=np.minimum(t/.7,1)*np.minimum((lengths[k]-t)/.8,1);a=(np.sin(2*np.pi*110*t)+.4*np.sin(2*np.pi*165*t)+.2*np.sin(2*np.pi*220*t))*env*.14
  elif k in [5,7]:
   f=(420 if k==5 else 145)*shift;env=np.sin(np.pi*t/lengths[k])**1.4
   a=(np.sin(2*np.pi*f*t+.2*np.sin(t*85))+.22*np.sin(2*np.pi*f*(3 if k==5 else 2)*t))*env*.22
  elif k==9:
   f=620*shift;env=np.sin(np.pi*t/lengths[k])**2;a=(np.sin(2*np.pi*(f*t+100*t*t))*.19+np.sin(2*np.pi*(f*1.5*t+85*t*t))*.07)*env
  elif k in [6,13]:
   smooth=np.convolve(noise,np.ones(35)/35,mode='same');env=np.sin(np.pi*t/lengths[k])**2;a=(smooth*.8+np.sin(t*2*np.pi*67)*.035)*env
  a[-min(100,n):]*=np.linspace(1,0,min(100,n));write(f'{name}-{v}',a)
# Low-level sea air, with a slow surf envelope; no harsh high-frequency noise.
t=np.arange(sr*20)/sr;noise=rng.normal(0,1,len(t));air=np.convolve(noise,np.ones(90)/90,mode='same')*(.09+.04*np.sin(2*np.pi*t/5))
write('harbor-air',air)
# Shared original motif, five clearly different instrument arrangements, 32-second loops.
beat=.5;duration=32;n=int(sr*duration)
chords=[[48,55,60,64],[45,52,57,60],[41,48,53,57],[43,50,55,59]]
melody=[72,76,79,76,74,72,67,71,69,72,76,72,67,65,64,67]
for mood in range(5):
 a=np.zeros(n)
 def note(start,dur,midi,gain,kind):
  first=int(start*sr);u=np.arange(int(dur*sr))/sr;f=440*2**((midi-69)/12)
  if kind=='pad':z=(np.sin(2*np.pi*f*u)+.14*np.sin(2*np.pi*2*f*u))*np.sin(np.pi*u/dur)**.8
  elif kind=='flute':z=(np.sin(2*np.pi*f*u+.018*np.sin(u*29))+.07*np.sin(2*np.pi*f*2*u))*np.minimum(u/.06,1)*np.minimum((dur-u)/.2,1)
  elif kind=='drum':z=np.sin(2*np.pi*(54*u+2*(1-np.exp(-u*35))))*np.exp(-u*18)
  else:z=tone(u,f,4.2)
  ids=(first+np.arange(len(u)))%n;np.add.at(a,ids,z*gain)
 for bar in range(16):
  start=bar*2;c=chords[(bar//2)%4]
  for m in c:note(start,2.3,m-(12 if mood==2 else 0),.022 if mood<2 else .035,'pad')
  count=2 if mood==0 else 4 if mood==1 else 2 if mood==2 else 8
  for j in range(count):note(start+j*2/count,.9,c[j%4]+12,.05 if mood!=2 else .024,'key')
  if mood!=2 and (mood>0 or bar%2==0):
   for j in range(2):note(start+j,.83,melody[(bar*2+j)%16]+(12 if mood==4 else 0),.038,'flute' if mood<4 else 'key')
  if mood>=2:
   for j in range(4 if mood==3 else 2):note(start+j*(.5 if mood==3 else 1),.35,36,.045 if mood==2 else .072,'drum')
  if mood==2:note(start+.75,1.1,c[0]+1,.017,'pad')
 a+=np.roll(a,int(.1875*sr))*.16;write(f'theme-{mood}',a)
(Path(__file__).resolve().parent/'V5-audio-score.json').write_text(json.dumps({'source':'Original deterministic synthesis, no third-party recordings','seed':5417,'sampleRate':sr,'stereo':True,'motifMIDI':melody,'chordsMIDI':chords,'themes':['Quiet harbor','Working harbor','Undertow','Dawn engine','Sailing'],'effects':names,'variationsEach':4},indent=2),encoding='utf-8')
