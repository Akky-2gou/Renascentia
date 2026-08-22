Shader "UI/SkillGaugeWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)

        _FillAmount ("Fill Amount", Range(0,1)) = 1

        _WaveHeight ("Wave Height", Range(0,0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(1,30)) = 10
        _WaveSpeed ("Wave Speed", Range(0,10)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            fixed4 _Color;

            float _FillAmount;
            float _WaveHeight;
            float _WaveFrequency;
            float _WaveSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 波を作る
                float wave = sin(i.uv.x * _WaveFrequency + _Time.y * _WaveSpeed);

                // FillAmountの境界位置
                float boundary =_FillAmount - _WaveHeight + (wave + 1.0) * 0.5 * _WaveHeight * 2.0;

                // 境界より上を透明にする
                if (i.uv.y > boundary)
                {
                    discard;
                }

                return col;
            }

            ENDCG
        }
    }
}