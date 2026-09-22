Shader "Nantes/Distant Stars"
{
    Properties { _TwinkleTime ("Animation time", Float) = 0 _Trail ("Trail profile", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _TwinkleTime, _Trail;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; float2 phase:TEXCOORD1; float4 color:COLOR; };
            struct Fragment { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 phase:TEXCOORD1; float4 color:COLOR; };
            Fragment vert(Input v)
            {
                Fragment o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.phase=v.phase; o.color=v.color; return o;
            }
            float4 frag(Fragment i):SV_Target
            {
                float r=dot(i.uv,i.uv);
                float core=exp(-r*5.5) * saturate(1-r);
                if (_Trail > .5) core = pow(saturate(1-abs(i.uv.y*2-1)), 2);
                float twinkle=.78 + .15*sin(_TwinkleTime*i.phase.y+i.phase.x)+.07*sin(_TwinkleTime*i.phase.y*.37+i.phase.x*2);
                return float4(i.color.rgb, i.color.a*core*twinkle);
            }
            ENDCG
        }
    }
}
