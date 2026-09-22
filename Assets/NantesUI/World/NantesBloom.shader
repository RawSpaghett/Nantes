Shader "Nantes/Bloom Filter"
{
    Properties { _MainTex ("Source",2D)="black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        // Stop invalid pixels from spreading through bloom.
        float3 radiance(float3 c)
        { return float3(c.r>=0?min(c.r,16):0,c.g>=0?min(c.g,16):0,c.b>=0?min(c.b,16):0); }
        float4 threshold(v2f_img i):SV_Target
        {
            float2 d=_MainTex_TexelSize.xy;
            float3 c=(radiance(tex2D(_MainTex,i.uv+d).rgb)+radiance(tex2D(_MainTex,i.uv-d).rgb)+radiance(tex2D(_MainTex,i.uv+d*float2(-1,1)).rgb)+radiance(tex2D(_MainTex,i.uv+d*float2(1,-1)).rgb))*.25;
            float b=max(c.r,max(c.g,c.b));
            return float4(c*smoothstep(.48,1.2,b)*.7,1);
        }
        float4 blur(float2 uv,float2 d)
        {
            return tex2D(_MainTex,uv)*.227027+
                (tex2D(_MainTex,uv+d*1.384615)+tex2D(_MainTex,uv-d*1.384615))*.316216+
                (tex2D(_MainTex,uv+d*3.230769)+tex2D(_MainTex,uv-d*3.230769))*.070270;
        }
        float4 horizontal(v2f_img i):SV_Target { return blur(i.uv,float2(_MainTex_TexelSize.x,0)*1.4); }
        float4 vertical(v2f_img i):SV_Target { return blur(i.uv,float2(0,_MainTex_TexelSize.y)*1.4); }
        ENDCG
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment threshold
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment horizontal
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment vertical
            ENDCG }
    }
}
