Shader "Nantes/Tablet Preview Optics"
{
    Properties { _MainTex("Scene",2D)="white"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float Depth(float2 uv){return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));}
            fixed4 frag(v2f_img i):SV_Target
            {
                float depth=Depth(i.uv);
                float blur=saturate((depth-11)/7);
                float3 c=tex2D(_MainTex,i.uv).rgb;
                if(blur>.01){
                    float3 total=c;float weight=1;
                    for(int n=0;n<12;n++){
                        float a=n*2.399963;float2 offset=float2(cos(a),sin(a))*sqrt((n+1)/12.0)*8*_MainTex_TexelSize.xy*blur;
                        float2 uv=i.uv+offset;float w=step(11,Depth(uv));total+=tex2D(_MainTex,uv).rgb*w;weight+=w;
                    }
                    c=total/weight;
                }
                float background=smoothstep(11,18,depth);
                c*=lerp(1.0,.33,background);
                c=max(0,c-.0025);
                float2 v=(i.uv-.5)*float2(1.15,.94);
                c*=1-saturate(dot(v,v)*.82);
                return float4(c,1);
            }
            ENDCG
        }
    }
}
