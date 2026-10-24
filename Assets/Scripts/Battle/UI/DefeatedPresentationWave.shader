Shader "UI/DefeatedPresentationWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)

        _FillAmount ("Fill Amount", Range(0,1)) = 1

        _CenterPos ("Center Position", Vector) = (-0.1, 0.1, 0, 0)
        _WaveHeight ("Wave Height", Range(0,1.0)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(1,30)) = 10
        _WaveSpeed ("Wave Speed", Range(0,10)) = 2
        _WaveFrequency2 ("Wave Frequency 2", Range(1,30)) = 17
        _WaveSpeed2 ("Wave Speed 2", Range(0,10)) = 3
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

            float4 _CenterPos;
            float _FillAmount;
            float _WaveHeight;
            float _WaveFrequency;
            float _WaveSpeed;
            float _WaveFrequency2;
            float _WaveSpeed2;

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
                
                // 中心からの距離・角度を求める
                float2 centeredUV = i.uv - _CenterPos.xy;
                float radius = length(centeredUV);
                float angle = atan2(centeredUV.y, centeredUV.x); // ラジアン、-π〜π

                // 周波数と速度の異なる波を重ねて、境界に揺らぎを加える
                float wave1 = sin(angle * _WaveFrequency + _Time.y * _WaveSpeed);
                float wave2 = sin(angle * _WaveFrequency2 + _Time.y * _WaveSpeed2);
                float wave = (wave1 + wave2 * 0.5) / 1.5;

                // 境界位置を計算（_FillAmount = 1で全て埋まるように補正）
                float2 closestUV = clamp(_CenterPos.xy, float2(0.0, 0.0), float2(1.0, 1.0));
                float minRadius = length(closestUV - _CenterPos.xy) - _WaveHeight * _FillAmount;
                float2 farthestDelta = max(abs(_CenterPos.xy), abs(float2(1.0, 1.0) - _CenterPos.xy));
                float maxRadius = length(farthestDelta) + _WaveHeight * _FillAmount;
                float boundary = minRadius + (maxRadius - minRadius) * _FillAmount + wave * _WaveHeight * _FillAmount;

                // 境界より遠い部分を透明にする
                if (radius > boundary)
                {
                    discard;
                }

                return col;
            }

            ENDCG
        }
    }
}