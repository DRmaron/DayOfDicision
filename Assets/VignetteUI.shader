Shader "Custom/VignetteUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _VignetteSize ("Vignette Size", Range(0, 3)) = 1.0 // 画面端の暗さの範囲
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 texcoord : TEXCOORD0; float4 color : COLOR; };

            sampler2D _MainTex;
            fixed4 _Color;
            float _VignetteSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 画面の中心からの距離を計算（中心が 0, 0）
                float2 uv = IN.texcoord - float2(0.5, 0.5);
                
                // 中心からの距離の2乗（これで綺麗な丸い減衰になる）
                float dist = dot(uv, uv) * _VignetteSize;

                // 画像の元の色（黒や赤）に、距離に応じた透明度を掛け合わせる
                fixed4 col = _Color;
                col.a *= saturate(dist * 4.0); // 4.0はグラデーションの滑らかさ調整

                return col;
            }
            ENDCG
        }
    }
}