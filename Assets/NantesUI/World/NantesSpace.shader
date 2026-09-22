Shader "Nantes/Space Surface"
{
    Properties
    {
        _MainTex ("Surface", 2D) = "white" {}
        _HeightMap ("Elevation", 2D) = "gray" {}
        _NormalMap ("Surface normal", 2D) = "bump" {}
        _NormalStrength ("Surface normal strength", Range(0,1)) = 0
        _Saturation ("Surface color saturation", Range(0,1)) = 0
        _AngularBlur ("Planet rotational shutter", Float) = 0
        _SpinAxis ("Object-space spin axis", Vector) = (0,1,0,0)
        _RimColor ("Edge light color", Color) = (1,1,1,1)
        _Color ("Tint", Color) = (1,1,1,1)
        _LightDirection ("Direction to sun", Vector) = (-0.6,0.9,0.15,0)
        _Relief ("Relief", Float) = 0
        _Ambient ("Ambient", Float) = 0.006
        _Exposure ("Exposure", Float) = 1.4
        _Specular ("Metal highlight", Float) = 0
        _Emission ("Emission", Float) = 0
        _EmissionMap ("Emission map", 2D) = "black" {}
        _MappedEmission ("Emission map strength", Float) = 0
        _EmissionTint ("Emission map tint", Color) = (0.08,0.72,1,1)
        _Rim ("Edge light", Float) = 0.3
        _BouncePosition ("Planet bounce position", Vector) = (-6,-9.6,7,1)
        _BounceStrength ("Planet reflected light", Float) = 0
        _AlbedoFloor ("Minimum metal reflectance", Range(0,1)) = 0
        _RimPower ("Rim falloff",Float)=7
        _Gloss ("Highlight focus",Float)=80
        _SurfaceDetail ("Chitin detail",Range(0,1))=0
        _EnginePosition ("Engine light position",Vector)=(0,0,0,0)
        _EngineColor ("Engine light",Color)=(0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex, _HeightMap, _NormalMap, _EmissionMap;
            float4 _EmissionTint;
            float _MappedEmission;
            float4 _HeightMap_TexelSize, _Color, _LightDirection, _BouncePosition, _RimColor;
            float _Saturation, _NormalStrength, _AngularBlur;
            float4 _SpinAxis;
            float _Relief, _Ambient, _Exposure, _Specular, _Emission, _Rim, _BounceStrength, _AlbedoFloor;
            float _RimPower,_Gloss,_SurfaceDetail;
            float4 _EnginePosition,_EngineColor;
            struct Input { float4 vertex:POSITION; float3 normal:NORMAL; float4 tangent:TANGENT; float2 uv:TEXCOORD0; };
            struct Fragment { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 normal:TEXCOORD1; float3 tangent:TEXCOORD2; float3 bitangent:TEXCOORD3; float3 world:TEXCOORD4; float3 local:TEXCOORD5; };
            // Guard zero normals and tangents.
            float3 safeDirection(float3 v) { return v*rsqrt(max(dot(v,v),1e-12)); }
            Fragment vert(Input v)
            {
                Fragment o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = safeDirection(mul(v.normal,(float3x3)unity_WorldToObject));
                o.tangent = safeDirection(mul((float3x3)unity_ObjectToWorld,v.tangent.xyz));
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w * unity_WorldTransformParams.w;
                o.uv = v.uv; o.local=v.vertex.xyz;
                return o;
            }
            float4 frag(Fragment i):SV_Target
            {
                float2 d = _HeightMap_TexelSize.xy;
                float du = tex2D(_HeightMap, i.uv + float2(d.x,0)).r - tex2D(_HeightMap, i.uv - float2(d.x,0)).r;
                float dv = tex2D(_HeightMap, i.uv + float2(0,d.y)).r - tex2D(_HeightMap, i.uv - float2(0,d.y)).r;
                float3 N = safeDirection(i.normal - _Relief * (du * i.tangent + dv * i.bitangent));
                float3 detail = UnpackNormal(tex2D(_NormalMap, i.uv));
                N = safeDirection(N + _NormalStrength * (detail.x * safeDirection(i.tangent) + detail.y * safeDirection(i.bitangent)));
                float3 L = safeDirection(_LightDirection.xyz);
                float3 V = safeDirection(_WorldSpaceCameraPos - i.world);
                float3 surface = tex2D(_MainTex, i.uv).rgb;
                if(_AngularBlur>.00001)
                {
                    // Convert the spin direction to a UV blur direction.
                    float3 px=ddx(i.local),py=ddy(i.local);
                    float2 ux=ddx(i.uv),uy=ddy(i.uv);
                    ux.x-=round(ux.x);uy.x-=round(uy.x);
                    float3 travel=cross(safeDirection(_SpinAxis.xyz),i.local);
                    float aa=dot(px,px),bb=dot(px,py),cc=dot(py,py);
                    float det=max(1e-12,aa*cc-bb*bb);
                    float dx=(dot(travel,px)*cc-dot(travel,py)*bb)/det;
                    float dy=(dot(travel,py)*aa-dot(travel,px)*bb)/det;
                    float2 sweep=(ux*dx+uy*dy)*_AngularBlur;
                    sweep=clamp(sweep,-.07,.07);
                    // Overlap the blur samples to avoid repeated edges.
                    float2 blurDx=ux+sweep/12,blurDy=uy;
                    surface=tex2Dgrad(_MainTex,i.uv,blurDx,blurDy).rgb;
                    float weight=1;
                    [unroll] for(int tap=1;tap<=12;tap++)
                    {
                        float offset=tap/12.0;
                        float w=1-offset*.65;
                        surface+=tex2Dgrad(_MainTex,i.uv+sweep*offset,blurDx,blurDy).rgb*w;
                        surface+=tex2Dgrad(_MainTex,i.uv-sweep*offset,blurDx,blurDy).rgb*w;
                        weight+=w*2;
                    }
                    surface/=weight;
                }
                float3 albedo = lerp(dot(surface, float3(.2126,.7152,.0722)).xxx, surface, _Saturation);
                albedo = max(albedo, _AlbedoFloor);
                float diffuse = max(0, dot(N,L));
                float ridge=pow(saturate(.5+.5*sin(i.uv.y*174+sin(i.uv.x*6.283)*2)),5);
                float rim = pow(1 - saturate(dot(safeDirection(i.normal),V)), _RimPower) * saturate(dot(safeDirection(i.normal),L) + .18) * _Rim;
                float spec = pow(saturate(dot(N, safeDirection(L + V))), _Gloss) * _Specular*lerp(1,.35+.65*ridge,_SurfaceDetail);
                float bounce = max(0, dot(N, safeDirection(_BouncePosition.xyz - i.world))) * _BounceStrength;
                float3 light = _Color.rgb * (albedo * (_Ambient + diffuse * _Exposure + bounce) + _Emission) + rim * _RimColor.rgb + spec*float3(.65,.79,.84);
                float3 toEngine=_EnginePosition.xyz-i.world;
                float engineLight=(max(0,dot(N,safeDirection(toEngine)))+.06)/(1+dot(toEngine,toEngine)*.65);
                light+=_EngineColor.rgb*engineLight*(albedo*.35+.04);
                float3 emissionMask = tex2D(_EmissionMap,i.uv).rgb;
                light += max(emissionMask.r,max(emissionMask.g,emissionMask.b))*_MappedEmission*_EmissionTint.rgb;
                return float4(light, 1);
            }
            ENDCG
        }
    }
}
