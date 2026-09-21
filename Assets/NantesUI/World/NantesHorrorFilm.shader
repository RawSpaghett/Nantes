Shader "Nantes/Horror Film"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} _SignalTime ("Time", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _SignalTime;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Fragment { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            Fragment vert(Input v) { Fragment o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float4 frag(Fragment i):SV_Target
            {
                float2 p=(i.uv-.5)*float2(1.16,1);
                float edge=smoothstep(.32,.79,length(p));
                float grain=hash(floor(i.uv*_ScreenParams.xy)+floor(_SignalTime*12));
                float alpha=edge*.10 + grain*.018;
                float3 ink=lerp(float3(.010,.016,.020),float3(0,0,0),edge);
                return float4(ink,alpha);
            }
            ENDCG
        }
    }
}
