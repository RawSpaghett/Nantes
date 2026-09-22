Shader "Nantes/Engraved Logo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Wordmark", 2D) = "white" {}
        _LogoTime ("Animation time", Float) = 0
        _Corruption ("Corruption", Range(0,1)) = 0
        _ReducedMotion ("Reduced motion", Float) = 0
        _VesselMask ("Foreground vessel silhouette", 2D) = "black" {}
        _VesselInFront ("Vessel crosses title", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _VesselMask;
            float _VesselInFront;
            float4 _MainTex_TexelSize;
            float _LogoTime, _Corruption, _ReducedMotion;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Fragment { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; float4 screen:TEXCOORD1; };
            Fragment vert(Input v) { Fragment o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; o.screen=ComputeScreenPos(o.vertex); return o; }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float4 frag(Fragment i):SV_Target
            {
                float c=_Corruption*(1-_ReducedMotion);
                float tick=floor(_LogoTime*12);
                float band=floor(i.uv.y*11);
                float block=floor(i.uv.x*6);
                float stutter=hash(float2(tick,17));
                float2 uv=i.uv;
                float slice=(hash(float2(band,tick))-.5)*step(.46,hash(float2(band,tick+51)));
                uv.x+=c*(slice*.028+(stutter-.5)*.008);
                uv.y+=c*(hash(float2(block,tick+23))-.5)*.019;
                float4 original=tex2D(_MainTex,uv);
                float echo=tex2D(_MainTex,uv+float2(.008*c,0)).a;
                float edge=saturate(echo-original.a)*c*.34;
                float3 color=lerp(original.rgb,float3(.47,.62,.60),c*.14);
                color=lerp(color,float3(.10,.36,.44),edge);
                float alpha=max(original.a,edge);
                float cover=tex2D(_VesselMask,i.screen.xy/i.screen.w).a;
                alpha*=1-saturate(cover)*_VesselInFront;
                return float4(color,alpha)*i.color;
            }
            ENDCG
        }
    }
}
