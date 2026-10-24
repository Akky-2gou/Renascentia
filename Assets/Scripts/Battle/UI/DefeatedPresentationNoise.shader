Shader "UI/DefeatedPresentationNoise"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)

        _FillAmount ("Fill Amount", Range(0,1)) = 1

        _CircleRadius ("Circle Radius (UV)", Range(0,1)) = 0.25
        _CircleCount ("Circle Count", Range(1,32)) = 16
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
            #pragma target 3.0

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
            float _CircleRadius;
            float _CircleCount;
            float4 _CircleCenters[32];

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
                
                // 配置された点からradiusより離れていれば透明
                float radius = _CircleRadius * _FillAmount;
                float nearestCenterDistance = 1e20;

                for (int index = 0; index < _CircleCount; index++)
                {
                    float distanceToCenter = distance(i.uv, _CircleCenters[index].xy);
                    nearestCenterDistance = min(nearestCenterDistance, distanceToCenter);
                }

                if (nearestCenterDistance > radius)
                {
                    discard;
                }

                return col;
            }

            ENDCG
        }
    }
}