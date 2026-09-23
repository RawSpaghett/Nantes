Shader "Nantes/World/Overcast Sky"
{
    Properties
    {
        _Panorama("Cloud Panorama", 2D) = "gray" {}
        _Exposure("Exposure", Range(0,4)) = 0.7
        _Rotation("Rotation", Range(0,360)) = 85
        _Tint("Cloud Tint", Color) = (0.72, 0.78, 0.76, 1)
        _Horizon("Dust Haze", Color) = (0.14, 0.135, 0.105, 1)
        _CloudCover("Cloud Shadow", Range(0,1)) = 0.18
        _Wind("Wind Speed", Range(0,0.1)) = 0.003
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Pass
        {
            Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint, _Horizon;
                float _Exposure, _Rotation, _CloudCover, _Wind;
            CBUFFER_END
            TEXTURE2D(_Panorama); SAMPLER(sampler_Panorama);
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            Varyings Vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.direction=v.positionOS.xyz; return o; }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            float Fbm(float2 p)
            {
                float value=0, gain=.53;
                for(int i=0;i<5;i++){value+=Noise(p)*gain;p=mul(float2x2(.8,-.6,.6,.8),p)*2.07+13.2;gain*=.48;}
                return value;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 d=normalize(i.direction);
                float angle=radians(_Rotation)+_Time.y*_Wind*.003;
                float2 rotated=mul(float2x2(cos(angle),-sin(angle),sin(angle),cos(angle)),d.xz);
                float2 panoramaUV=float2(.5-atan2(rotated.y,rotated.x)/(2*PI),1-acos(clamp(d.y,-1,1))/PI);
                float2 dx=ddx(panoramaUV), dy=ddy(panoramaUV);
                dx.x-=round(dx.x); dy.x-=round(dy.x);
                float3 photo=SAMPLE_TEXTURE2D_GRAD(_Panorama,sampler_Panorama,panoramaUV,dx,dy).rgb;
                float luminance=dot(photo,float3(.2126,.7152,.0722));
                float3 color=lerp(luminance.xxx,photo,.2)*_Tint.rgb*_Exposure;
                float2 uv=d.xz/max(d.y+.2,.045)*.8;
                float2 wind=float2(_Time.y*_Wind,_Time.y*_Wind*.31);
                float large=Fbm(uv*.8+wind);
                float small=Fbm(uv*2.6+large*2.2+wind*.65);
                color*=1-_CloudCover*smoothstep(.3,.7,large*.72+small*.28)*smoothstep(0,.15,d.y);
                float horizonHaze=exp(-abs(d.y)*9);
                color=lerp(color,_Horizon.rgb,horizonHaze*.6);
                color=lerp(_Horizon.rgb,color,smoothstep(-.15,.02,d.y));
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
