Shader "Nantes/Pause Backdrop"
{
    Properties {
        [PerRendererData] _MainTex("Texture",2D)="white" {}
        _MotionTime("Motion time",Float)=0
        _Color("Tint",Color)=(1,1,1,1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _MotionTime;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            struct V {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct F {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            F vert(V v){F o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            float4 frag(F i):SV_Target {
                float2 p=i.uv;
                float edge=smoothstep(.17,.73,length((p-.5)*float2(1.2,1)));
                float panel=smoothstep(.39,.70,p.x);
                float mist=sin(p.x*4.2+p.y*2+_MotionTime*.045)*.5+.5;
                float2 d=_MainTex_TexelSize.xy*3;
                float3 blur=tex2D(_MainTex,p).rgb*.2;
                blur+=(tex2D(_MainTex,p+float2(d.x,0)).rgb+tex2D(_MainTex,p-float2(d.x,0)).rgb)*.1;
                blur+=(tex2D(_MainTex,p+float2(0,d.y)).rgb+tex2D(_MainTex,p-float2(0,d.y)).rgb)*.1;
                blur+=(tex2D(_MainTex,p+d).rgb+tex2D(_MainTex,p-d).rgb+tex2D(_MainTex,p+float2(d.x,-d.y)).rgb+tex2D(_MainTex,p+float2(-d.x,d.y)).rgb)*.1;
                float3 c=blur*float3(.12,.20,.24)*lerp(.38,.12,panel)*(1-edge*.65);
                c+=lerp(float3(.009,.017,.022),float3(.003,.008,.012),panel)+mist*.0015;
                return float4(c,1)*i.color;
            }
            ENDCG
        }
    }
}
