Shader "Unlit/General"
{
    Properties
    {
        // Blending state
        [HideInInspector] _BlendMode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        // Cutoutの閾値
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // メインテクスチャ
        _MainTex ("Texture", 2D) = "white" {}

        // 乗算カラー
        [HDR] _TintColor("Tint Color",  Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile __ _ALPHATEST_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Cutoff;
            half4 _TintColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                #ifdef _ALPHATEST_ON
                {
                    clip(col.a - _Cutoff);
                }
                #endif

                col *= _TintColor;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
    CustomEditor "GeneralShaderGUI"
}
