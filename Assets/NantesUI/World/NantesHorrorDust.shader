Shader "Nantes/Horror Dust"
{
    Properties
    {
        _TwinkleTime ("Time", Float) = 0
        _Opacity ("Opacity", Range(0,1)) = .68
        _ColdTint ("Cold smoke", Color) = (.018,.037,.035,1)
        _RustTint ("Warm smoke", Color) = (.050,.018,.011,1)
        _Flow ("Drift XY, detail ZW", Vector) = (.003,0,7,5)
        _Band ("Height, slope, tightness, seed", Vector) = (.46,.35,3.8,0)
        _Foreground ("Foreground wisps", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _TwinkleTime, _Opacity, _Foreground;
            float4 _ColdTint, _RustTint, _Flow, _Band;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Fragment { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            Fragment vert(Input v) { Fragment o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p)
            {
                float2 a=floor(p),f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);
            }
            float fbm(float2 p) { return noise(p)*.57+noise(p*2.07)*.27+noise(p*4.11)*.12+noise(p*8.3)*.04; }
            float4 frag(Fragment i):SV_Target
            {
                float2 uv=i.uv;
                float2 p=uv*_Flow.zw+_TwinkleTime*_Flow.xy+_Band.w;
                float2 warp=float2(fbm(p*.7),fbm(p*.7+float2(4.1,7.6)));
                float n=fbm(p+warp*lerp(1.2,2.4,_Foreground));
                float band=exp(-pow((uv.y-_Band.x-(uv.x-.5)*_Band.y)*_Band.z,2));
                float filaments=pow(saturate(1-abs(n-.53)*8),3);
                float breath=.90+.10*sin(_TwinkleTime*.13+uv.x*8+uv.y*5);
                float density=(pow(saturate(n*1.35-.28),1.5)+filaments*.10)*band*breath;
                float edge=smoothstep(0,.12,uv.x)*smoothstep(0,.12,1-uv.x)*smoothstep(0,.12,uv.y)*smoothstep(0,.12,1-uv.y);
                density*=lerp(1,edge,_Foreground);
                float3 tint=lerp(_ColdTint.rgb,_RustTint.rgb,smoothstep(.50,.92,uv.x)*.66);
                return float4(tint,density*_Opacity);
            }
            ENDCG
        }
    }
}
