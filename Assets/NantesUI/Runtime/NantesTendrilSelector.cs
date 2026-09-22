using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

namespace NantesGame.UI
{
    [DefaultExecutionOrder(75)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesTendrilSelector : MaskableGraphic
    {
        public NantesMenu menu;
        public bool automatic=true;
        public AudioClip whipSound;
        [Range(0,1)] public float whipVolume=.7f;
        public float WhipElapsed { get; private set; } = -1;
        const float BackswingTime=.36f,StrikePeak=.70f,ActivationTime=.94f,StrikeDuration=1.12f;
        public RectTransform Target { get; private set; }
        public float Wrap { get; private set; }
        public float StrikeStrength { get; private set; }
        public bool IsStriking => strikeTime<StrikeDuration;
        public bool ActivationPending => pending!=null;
        public Vector2 FocusCenter => center;
        const int StemSegments=96,CoilSegments=180,Sides=12;
        readonly Vector2[] points=new Vector2[StemSegments+CoilSegments+1];
        readonly Vector2[] strikeStart=new Vector2[StemSegments+CoilSegments+1];
        readonly Vector2[] releaseStart=new Vector2[StemSegments+CoilSegments+1];
        readonly Vector3[] corners=new Vector3[4];
        Selectable hovered;
        Vector2 center,velocity,size=new Vector2(400,60);
        float time,strikeTime=10,travelAge=10,travelDuration=.6f,releaseTime=10;
        RectTransform destination;
        Vector2 departure,travelBow;
        int approach;
        AudioSource whipVoice;
        NantesSpaceMotion motion;
        Renderer surfaceSource;
        MeshRenderer limbRenderer;
        Mesh limbMesh;
        Vector3[] worldPoints,vertices;
        Canvas ownerCanvas;
        MaterialPropertyBlock lighting;
        Rect strikeRect;
        RectTransform strikeTarget;
        UnityAction pending;
        bool ready;

        public void Initialize(NantesMenu owner)
        {
            menu=owner;raycastTarget=false;maskable=false;
            foreach(var control in menu.GetComponentsInChildren<Selectable>(true))
            {
                var target=control.GetComponent<NantesTendrilTarget>();
                if(target==null)target=control.gameObject.AddComponent<NantesTendrilTarget>();
                target.selector=this;
            }
            center=IdleTip();
            motion=FindAnyObjectByType<NantesSpaceMotion>();
            ownerCanvas=owner.GetComponent<Canvas>();
            var presence=motion!=null?motion.GetComponentInChildren<NantesForegroundPresence>():null;
            if(presence!=null&&presence.limbs.Length>0){
                surfaceSource=presence.limbs[0].GetComponent<Renderer>();
                CreateLimb();
            }
            whipVoice=gameObject.AddComponent<AudioSource>();whipVoice.playOnAwake=false;
            whipVoice.spatialBlend=0;whipVoice.dopplerLevel=0;whipVoice.loop=false;
            whipVoice.clip=whipSound;whipVoice.volume=whipVolume;
            ready=true;
            RenderStep(0);
        }

        public void Hover(Selectable control){hovered=control;}
        public void Leave(Selectable control){if(hovered==control)hovered=null;}
        public void PageChanged(){CancelStrike();hovered=null;Target=null;destination=null;}

        void CancelStrike()
        {
            if(ready&&IsStriking){System.Array.Copy(points,releaseStart,points.Length);releaseTime=0;}
            pending=null;strikeTime=10;WhipElapsed=-1;
            if(whipVoice!=null)whipVoice.Stop();
        }

        public void Strike(RectTransform target,UnityAction action)
        {
            if(IsStriking)return;
            if(menu.ReducedMotion){action?.Invoke();return;}
            System.Array.Copy(points,strikeStart,points.Length);releaseTime=10;
            strikeRect=Bounds(target);strikeTarget=target;strikeTime=0;pending=action;
            center=strikeRect.center;size=strikeRect.size;velocity=Vector2.zero;
            Wrap=Mathf.Max(Wrap,.90f);Target=target;destination=target;travelAge=travelDuration;
            WhipElapsed=0;
            if(whipVoice!=null&&whipSound!=null){whipVoice.clip=whipSound;whipVoice.volume=whipVolume;whipVoice.time=0;whipVoice.Play();}
        }

        void Update(){if(automatic)Advance(Time.unscaledDeltaTime);}
        void LateUpdate(){if(automatic)RenderStep(Time.unscaledDeltaTime);}
        public void Evaluate(float deltaTime){Advance(deltaTime);RenderStep(deltaTime);}

        void Advance(float deltaTime)
        {
            if(!ready)return;
            if(IsStriking){
                var struck=strikeTarget!=null?strikeTarget.GetComponent<Selectable>():null;
                if(struck==null||!struck.isActiveAndEnabled||!struck.IsInteractable())CancelStrike();
            }
            if(menu.ReducedMotion&&pending!=null)
            {CompleteActivation();CancelStrike();}
            if(!menu.ReducedMotion)time+=deltaTime;
            strikeTime+=deltaTime;
            travelAge+=deltaTime;
            releaseTime+=deltaTime;
            if(WhipElapsed>=0){
                WhipElapsed+=deltaTime;
                if(whipSound==null||WhipElapsed>=whipSound.length){WhipElapsed=-1;if(whipVoice!=null)whipVoice.Stop();}
                else if(whipVoice!=null&&Mathf.Abs(whipVoice.time-WhipElapsed)>.10f){
                    whipVoice.time=WhipElapsed;if(!whipVoice.isPlaying)whipVoice.Play();
                }
            }
            // Wait for the whip to finish before changing pages.
            if(pending!=null&&strikeTime>=ActivationTime)CompleteActivation();
        }

        void CompleteActivation()
        {
            var action=pending;pending=null;
            var control=strikeTarget!=null?strikeTarget.GetComponent<Selectable>():null;
            if(control!=null&&control.isActiveAndEnabled&&control.IsInteractable())action?.Invoke();
        }

        void RenderStep(float deltaTime)
        {
            if(!ready)return;
            float dt=Mathf.Min(.05f,deltaTime);
            var selected=EventSystem.current!=null?EventSystem.current.currentSelectedGameObject:null;
            var control=menu.KeyboardNavigation?(selected!=null?selected.GetComponent<Selectable>():null):hovered;
            bool strike=IsStriking&&!menu.ReducedMotion;
            if(strike&&strikeTarget!=null)control=strikeTarget.GetComponent<Selectable>();
            bool active=control!=null&&control.isActiveAndEnabled&&control.IsInteractable();
            Target=active?(RectTransform)control.transform:null;
            Rect wanted=active?Bounds(Target):new Rect(IdleTip()-new Vector2(180,35),new Vector2(360,70));
            if(strike)wanted=strikeRect;
            if(Target!=destination&&!strike){
                destination=Target;departure=center;travelAge=0;approach++;
                float distance=Vector2.Distance(departure,wanted.center);
                travelDuration=Mathf.Clamp(.30f+distance/850,.40f,.76f);
                float side=approach%3==0?1:-1;
                travelBow=new Vector2(side*Mathf.Clamp(40+distance*.25f,50,150),Mathf.Sin(approach*2.4f)*38);
            }
            if(menu.ReducedMotion)
            {
                center=wanted.center;size=wanted.size;velocity=Vector2.zero;travelAge=travelDuration;
                Wrap=active?1:0;StrikeStrength=0;
            }
            else
            {
                float p=Mathf.Clamp01(travelAge/travelDuration);
                if(active&&!strike&&p<1){
                    center=Vector2.Lerp(departure,wanted.center,Mathf.SmoothStep(0,1,p))+travelBow*Mathf.Sin(p*Mathf.PI);
                    velocity=Vector2.zero;
                }
                else center=Vector2.SmoothDamp(center,wanted.center,ref velocity,active?.12f:.55f,2800,dt);
                size=Vector2.Lerp(size,wanted.size,1-Mathf.Exp(-dt*18));
                float extent=active?1-.75f*Mathf.Sin(p*Mathf.PI):0;
                Wrap=Mathf.MoveTowards(Wrap,strike?1:extent,dt*4.5f);
                StrikeStrength=strike?Mathf.Exp(-Mathf.Pow((strikeTime-StrikePeak)/.06f,2)):0;
            }
            Shape();
            SetVerticesDirty();
        }

        Rect Bounds(RectTransform target)
        {
            // Fit the visible text without changing its hit area.
            if(target.GetComponent<Button>()!=null){
                var item=target.GetComponent<NantesMenuItem>();
                var label=item!=null?item.label:target.GetComponentInChildren<TMP_Text>();
                if(label!=null){
                    label.ForceMeshUpdate();
                    Vector2 lo=new Vector2(float.PositiveInfinity,float.PositiveInfinity),hi=-lo;
                    for(int i=0;i<label.textInfo.characterCount;i++){
                        var glyph=label.textInfo.characterInfo[i];if(!glyph.isVisible)continue;
                        Vector2 a=rectTransform.InverseTransformPoint(label.transform.TransformPoint(glyph.bottomLeft));
                        Vector2 b=rectTransform.InverseTransformPoint(label.transform.TransformPoint(glyph.topRight));
                        lo=Vector2.Min(lo,Vector2.Min(a,b));hi=Vector2.Max(hi,Vector2.Max(a,b));
                    }
                    if(!float.IsInfinity(lo.x))return Rect.MinMaxRect(lo.x-14,lo.y-14,hi.x+14,hi.y+14);
                }
            }
            target.GetWorldCorners(corners);
            Vector2 min=rectTransform.InverseTransformPoint(corners[0]);
            Vector2 max=rectTransform.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x-12,min.y-8,max.x+12,max.y+8);
        }

        // UI anchor only; the mesh renders with the scene.
        protected override void OnPopulateMesh(VertexHelper mesh){mesh.Clear();}

        Vector2 IdleTip()
        {
            var r=rectTransform.rect;
            return new Vector2(r.xMin+r.width*.59f+Mathf.Sin(time*.31f)*48,
                r.center.y-35+Mathf.Sin(time*.47f)*62+Mathf.Sin(time*.19f)*31);
        }

        void Shape()
        {
            var r=rectTransform.rect;
            bool reduced=menu.ReducedMotion;
            Vector2 dimensions=size+new Vector2(Mathf.Sin(time*.83f)*7,Mathf.Sin(time*1.1f)*4);
            Rect box=new Rect(center-dimensions*.5f,dimensions);
            Vector2 root=new Vector2(r.xMin-130,r.yMin+r.height*.24f);
            Vector2 shoulder=new Vector2(r.xMin+r.width*.23f,root.y+100+Mathf.Sin(time*.51f)*40);
            Vector2 restingTip=IdleTip();
            Vector2 idleWrist=new Vector2(r.xMin+r.width*.48f,r.center.y+125+Mathf.Cos(time*.37f)*48);
            Vector2 entry=CoilPoint(box,0);
            float reachWave=Mathf.Sin(Mathf.Clamp01(travelAge/travelDuration)*Mathf.PI);
            Vector2 wrist=entry+new Vector2(-185,25+Mathf.Sin(time*.8f)*18+reachWave*Mathf.Sin(approach*1.9f)*70);
            float reach=Mathf.SmoothStep(0,1,Wrap);
            for(int i=0;i<points.Length;i++)
            {
                float u=(float)i/(points.Length-1);
                Vector2 idle=Bezier(root,shoulder,idleWrist,restingTip,u);
                float distal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,1,u));
                idle+=new Vector2(Mathf.Sin(u*10-time*.83f)*18,Mathf.Sin(u*12-time*1.17f)*27)*distal;
                idle+=new Vector2(-Mathf.Sin(u*5+time*.5f)*28,Mathf.Cos(u*6-time*.63f)*19)*u*u;
                Vector2 focused;
                if(i<=StemSegments){
                    float v=(float)i/StemSegments;
                    focused=Bezier(root,shoulder,wrist,entry,v);
                    focused+=new Vector2(Mathf.Sin(v*10-time*.91f)*19,Mathf.Sin(v*13-time*1.21f)*24)*Mathf.Sin(v*Mathf.PI);
                }
                else focused=CoilPoint(box,(float)(i-StemSegments)/CoilSegments);
                points[i]=Vector2.Lerp(idle,focused,reach);
            }
            if(!reduced&&IsStriking)ShapeWhip(root);
            else if(!reduced&&releaseTime<.32f){
                float settle=Mathf.SmoothStep(0,1,releaseTime/.32f);
                for(int i=0;i<points.Length;i++)points[i]=Vector2.Lerp(releaseStart[i],points[i],settle);
            }
        }

        void ShapeWhip(Vector2 root)
        {
            var r=rectTransform.rect;
            Vector2 hit=strikeRect.center+new Vector2(strikeRect.width*.18f,0);
            float pull=Mathf.Min(r.width*.36f,hit.x-r.xMin-100);
            Vector2 back=hit+new Vector2(-pull,20);
            Vector2 handle=root+new Vector2((back.x-root.x)*.62f,Mathf.Clamp(hit.y-root.y+120,-220,360));
            Vector2 fold=back+new Vector2(300,250);
            float draw=Mathf.SmoothStep(0,1,Mathf.Clamp01(strikeTime/BackswingTime));
            float launch=Mathf.Clamp01((strikeTime-BackswingTime)/(StrikePeak-BackswingTime));
            float recoil=Mathf.Clamp01((strikeTime-StrikePeak)/.24f);
            float settle=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.92f,StrikeDuration,strikeTime));
            Vector2 tip=Vector2.Lerp(back,hit,Mathf.Pow(launch,2.15f));
            tip+=new Vector2(-145,65)*Mathf.SmoothStep(0,1,recoil);
            Vector2 a=Vector2.Lerp(handle,Vector2.Lerp(root,hit,.42f)+new Vector2(0,-100),launch);
            Vector2 b=Vector2.Lerp(fold,hit-new Vector2(235,-20),launch);
            b+=new Vector2(-80,110)*Mathf.Sin(recoil*Mathf.PI*.5f);
            float crest=Mathf.Lerp(.18f,1,Mathf.Pow(launch,.72f));
            float width=Mathf.Lerp(.18f,.035f,launch);
            float amplitude=320*Mathf.Sin(launch*Mathf.PI);
            for(int i=0;i<points.Length;i++){
                float u=(float)i/(points.Length-1);
                Vector2 rope=Bezier(root,a,b,tip,u);
                float q=(u-crest)/width;
                rope.y+=-q*Mathf.Exp(-q*q*1.5f)*amplitude*Mathf.Sin(u*Mathf.PI);
                Vector2 strike=Vector2.Lerp(strikeStart[i],rope,draw);
                points[i]=Vector2.Lerp(strike,points[i],settle);
            }
        }

        Vector2 CoilPoint(Rect box,float u)
        {
            float along=u*.965f;
            Vector2 point=Perimeter(box,along);
            Vector2 tangent=(Perimeter(box,Mathf.Min(1,along+.001f))-Perimeter(box,Mathf.Max(0,along-.001f))).normalized;
            Vector2 normal=new Vector2(-tangent.y,tangent.x);
            float breathe=Mathf.Sin(along*11-time*1.35f)*4.2f+Mathf.Sin(along*23+time*.81f)*1.8f;
            point+=normal*breathe;
            float tip=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.83f,1,u));
            point+=new Vector2(Mathf.Sin(time*1.31f)*11,Mathf.Cos(time*.94f)*10)*tip;
            return point;
        }

        static Vector2 Bezier(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t)
        {float q=1-t;return q*q*q*a+3*q*q*t*b+3*q*t*t*c+t*t*t*d;}

        void CreateLimb()
        {
            var go=new GameObject("Menu searching tendril",typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(motion.transform,false);go.layer=surfaceSource.gameObject.layer;
            limbRenderer=go.GetComponent<MeshRenderer>();limbRenderer.sharedMaterial=surfaceSource.sharedMaterial;
            limbRenderer.shadowCastingMode=ShadowCastingMode.Off;limbRenderer.receiveShadows=false;
            limbMesh=new Mesh{name="Flowing menu tendril"};limbMesh.MarkDynamic();
            worldPoints=new Vector3[points.Length];vertices=new Vector3[points.Length*Sides];
            var uv=new Vector2[vertices.Length];var triangles=new int[(points.Length-1)*Sides*6];
            for(int j=0;j<points.Length;j++)for(int k=0;k<Sides;k++){
                uv[j*Sides+k]=new Vector2((float)k/Sides,(float)j/(points.Length-1));
                if(j==points.Length-1)continue;
                int a=j*Sides+k,b=j*Sides+(k+1)%Sides,c=a+Sides,d=b+Sides,index=(j*Sides+k)*6;
                triangles[index]=a;triangles[index+1]=c;triangles[index+2]=b;
                triangles[index+3]=b;triangles[index+4]=c;triangles[index+5]=d;
            }
            limbMesh.vertices=vertices;limbMesh.uv=uv;limbMesh.triangles=triangles;
            go.GetComponent<MeshFilter>().sharedMesh=limbMesh;lighting=new MaterialPropertyBlock();
        }

        Vector2 ViewportPoint(Vector2 p)
        {
            Vector3 world=rectTransform.TransformPoint(p);
            if(ownerCanvas.renderMode!=RenderMode.ScreenSpaceOverlay&&ownerCanvas.worldCamera!=null)
                return ownerCanvas.worldCamera.WorldToViewportPoint(world);
            Vector2 screen=RectTransformUtility.WorldToScreenPoint(null,world);
            return new Vector2(screen.x/Mathf.Max(1,Screen.width),screen.y/Mathf.Max(1,Screen.height));
        }

        void RenderLimb(Camera camera)
        {
            if(!ready||limbRenderer==null||camera.cameraType!=CameraType.Game||
                (camera.cullingMask&(1<<limbRenderer.gameObject.layer))==0)return;
            const float depth=12.5f;
            Vector2 zero=ViewportPoint(Vector2.zero),unit=ViewportPoint(Vector2.right);
            float scale=Vector3.Distance(camera.ViewportToWorldPoint(new Vector3(zero.x,zero.y,depth)),camera.ViewportToWorldPoint(new Vector3(unit.x,unit.y,depth)));
            for(int i=0;i<points.Length;i++){
                Vector2 viewport=ViewportPoint(points[i]);float u=(float)i/(points.Length-1);
                float z=depth+Mathf.Sin(u*9-time*.57f)*.16f*u;
                worldPoints[i]=camera.ViewportToWorldPoint(new Vector3(viewport.x,viewport.y,z));
            }
            Vector3 side=camera.transform.up;
            for(int j=0;j<points.Length;j++){
                float u=(float)j/(points.Length-1);
                Vector3 tangent=(worldPoints[Mathf.Min(points.Length-1,j+1)]-worldPoints[Mathf.Max(0,j-1)]).normalized;
                side=Vector3.ProjectOnPlane(side,tangent);
                if(side.sqrMagnitude<.001f)side=Vector3.Cross(tangent,camera.transform.forward);
                side.Normalize();Vector3 up=Vector3.Cross(side,tangent).normalized;
                float ambientRadius=Mathf.Lerp(13,.75f,Mathf.Pow(u,.77f));
                float focusRadius=j<=StemSegments?Mathf.Lerp(13,4.8f,(float)j/StemSegments):Mathf.Lerp(4.8f,.65f,(float)(j-StemSegments)/CoilSegments);
                float radius=Mathf.Lerp(ambientRadius,focusRadius,Mathf.SmoothStep(0,1,Wrap));
                radius*=scale*(1+.055f*Mathf.Cos(u*170))*(1+StrikeStrength*.20f);
                for(int k=0;k<Sides;k++){
                    float a=(float)k/Sides*Mathf.PI*2;
                    vertices[j*Sides+k]=limbRenderer.transform.InverseTransformPoint(worldPoints[j]+side*Mathf.Cos(a)*radius+up*Mathf.Sin(a)*radius*.83f);
                }
            }
            limbMesh.vertices=vertices;limbMesh.RecalculateNormals();limbMesh.RecalculateBounds();
            surfaceSource.GetPropertyBlock(lighting);limbRenderer.SetPropertyBlock(lighting);
        }

        void PipelineRendering(ScriptableRenderContext context,Camera camera){RenderLimb(camera);}
        protected override void OnEnable()
        {
            base.OnEnable();Camera.onPreCull+=RenderLimb;RenderPipelineManager.beginCameraRendering+=PipelineRendering;
            if(limbRenderer!=null)limbRenderer.gameObject.SetActive(true);
        }
        protected override void OnDisable()
        {
            Camera.onPreCull-=RenderLimb;RenderPipelineManager.beginCameraRendering-=PipelineRendering;
            CancelStrike();hovered=null;Target=null;
            if(limbRenderer!=null)limbRenderer.gameObject.SetActive(false);
            base.OnDisable();
        }
        protected override void OnDestroy()
        {
            if(limbRenderer!=null)Destroy(limbRenderer.gameObject);
            if(limbMesh!=null)Destroy(limbMesh);
            if(whipVoice!=null)Destroy(whipVoice);
            base.OnDestroy();
        }

        static Vector2 Perimeter(Rect r,float t)
        {
            float radius=Mathf.Min(22,r.height*.36f),w=r.width-2*radius,h=r.height-2*radius;
            float arc=Mathf.PI*radius*.5f,total=2*w+2*h+4*arc,s=Mathf.Clamp01(t)*total;
            Vector2 p;
            if(s<w)p=new Vector2(r.xMax-radius-s,r.yMin);
            else if((s-=w)<arc)p=Corner(new Vector2(r.xMin+radius,r.yMin+radius),radius,-90-s/arc*90);
            else if((s-=arc)<h)p=new Vector2(r.xMin,r.yMin+radius+s);
            else if((s-=h)<arc)p=Corner(new Vector2(r.xMin+radius,r.yMax-radius),radius,180-s/arc*90);
            else if((s-=arc)<w)p=new Vector2(r.xMin+radius+s,r.yMax);
            else if((s-=w)<arc)p=Corner(new Vector2(r.xMax-radius,r.yMax-radius),radius,90-s/arc*90);
            else if((s-=arc)<h)p=new Vector2(r.xMax,r.yMax-radius-s);
            else p=Corner(new Vector2(r.xMax-radius,r.yMin+radius),radius,-Mathf.Clamp01((s-h)/arc)*90);
            p.x=2*r.center.x-p.x;p.y=2*r.center.y-p.y;return p;
        }
        static Vector2 Corner(Vector2 center,float radius,float degrees)
        {float a=degrees*Mathf.Deg2Rad;return center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}
    }
}
