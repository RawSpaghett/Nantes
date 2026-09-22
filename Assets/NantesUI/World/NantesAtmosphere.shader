Shader "Nantes/Planet Atmosphere"
{
    Properties
    {
        _Color ("Scattered light",Color)=(.16,.48,.65,1)
        _LightDirection ("Sun direction",Vector)=(-.58,.62,.18,0)
        _Intensity ("Scattering",Float)=1
    }
    SubShader
    {
        Tags {"Queue"="Transparent-30" "RenderType"="Transparent"}
        Blend SrcAlpha One
        ZWrite Off Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Color,_LightDirection;float _Intensity;
            struct Input{float4 vertex:POSITION;float3 normal:NORMAL;};
            struct Fragment{float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;};
            Fragment vert(Input v){Fragment o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
            float4 frag(Fragment i):SV_Target
            {
                float3 n=normalize(i.normal),v=normalize(_WorldSpaceCameraPos-i.world),l=normalize(_LightDirection.xyz);
                float mu=saturate(dot(n,v));
                float rim=saturate((mu-sqrt(max(0,mu*mu-.01776)))/.13327);
                float daylight=smoothstep(-.10,.55,dot(n,l));
                float density=rim*(.025+daylight*.72)*_Intensity;
                return float4(_Color.rgb,density);
            }
            ENDCG
        }
    }
}
