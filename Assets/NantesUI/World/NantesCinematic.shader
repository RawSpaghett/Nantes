Shader "Nantes/Cinematic World"
{
    Properties
    {
        _MainTex ("World",2D)="black" {}
        _BloomTex ("Bloom",2D)="black" {}
        _Exposure ("Exposure",Float)=1.18
        _Bloom ("Bloom intensity",Float)=.24
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex,_BloomTex;
        float4 _MainTex_TexelSize;
        float _Exposure,_Bloom;
        float3 radiance(float3 c)
        { return float3(c.r>=0?min(c.r,16):0,c.g>=0?min(c.g,16):0,c.b>=0?min(c.b,16):0); }
        float3 tone(float3 x) { return saturate((x*(2.51*x+.03))/(x*(2.43*x+.59)+.14)); }
        float4 grade(v2f_img i):SV_Target
        {
            float3 c=radiance(tex2D(_MainTex,i.uv).rgb)+radiance(tex2D(_BloomTex,i.uv).rgb)*_Bloom;
            float l=dot(c,float3(.2126,.7152,.0722));
            c=lerp(l.xxx,c,.82);
            c*=lerp(float3(.72,.89,1.10),float3(1.05,1.01,.94),smoothstep(.035,.65,l));
            c=tone(max(0,c-.0002)*_Exposure);
            float2 p=(i.uv-float2(.46,.52))*float2(1,.83);
            c*=1-.35*smoothstep(.22,.68,length(p));
            return float4(c,1);
        }
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment grade
            ENDCG }
    }
}
