
Shader "UI/RichText"
{
    Properties
    {
        _MainTex("Font Texture", 2D) = "white" {}
        _SpriteTex("Sprite Texture", 2D) = "white" {}
        _Color("Text Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Lighting Off
        Cull Off
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 uv0 	: TEXCOORD0;
                float2 uv1 	: TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 uv0 	: TEXCOORD0;
                float2 uv1 	: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SpriteTex;
            float4 _SpriteTex_ST;

            uniform fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _SpriteTex);
                o.color = v.color;// *_Color;

                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 result = i.color * i.uv1.x;
                result.a *= (tex2D(_MainTex, i.uv0)).a;
                result += i.uv1.y * i.color * tex2D(_SpriteTex, i.uv0);

                return result;
            }
            ENDCG
        }
    }
}
