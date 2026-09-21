"""Rebuild the encounter WAVs. Needs numpy, imageio-ffmpeg, and the source MP3."""
from pathlib import Path
import subprocess,wave
import numpy as np

root=Path(__file__).resolve().parents[2]
import imageio_ffmpeg
ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
assets=root/'Assets/NantesUI'
audio=assets/'Audio'; audio.mkdir(exist_ok=True)
credit=assets/'ThirdParty/DragonStudio';credit.mkdir(exist_ok=True)
source=credit/'Simple-Whoosh-02.mp3'
rate=48000
raw=subprocess.check_output([ffmpeg,'-v','error','-i',str(source),'-f','f32le','-ac','2','-ar',str(rate),'-'])
whoosh=np.frombuffer(raw,dtype='<f4').reshape(-1,2).copy()
energy=np.array([np.mean(whoosh[i:i+960]**2) for i in range(0,len(whoosh),960)])
peak=(np.argmax(energy)+.5)*.020
print('Original whoosh peak:',peak,'duration:',len(whoosh)/rate)

def save(name,data):
    if data.ndim==1:data=np.column_stack((data,data))
    data=np.nan_to_num(data);data*=.88/max(.88,np.max(np.abs(data)))
    with wave.open(str(audio/name),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes((data*32767).astype('<i2').tobytes())
    print(name,'seconds',len(data)/rate,'peak',float(np.max(abs(data))),'rms',float(np.sqrt(np.mean(data**2))))

# Align the whoosh peak with release at encounter second 3.7.
t=np.arange(round(7.3*rate))/rate
source_times=np.interp(t,[0,1.9,7.3],[0,peak,len(whoosh)/rate])
earth=np.column_stack([np.interp(source_times*rate,np.arange(len(whoosh)),whoosh[:,ch]) for ch in range(2)])
earth*=np.minimum(1,t/.05)[:,None]*np.clip((7.3-t)/.65,0,1)[:,None]
save('Earth-Flick.wav',earth)

rng=np.random.default_rng(4725)
def noise(seconds,low,high):
    n=round(seconds*rate);freq=np.fft.rfftfreq(n,1/rate)
    spectrum=rng.normal(size=len(freq))+1j*rng.normal(size=len(freq))
    shape=(1-np.exp(-(freq/max(1,low))**4))*np.exp(-(freq/high)**4)/np.sqrt(np.maximum(freq,40))
    x=np.fft.irfft(spectrum*shape,n);return x/(np.std(x)+1e-9)

t=np.arange(4*rate)/rate
left=noise(4,70,2100);right=noise(4,70,2100)
mod=.78+.12*np.sin(2*np.pi*3.25*t)+.10*np.sin(2*np.pi*6.75*t)
tone=.15*np.sin(2*np.pi*94*t+1.8*np.sin(2*np.pi*1.25*t))+.07*np.sin(2*np.pi*188*t+.7*np.sin(2*np.pi*2.25*t))
strain=np.column_stack(((.12*left+tone)*mod,(.12*right+tone)*mod))
save('Ship-Strain-Loop.wav',strain)

t=np.arange(round(.95*rate))/rate
whip=noise(.95,150,6500)*np.exp(-((t-.12)/.08)**2)*.15
impact=(np.sin(2*np.pi*(110*t-36*t*t))*.36+noise(.95,30,1050)*.10)*np.exp(-np.maximum(0,t-.17)*10)*(t>=.17)
grab=whip+impact;grab*=np.minimum(1,t/.006)*np.minimum(1,(.95-t)/.04)
save('Ship-Bind.wav',grab)

t=np.arange(round(2.3*rate))/rate
envelope=np.minimum(1,t/.025)*np.exp(-t*2.05)*np.minimum(1,(2.3-t)/.16)
burst=(noise(2.3,80,4600)*.18+np.sin(2*np.pi*(210*t+70*(1-np.exp(-t*5))))*.16)*envelope
save('Ship-Breakaway.wav',np.column_stack((burst,np.roll(burst,330)*.92)))
