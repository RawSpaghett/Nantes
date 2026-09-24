from pathlib import Path
import argparse
import numpy as np
from scipy.io import wavfile
from scipy import signal
root=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser()
parser.add_argument('--voice', type=Path, help='Unpacked Male Adventurer RPG voice folder')
args=parser.parse_args()
dest=root/"Assets/NantesGame/Resources/GameplayAudio"
dest.mkdir(parents=True,exist_ok=True)
sr=44100
rng=np.random.default_rng(814)
def save(name,data):
    data=np.asarray(data)
    data=data-np.mean(data)
    peak=np.max(np.abs(data))
    if peak>.92:data=data*(.92/peak)
    wavfile.write(dest/(name+".wav"),sr,(np.clip(data,-1,1)*32767).astype(np.int16))
def noise(n,low,high):
    return signal.sosfilt(signal.butter(2,[low,high],btype="bandpass",fs=sr,output="sos"),rng.normal(0,1,n))
#gear teeth, motor friction and a low dynamo hum
t=np.arange(sr*2)/sr
pulse=(.5+.5*np.sin(2*np.pi*14*t))**14
crank=.19*noise(len(t),180,5000)*(.25+pulse)+.04*np.sin(2*np.pi*112*t)+.022*np.sin(2*np.pi*224*t)
fade=np.minimum(np.minimum(t/.025,(2-t)/.025),1)
save("Crank",crank*fade)
buzz=(.07*np.sin(2*np.pi*100*t)+.035*np.sin(2*np.pi*200*t)+.018*np.sin(2*np.pi*600*t))
buzz+=(.018*noise(len(t),900,4500))
save("Bulb-Buzz",buzz*fade)
t=np.arange(int(sr*.22))/sr
save("Bulb-On",(.19*noise(len(t),600,6500)+.08*np.sin(2*np.pi*380*t))*np.exp(-t*30)*np.minimum(t/.003,1))
t=np.arange(int(sr*.7))/sr
phase=2*np.pi*(165*t-85*t*t)
save("Crank-Down",(.085*np.sin(phase)+.032*noise(len(t),300,3800))*np.sin(np.pi*t/.7)**2*(1-t/.7))
#two short nonverbal calls from the CC0 voice pack
for i,source in enumerate(["attackbig0.wav","attackbig2.wav"] if args.voice else []):
    rate,x=wavfile.read(next(args.voice.rglob(source)))
    x=x.astype(np.float64)/(32768 if x.dtype==np.int16 else 2147483648) if x.dtype.kind=="i" else x.astype(float)
    if x.ndim>1:x=x.mean(axis=1)
    active=np.where(np.abs(x)>.012)[0]
    if len(active):x=x[max(0,active[0]-int(.025*rate)):min(len(x),active[-1]+int(.09*rate))]
    #slower, lower voice; a little close-room reflection
    x=signal.resample(x,int(len(x)*sr/rate/.9))
    y=np.pad(x,(0,int(.12*sr)))
    y[int(.065*sr):int(.065*sr)+len(x)]+=x*.09
    y=signal.sosfilt(signal.butter(2,[110,6500],btype="bandpass",fs=sr,output="sos"),y)
    n=min(int(.008*sr),len(y)//2); y[:n]*=np.linspace(0,1,n);y[-n:]*=np.linspace(1,0,n)
    y*=.79/max(np.max(np.abs(y)),.001)
    save("Yell-"+str(i+1),y)
for f in dest.glob("*.wav"):
    rate,x=wavfile.read(f);print(f.name,round(len(x)/rate,2),"seconds", "peak",int(np.max(np.abs(x))))


