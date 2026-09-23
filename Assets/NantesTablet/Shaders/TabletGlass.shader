Shader "Nantes/Tablet Glass"
{
    Properties
    {
        _MainTex("Display", 2D) = "black" {}
        _Power("Power", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+2" }
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Power;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float3 normal:NORMAL; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; float3 normal:TEXCOORD2; };
            v2f vert(appdata v) {
                v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);return o;
            }
            fixed4 frag(v2f i):SV_Target {
                float2 uv=i.uv;
                float3 display=tex2D(_MainTex,uv).rgb;
                float2 edge=abs(uv-.5)*2;
                float vignette=1-.2*pow(max(edge.x,edge.y),8);
                float reflection=pow(saturate(1-abs(uv.x*.55+uv.y*.8-.88)),26)*.014;
                float fresnel=pow(1-abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),4);
                float noise=frac(sin(dot(uv*1024,float2(127.1,311.7)))*43758.5453);
                float3 glass=float3(.009,.016,.018)+reflection*float3(.55,.78,.8)+fresnel*.016;
                return float4(display*_Power*vignette+glass+(noise-.5)*.0015,1);
            }
            ENDCG
        }
    }
}
