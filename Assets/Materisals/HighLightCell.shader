Shader "Custom/HighLightCell"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}// текстура спрайта        
        _Color ("Tint", Color) = (1,1,1,1)                             // основной цвет (множитель)

        _HighlightColor ("Highlight Color", Color) = (1,1,0,1)         // цвет подсветки
        _HighlightStrength ("Highlight Strength", Range(0,1)) = 0.0    // сила подсветки
        _PulseSpeed ("Pulse Speed", Range(0.1,10)) = 2.0               // скорость пульсации
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"         // 👈 важно для SpriteRenderer
            "PreviewType"="Plane"              // предпросмотр в инспекторе
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
                // основной цвет спрайта
                half4 texColor = tex2D(_MainTex, i.texcoord) * i.color;

                // делаем пульсацию через синус
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);
                float highlight = _HighlightStrength * pulse;

                // добавляем подсветку
                half4 finalColor = texColor + _HighlightColor * highlight;

                // сохраняем альфу спрайта
                finalColor.a = texColor.a;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
