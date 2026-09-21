Shader "Nantes/Engine Exhaust"
{
    Properties { _Thrust ("Throttle", Range(0,1))=.4 _JetTime ("Time", Float)=0 _Auxiliary ("Maneuvering jet",Range(0,1))=0 }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha One
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Thrust, _JetTime, _Auxiliary;
            float3 safeDirection(float3 v) { return v*rsqrt(max(dot(v,v),1e-12)); }
            struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float3 normal:NORMAL;float4 color:COLOR;};
            struct Fragment {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 view:TEXCOORD2;float kind:TEXCOORD3;};
            Fragment vert(Input v){
                Fragment o;
                float tail=v.uv.y*v.uv.y*(1-step(.8,v.color.r));
                v.vertex.xy+=float2(sin(v.uv.y*31-_JetTime*1.4),cos(v.uv.y*24-_JetTime*1.7))*tail*.045*_Thrust;
                o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.normal=safeDirection(mul(v.normal,(float3x3)unity_WorldToObject));o.view=_WorldSpaceCameraPos-mul(unity_ObjectToWorld,v.vertex).xyz;o.kind=v.color.r;return o;
            }
            float4 frag(Fragment i):SV_Target
            {
                float ring=step(.8,i.kind),core=1-step(.2,i.kind);
                // Small jets skip the rings to avoid white flashes.
                clip(.5-ring*_Auxiliary);
                float thrust=saturate(_Thrust);
                float lengthFade=pow(saturate(1-i.uv.y),.65);
                float face=pow(saturate(abs(dot(safeDirection(i.normal),safeDirection(i.view)))),.65);
                float pulse=.78+.16*sin(i.uv.y*51-_JetTime)+.06*sin(i.uv.x*31+_JetTime*.81);
                float hot=exp(-i.uv.y*lerp(15,4,thrust))*core*smoothstep(.15,.9,thrust);
                float energy=smoothstep(.04,.92,thrust);
                float3 plume=lerp(float3(.008,.08,.58),float3(.035,.58,1.20),energy);
                float whiteCore=pow(saturate(hot),.55)*(1-_Auxiliary);
                float3 color=lerp(plume,float3(1.75,1.95,2.05),whiteCore);
                color=lerp(color,lerp(float3(.018,.25,.8),float3(.48,1.1,1.65),energy),ring);
                color*=lerp(.32,1.5,energy);
                float beam=lengthFade*(.12+.65*face)*lerp(.34,1,core);
                float ringsVisible=smoothstep(.12+i.uv.y*.38,.40+i.uv.y*.55,thrust);
                float alpha=lerp(beam,(.65+.25*face)*ringsVisible,ring)*pulse*smoothstep(.008,.18,thrust)*(.12+1.05*pow(thrust,.7));
                return float4(color*lerp(1,.38,_Auxiliary),saturate(alpha));
            }
            ENDCG
        }
    }
}
