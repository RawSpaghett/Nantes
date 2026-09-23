using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NantesGame.Tablet
{
    public sealed class TabletDisplay : MaskableGraphic
    {
        public TabletController tablet;
        public static readonly Color Ink = new Color(.77f, .84f, .78f);
        public static readonly Color Cyan = new Color(.29f, .72f, .68f);
        public static readonly Color Muted = new Color(.23f, .35f, .34f);
        public static readonly Color Amber = new Color(.79f, .61f, .32f);
        VertexHelper mesh;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); mesh = vh;
            if (!tablet) return;
            float t = tablet.Clock;
            if(tablet.squareDisplay){Square(t);return;}
            Rect(0, 0, 1440, 840, new Color(.017f, .033f, .035f));
            for (int y = 0; y < 840; y += 4) Rect(0, y, 1440, .5f, new Color(.04f, .12f, .12f, .12f));
            if (tablet.State == TabletState.Off) return;
            if (tablet.State == TabletState.Booting || tablet.State == TabletState.ShuttingDown)
            {
                float progress = tablet.Transition;
                if (tablet.State == TabletState.ShuttingDown) progress = 1 - progress;
                Icon(720, 346, 50, 1, Cyan);
                for (int i = 0; i < 24; i++) Rect(456+i*22, 462, 14, 3, i < progress*24 ? Ink : Muted * .5f);
                return;
            }
            Line(42, 92, 1398, 92, 1, Muted);
            Line(152, 124, 152, 746, 1, new Color(.13f,.24f,.23f));
            Line(42, 761, 1398, 761, 1, Muted);
            Rect(270, 42, 2, 16, Muted);
            for(int i=0;i<5;i++) Rect(1122+i*13, 52-i*3, 6, 8+i*3, i<4?Cyan:Muted);
            Circle(1220, 49, 3, 12, Cyan, true);
            Icon(1300, 50, 17, 2, Ink);
            Nav(0, 92, 184, 0, tablet.State==TabletState.Home);
            Nav(1, 92, 307, 1, tablet.State==TabletState.Scanner);
            Nav(2, 92, 683, 3, false);
            for (int i=0;i<3;i++) Rect(86+i*7, 437, 2, 29-i*6, Muted*.65f);
            if(tablet.State==TabletState.Home) Home(t); else Scanner(t);
            for(int i=0;i<21;i++) Rect(1060+i*15,790, i%4==0?3:1, i%4==0?19:12, Muted);
        }

        void Square(float t)
        {
            Rect(0,0,1024,1024,new Color(.012f,.026f,.029f));
            for(int y=0;y<1024;y+=4)Rect(0,y,1024,.45f,new Color(.04f,.12f,.12f,.10f));
            if(tablet.State==TabletState.Off)return;
            if(!tablet.IsReady){
                float p=tablet.State==TabletState.ShuttingDown?1-tablet.Transition:tablet.Transition;
                Icon(512,420,52,1,Cyan);Circle(512,420,106,90,Muted*.55f);
                Arc(512,420,106,-90,-90+Mathf.Max(.01f,p)*360,3,Ink);
                for(int i=0;i<24;i++)Rect(260+i*21,590,12,4,i<p*24?Ink:Muted*.4f);return;
            }
            Icon(836,53,21,2,Cyan);Line(36,103,988,103,1,Muted);
            Line(150,133,150,904,1,Muted*.65f);Line(36,930,988,930,1,Muted);
            Nav(0,86,184,0,tablet.State==TabletState.Home);Nav(1,86,307,1,tablet.State==TabletState.Scanner);
            Nav(2,86,834,3,false);
            if(tablet.State==TabletState.Home){
                Panel(174,136,804,770,15,new Color(.023f,.052f,.055f));
                Bracket(174,136,804,770,tablet.Hover==3?Cyan:Muted);Line(212,213,940,213,1,Muted*.7f);
                const float x=576,y=511;
                for(int i=1;i<=3;i++)Circle(x,y,i*66,100,Muted*(i==3?.9f:.5f));
                for(int i=0;i<60;i++){float a=i*Mathf.PI/30;float r=i%5==0?220:211;Line(x+Mathf.Cos(a)*204,y+Mathf.Sin(a)*204,x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,1,i%5==0?Ink*.65f:Muted);}
                Line(x-235,y,x-36,y,1,Muted);Line(x+36,y,x+235,y,1,Muted);Line(x,y-225,x,y-36,1,Muted);Line(x,y+36,x,y+225,1,Muted);
                Icon(x,y,26,1,Cyan);Arc(x,y,199,-68+Mathf.Sin(t*.22f)*23,-10+Mathf.Sin(t*.22f)*23,3,Cyan);
                Line(212,839,940,839,1,Muted*.6f);Line(853,873,916,873,2,Ink);Line(903,860,916,873,2,Ink);Line(903,886,916,873,2,Ink);
            }else{
                const float x=573,y=471,r=267;Icon(817,166,15,2,Amber);
                for(int i=1;i<=4;i++)Circle(x,y,r*i/4,100,Muted*.65f);
                for(int i=-3;i<=3;i++){float v=i*r/4,h=Mathf.Sqrt(r*r-v*v);Line(x+v,y-h,x+v,y+h,1,Muted*.28f);Line(x-h,y+v,x+h,y+v,1,Muted*.28f);}
                for(int i=0;i<100;i++){float a=i*Mathf.PI/50;float rr=i%5==0?284:275;Line(x+Mathf.Cos(a)*270,y+Mathf.Sin(a)*270,x+Mathf.Cos(a)*rr,y+Mathf.Sin(a)*rr,1,i%5==0?Ink*.5f:Muted);}
                if(tablet.ScanAge<2.3f){float rr=Mathf.Clamp01(tablet.ScanAge/1.8f)*r;for(int i=0;i<5;i++)Circle(x,y,Mathf.Max(1,rr-i*3),100,new Color(Cyan.r,Cyan.g,Cyan.b,(1-Mathf.Clamp01((tablet.ScanAge-1.25f)/1.05f))*(i==0?.85f:.09f)));}
                Arc(x,y,r-2,t*14%360-15,t*14%360,2,Cyan*.6f);Icon(x,y,14,4,Ink);
                foreach(var contact in tablet.Contacts){Vector2 p=contact.position/tablet.Range*r;if(p.magnitude>r-10)continue;float v=tablet.ContactVisibility(contact);if(v<=0)continue;Color c=contact.kind==ContactKind.Food?Amber:contact.kind==ContactKind.Movement?Cyan:Muted;c.a=v;float xx=x+p.x,yy=y-p.y;if(contact.kind==ContactKind.Food){Line(xx-7,yy,xx,yy-9,2,c);Line(xx,yy-9,xx+7,yy,2,c);Line(xx+7,yy,xx,yy+9,2,c);Line(xx,yy+9,xx-7,yy,2,c);}else Circle(xx,yy,6,14,c);}
                Panel(188,815,305,96,12,tablet.Hover==5?new Color(.08f,.17f,.16f):new Color(.035f,.07f,.072f));Bracket(188,815,305,96,Muted);Icon(434,864,22,5,Cyan);
                Panel(550,815,414,96,12,tablet.Hover==4?new Color(.08f,.19f,.18f):new Color(.035f,.088f,.084f));Bracket(550,815,414,96,Cyan);Icon(596,863,24,1,Ink);
            }
        }

        void Nav(int button,float x,float y,int icon,bool active)
        {
            if(active || tablet.Hover == button) {
                Panel(x-38,y-38,76,76,8,new Color(.055f,.135f,.134f));
                Rect(x-53,y-19,3,38,active?Cyan:Muted);
            }
            Icon(x,y,22,icon,active?Ink:Muted);
        }

        void Home(float t)
        {
            Color edge = tablet.Hover==3?Cyan:Muted;
            Panel(190,132,1207,568,14,new Color(.025f,.057f,.06f));
            Bracket(190,132,1207,568,edge);
            Line(224,204,1361,204,1,Muted*.7f);
            float x=791,y=428;
            Circle(x,y,156,100,Muted*.7f);
            Circle(x,y,105,80,Muted*.45f);
            Circle(x,y,53,60,Muted*.65f);
            for(int i=0;i<60;i++) {
                float a=i*Mathf.PI/30;
                Line(x+Mathf.Cos(a)*165,y+Mathf.Sin(a)*165,x+Mathf.Cos(a)*(i%5==0?175:169),y+Mathf.Sin(a)*(i%5==0?175:169),1,i%5==0?Ink*.6f:Muted);
            }
            Line(x-182,y,x-30,y,1,Muted);Line(x+30,y,x+182,y,1,Muted);
            Line(x,y-182,x,y-30,1,Muted);Line(x,y+30,x,y+182,1,Muted);
            Icon(x,y,23,1,Cyan);
            Arc(x,y,155,-64 + Mathf.Sin(t*.18f)*18,12+Mathf.Sin(t*.18f)*18,3,Cyan*.8f);
            Line(224,633,1362,633,1,Muted*.7f);
            Line(1297,667,1346,667,2,Ink);Line(1334,655,1346,667,2,Ink);Line(1334,679,1346,667,2,Ink);
        }

        void Scanner(float t)
        {
            const float x=570,y=421,r=260;
            for(int i=0;i<4;i++) {
                float rr=65*(i+1); Circle(x,y,rr,120,new Color(.12f,.23f,.22f));
            }
            for(int i=-4;i<=4;i++) {
                float v=i*65; float h=Mathf.Sqrt(r*r-v*v);
                Line(x+v,y-h,x+v,y+h,1,Muted*.3f);
                Line(x-h,y+v,x+h,y+v,1,Muted*.3f);
            }
            for(int i=0;i<120;i++) {
                float a=i*Mathf.PI/60;
                float rr=i%10==0?278: i%5==0?272:266;
                Line(x+Mathf.Cos(a)*262,y+Mathf.Sin(a)*262,x+Mathf.Cos(a)*rr,y+Mathf.Sin(a)*rr,1,i%10==0?Ink*.6f:Muted);
            }
            if(tablet.ScanAge < 2.3f) {
                float rr = Mathf.Clamp01(tablet.ScanAge/1.8f)*r;
                float alpha=1-Mathf.Clamp01((tablet.ScanAge-1.25f)/1.05f);
                for(int i=0;i<6;i++)Circle(x,y,Mathf.Max(1,rr-i*3),110,new Color(Cyan.r,Cyan.g,Cyan.b,alpha*(i==0?.8f:.08f)));
            }
            float sweep=t*14%360;
            Arc(x,y,257,sweep-16,sweep,2,Cyan*.65f);
            Icon(x,y,12,4,Ink);
            foreach(var contact in tablet.Contacts) {
                Vector2 p=contact.position/tablet.Range*r;
                if(p.magnitude>r-12 || contact.confidence<=0)continue;
                float visible=tablet.ContactVisibility(contact);
                if(visible<=0)continue;
                Color c=contact.kind==ContactKind.Food?Amber:contact.kind==ContactKind.Movement?Cyan:Muted;
                c.a=visible;
                float xx=x+p.x,yy=y-p.y;
                if(contact.kind==ContactKind.Food) {
                    Line(xx-7,yy,xx,yy-9,2,c);Line(xx,yy-9,xx+7,yy,2,c);
                    Line(xx+7,yy,xx,yy+9,2,c);Line(xx,yy+9,xx-7,yy,2,c);
                } else {
                    Circle(xx,yy,5,14,c);Line(xx-11,yy,xx-8,yy,1,c);Line(xx+8,yy,xx+11,yy,1,c);
                }
                if(visible>.4f) Arc(xx,yy,14,-60,60,1,new Color(c.r,c.g,c.b,c.a*.35f));
            }
            Line(945,176,945,691,1,Muted*.7f);
            Icon(1000,218,20,2,Amber);
            Line(986,270,1390,270,1,Muted*.7f);
            Icon(1000,377,20,1,Cyan);
            for(int i=0;i<39;i++) {
                float h=3+Mathf.Pow(Mathf.Sin(i*.713f+t*1.2f),10)*Mathf.Sin(i*.27f+.8f)*27;
                Rect(986+i*10,448-Mathf.Abs(h)*.5f,2,Mathf.Abs(h),Cyan*.55f);
            }
            Line(986,495,1390,495,1,Muted*.7f);
            Panel(986,557,404,74,8,tablet.Hover==4?new Color(.08f,.19f,.18f):new Color(.045f,.108f,.105f));
            Bracket(986,557,404,74,Cyan);
            Icon(1026,594,19,1,Ink);
            Rect(1370,583,2,22,Cyan);
            Icon(807,729,12,5,tablet.Hover==5?Ink:Muted);
        }

        void Bracket(float x,float y,float w,float h,Color c) {
            const float b=15;
            Line(x+b,y,x+80,y,1,c);Line(x,y+b,x,y+55,1,c);Line(x,y+b,x+b,y,1,c);
            Line(x+w-b,y,x+w-80,y,1,c);Line(x+w,y+b,x+w,y+55,1,c);Line(x+w,y+b,x+w-b,y,1,c);
            Line(x,y+h-16,x+16,y+h,1,c);Line(x+16,y+h,x+78,y+h,1,c);
            Line(x+w,y+h-16,x+w-16,y+h,1,c);Line(x+w-16,y+h,x+w-78,y+h,1,c);
        }

        public void Icon(float x,float y,float s,int type,Color c) {
            if(type==0) {
                Line(x-s,y-2,x,y-s,2,c);Line(x,y-s,x+s,y-2,2,c);
                Line(x-s*.7f,y-1,x-s*.7f,y+s*.8f,2,c);Line(x+s*.7f,y-1,x+s*.7f,y+s*.8f,2,c);
                Line(x-s*.7f,y+s*.8f,x+s*.7f,y+s*.8f,2,c);Line(x,y+s*.2f,x,y+s*.8f,2,c);
            } else if(type==1) {
                Arc(x,y,s,-70,235,2,c);Arc(x,y,s*.53f,-70,235,2,c);
                Line(x,y,x+s*.8f,y-s*.9f,2,c);Circle(x,y,2.5f,12,c,true);
            } else if(type==2) {
                Shrimp(x,y,s,c);
            } else if(type==3) {
                Arc(x,y,s,-52,232,2,c);Line(x,y-s*1.12f,x,y,2,c);
            } else if(type==4) {
                Line(x,y-s,x-s*.7f,y+s,2,c);Line(x,y-s,x+s*.7f,y+s,2,c);Line(x-s*.7f,y+s,x,y+s*.45f,2,c);Line(x+s*.7f,y+s,x,y+s*.45f,2,c);
            } else {
                Circle(x,y,s*.7f,24,c);Line(x+s*.55f,y+s*.55f,x+s*1.1f,y+s*1.1f,2,c);Line(x-s*.35f,y,x+s*.35f,y,2,c);
            }
        }

        static readonly Vector2[][] ShrimpOutline = {
            Curve(-.9f,-.4f,-.65f,-.4f,-.28f,-.4f,-.05f,-.4f),
            Curve(-.05f,-.4f,.7f,-.6f,1.12f,-.13f,.95f,.35f),
            Curve(.95f,.35f,.83f,.73f,.43f,.94f,.02f,.8f),
            Curve(.08f,.56f,.46f,.72f,.75f,.42f,.48f,.18f),
            Curve(.48f,.18f,.32f,.04f,.02f,.1f,-.22f,.03f),
            Curve(-.22f,.03f,-.6f,-.03f,-.8f,-.2f,-.9f,-.4f),
            Curve(-.69f,-.39f,-1.17f,-.91f,-.8f,-1.09f,-.25f,-.79f),
            Curve(-.62f,-.4f,-.54f,-.86f,.02f,-.98f,.55f,-.94f),
            new[]{new Vector2(.08f,.56f),new Vector2(-.27f,.45f),new Vector2(-.21f,.74f),new Vector2(.02f,.8f),new Vector2(-.2f,1.02f),new Vector2(.15f,.9f)}
        };
        static readonly Vector2[][] ShrimpSegments = {
            Curve(-.03f,-.4f,-.12f,-.21f,-.08f,-.05f,.02f,.07f),
            Curve(.37f,-.39f,.23f,-.2f,.25f,-.04f,.33f,.1f),
            Curve(.76f,-.19f,.58f,-.13f,.52f,0,.48f,.18f),
            Curve(.97f,.24f,.82f,.19f,.66f,.21f,.58f,.3f),
            Curve(.78f,.61f,.64f,.52f,.57f,.5f,.48f,.54f),
            new[]{new Vector2(-.49f,-.05f),new Vector2(-.53f,.21f),new Vector2(-.73f,.28f)},
            new[]{new Vector2(-.23f,.04f),new Vector2(-.24f,.31f),new Vector2(-.45f,.38f)}
        };
        void Shrimp(float x,float y,float s,Color c)
        {
            float width=Mathf.Clamp(s*.072f,1.1f,2);
            foreach(var path in ShrimpOutline)IconPath(path,x,y,s,width,c);
            if(s>=14)foreach(var path in ShrimpSegments)IconPath(path,x,y,s,width*.8f,c);
            Circle(x-s*.51f,y-s*.23f,Mathf.Max(.85f,s*.055f),10,c,true);
        }
        void IconPath(Vector2[] path,float x,float y,float s,float width,Color c)
        {
            for(int i=1;i<path.Length;i++)Line(x+path[i-1].x*s,y+path[i-1].y*s,x+path[i].x*s,y+path[i].y*s,width,c);
        }
        static Vector2[] Curve(float ax,float ay,float bx,float by,float cx,float cy,float dx,float dy)
        {
            var points=new Vector2[13];
            for(int i=0;i<points.Length;i++){
                float t=i/12f,u=1-t;
                points[i]=new Vector2(u*u*u*ax+3*u*u*t*bx+3*u*t*t*cx+t*t*t*dx,u*u*u*ay+3*u*u*t*by+3*u*t*t*cy+t*t*t*dy);
            }
            return points;
        }
        void Panel(float x,float y,float w,float h,float cut,Color c) {
            Vector2[] p={new Vector2(x+cut,y),new Vector2(x+w-cut,y),new Vector2(x+w,y+cut),new Vector2(x+w,y+h-cut),new Vector2(x+w-cut,y+h),new Vector2(x+cut,y+h),new Vector2(x,y+h-cut),new Vector2(x,y+cut)};
            for(int i=0;i<8;i++)Triangle(new Vector2(x+w/2,y+h/2),p[i],p[(i+1)%8],c);
        }
        void Circle(float x,float y,float r,int steps,Color c,bool filled=false) {
            if(r<=0)return;
            for(int i=0;i<steps;i++){float a=i*Mathf.PI*2/steps,b=(i+1)*Mathf.PI*2/steps;
                if(filled)Triangle(new Vector2(x,y),new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r),new Vector2(x+Mathf.Cos(b)*r,y+Mathf.Sin(b)*r),c);
                else Line(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,x+Mathf.Cos(b)*r,y+Mathf.Sin(b)*r,1,c);
            }
        }
        void Arc(float x,float y,float r,float start,float end,float width,Color c) {
            int n=Mathf.Max(3,Mathf.CeilToInt(Mathf.Abs(end-start)/4));
            for(int i=0;i<n;i++){float a=Mathf.Lerp(start,end,(float)i/n)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(float)(i+1)/n)*Mathf.Deg2Rad;Line(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,x+Mathf.Cos(b)*r,y+Mathf.Sin(b)*r,width,c);}
        }
        void Rect(float x,float y,float w,float h,Color c) { Quad(new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),c); }
        void Line(float x,float y,float xx,float yy,float width,Color c) {
            Vector2 a=new Vector2(x,y),b=new Vector2(xx,yy),v=(b-a).normalized;
            Vector2 n=new Vector2(-v.y,v.x)*width*.5f;
            Quad(a-n,b-n,b+n,a+n,c);
        }
        void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color) {
            int k=mesh.currentVertCount;Vertex(a,color);Vertex(b,color);Vertex(c,color);Vertex(d,color);mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
        }
        void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color) {
            int k=mesh.currentVertCount;Vertex(a,color);Vertex(b,color);Vertex(c,color);mesh.AddTriangle(k,k+1,k+2);
        }
        void Vertex(Vector2 p,Color c){float x=tablet.squareDisplay?512:720,y=tablet.squareDisplay?512:420;mesh.AddVert(new Vector3(p.x-x,y-p.y,0),c,Vector2.zero);}
    }
}
