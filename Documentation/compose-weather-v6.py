from pathlib import Path
import numpy as np,wave,json
root=Path(__file__).resolve().parents[1]
sr=22050;n=sr*12;t=np.arange(n)/sr;rng=np.random.default_rng(6027)
noise=rng.normal(0,1,n);low=np.convolve(noise,np.ones(85)/85,mode='same');wash=np.convolve(noise,np.ones(8)/8,mode='same')
s=(.20*low+.018*wash)*(0.45+0.55*np.sin(np.pi*t/6)**2)+.018*np.sin(2*np.pi*49*t)*np.sin(np.pi*t/6)**4
fade=sr//2;s[:fade]*=np.linspace(0,1,fade);s[-fade:]*=np.linspace(1,0,fade)
p=root/'Assets/Resources/AudioV5/storm-air-v6.wav'
with wave.open(str(p),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr);w.writeframes((s*32767).astype('<i2').tobytes())
(root/'Documentation/storm-audio-check.json').write_text(json.dumps(dict(seconds=12,sampleRate=sr,peak=float(np.max(np.abs(s))),rms=float(np.sqrt(np.mean(s*s))),source='Original deterministic synthesis, seed6027')),encoding='utf8')
