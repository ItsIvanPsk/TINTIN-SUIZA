Shader "Custom/BlueToAlphaGradient_NoTopCap"
{
    Properties
    {
        _ColorBottom("Color Base", Color) = (0.4, 0.6, 1, 1)
        _ColorTop("Color Cima", Color)    = (0.4, 0.6, 1, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            fixed4 _ColorBottom;
            fixed4 _ColorTop;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 posClip  : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.posClip    = UnityObjectToClipPos(v.vertex);
                o.uv         = v.uv;
                o.normalWS   = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // si la normal apunta casi hacia arriba, descartamos (tapa superior)
                if (i.normalWS.y > 0.9)
                    discard;

                // sigue usando el degradado por UV Y en el lateral
                float g = saturate(i.uv.y);
                return lerp(_ColorBottom, _ColorTop, g);
            }
            ENDCG
        }
    }
}
