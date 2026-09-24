Shader "Nantes/World Surface"
{
    Properties
    {
        _BaseMap ("Surface", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bump" {}
        _Roughness ("Roughness", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _Meters ("Metres per repeat", Float) = 2
        _WallBand ("Old wall paint", Float) = 0
        _Smoothness ("Smoothness", Range(0,1)) = .3
        _Saturation ("Saturation", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST, _BaseColor;
            float _Meters, _WallBand, _Smoothness, _Saturation;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_Roughness); SAMPLER(sampler_Roughness);
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; half fog : TEXCOORD2; };
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.fog=ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float3 normal=normalize(i.normalWS);
                float3 weights=pow(abs(normal),6); weights/=max(dot(weights,1),.0001);
                float3 p=i.positionWS/max(_Meters,.01);
                half3 albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy).rgb*weights.x
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb*weights.y
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb*weights.z;
                half rough=SAMPLE_TEXTURE2D(_Roughness,sampler_Roughness,p.zy).r*weights.x
                    +SAMPLE_TEXTURE2D(_Roughness,sampler_Roughness,p.xz).r*weights.y
                    +SAMPLE_TEXTURE2D(_Roughness,sampler_Roughness,p.xy).r*weights.z;
                half3 nx=UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.zy));
                half3 ny=UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.xz));
                half3 nz=UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,p.xy));
                float3 detail=float3(nx.z*sign(normal.x),nx.y,nx.x)*weights.x
                    +float3(ny.x,ny.z*sign(normal.y),ny.y)*weights.y
                    +float3(nz.x,nz.y,nz.z*sign(normal.z))*weights.z;
                normal=normalize(lerp(normal,detail,.45));
                float mottling=.91+.09*sin(i.positionWS.x*.81+sin(i.positionWS.z*.43))*sin(i.positionWS.y*1.23+i.positionWS.z*.71);
                float band=_WallBand*(1-smoothstep(1.08,1.13,i.positionWS.y));
                albedo=lerp(dot(albedo,half3(.2126,.7152,.0722)).xxx,albedo,_Saturation);
                albedo*=_BaseColor.rgb*mottling*lerp(half3(1,1,1),half3(.38,.48,.44),band);
                InputData input=(InputData)0;
                input.positionWS=i.positionWS; input.normalWS=normal;
                input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                input.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
                input.bakedGI=SampleSH(normal); input.vertexLighting=VertexLighting(i.positionWS,normal);
                input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                input.shadowMask=half4(1,1,1,1);
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=albedo; surface.alpha=1; surface.metallic=0;
                surface.smoothness=(1-rough)*_Smoothness;surface.occlusion=1;surface.normalTS=half3(0,0,1);
                half4 color=UniversalFragmentPBR(input,surface);
                color.rgb=MixFog(color.rgb,i.fog);
                return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection, _LightPosition;
            struct ShadowAttributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            float4 ShadowVertex(ShadowAttributes input):SV_POSITION
            {
                float3 position=TransformObjectToWorld(input.positionOS.xyz);
                float3 normal=TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 direction=normalize(_LightPosition-position);
                #else
                float3 direction=_LightDirection;
                #endif
                return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(position,normal,direction)));
            }
            half4 ShadowFragment():SV_TARGET { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment
            float4 DepthVertex(float4 position:POSITION):SV_POSITION { return TransformObjectToHClip(position.xyz); }
            half4 DepthFragment():SV_TARGET { return 0; }
            ENDHLSL
        }
    }
}
