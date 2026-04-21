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
            #include "UnityCG.cginc"

            fixed4 _ColorBottom;
            fixed4 _ColorTop;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;         // ¡añadido!
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 posClip  : SV_POSITION;
                float3 normalWS : TEXCOORD1;    // normal en world space
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.posClip    = UnityObjectToClipPos(v.vertex);
                o.uv         = v.uv;
                // convierte la normal a world space para comparar correctamente
                o.normalWS   = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
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
