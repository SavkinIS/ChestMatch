Shader "Custom/HighlightCellGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}  // текстура спрайта
        _Color ("Tint", Color) = (1,1,1,1)            // базовый цвет

        _HighlightColor ("Highlight Color", Color) = (1,1,0,1)   // цвет подсветки
        _HighlightStrength ("Highlight Strength", Range(0,2)) = 1.0 // сила подсветки
        _PulseSpeed ("Pulse Speed", Range(0.1,10)) = 2.0           // скорость пульсации
        _GlowSize ("Glow Size", Range(0,10)) = 3.0                 // размер ореола
        _GlowIntensity ("Glow Intensity", Range(0,2)) = 1.0        // яркость ореола
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _HighlightColor;
            float _HighlightStrength;
            float _PulseSpeed;
            float _GlowSize;
            float _GlowIntensity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Основной цвет спрайта
                half4 texColor = tex2D(_MainTex, i.texcoord) * i.color;

                // Пульсация (от 0 до 1)
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);
                float highlight = _HighlightStrength * pulse;

                // ---------- GLOW (ореол) ----------
                // Читаем альфу в соседних пикселях (по UV)
                float2 pixelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y); // размер пикселя на экране

                float glow = 0.0;
                // Проверим 8 направлений вокруг пикселя
                float2 offsets[8] = {
                    float2( 1,  0), float2(-1,  0),
                    float2( 0,  1), float2( 0, -1),
                    float2( 1,  1), float2(-1, -1),
                    float2(-1,  1), float2( 1, -1)
                };

                for (int k = 0; k < 8; k++)
                {
                    float alpha = tex2D(_MainTex, i.texcoord + offsets[k] * pixelSize * _GlowSize).a;
                    glow += alpha;
                }

                glow = saturate(glow / 8.0); // нормализуем
                glow *= _GlowIntensity;      // регулируем яркость

                // Финальный цвет = спрайт + подсветка + ореол
                half4 finalColor = texColor;
                finalColor.rgb += (_HighlightColor.rgb * highlight) + (_HighlightColor.rgb * glow);

                // Прозрачность = альфа спрайта (glow без альфы, иначе будет "размазывать фон")
                finalColor.a = texColor.a;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
