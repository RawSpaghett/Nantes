Shader "Nantes/World/Object Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0.85, 0.065, 0.035, 1)
        _Width("Width (pixels)", Range(0, 8)) = 2.5
        [HideInInspector] _Cull("Cull", Float) = 0
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [HideInInspector] _StencilComp("Stencil Compare", Float) = 6
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" "RenderType"="Transparent" "DisableBatching"="True" }
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            ColorMask [_ColorMask]
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 1
                Comp [_StencilComp]
                Pass [_StencilOp]
            }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _Width;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 normalVS = TransformWorldToViewDir(normalWS);
                float2 direction = mul((float2x2)UNITY_MATRIX_P, normalVS.xy);
                direction *= _ScaledScreenParams.xy;
                direction /= max(length(direction), 0.0001);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionCS.xy += direction * (2.0 * _Width / _ScaledScreenParams.xy) * o.positionCS.w;
                return o;
            }
            half4 Frag(Varyings i):SV_Target { return _OutlineColor; }
            ENDHLSL
        }
    }
}
