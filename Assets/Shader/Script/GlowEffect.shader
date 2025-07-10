Shader "UI/GlowEffect"
{
    Properties
    {
        // -----------------------------------------------------
        // 基本テクスチャとカラー設定
        // -----------------------------------------------------
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        
        // -----------------------------------------------------
        // グロー効果（発光効果）の設定
        // -----------------------------------------------------
        [Header(Glow Settings)]
        _GlowColor ("Glow Color", Color) = (1,1,1,1) // 色
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0 // 強さ
        _GlowSize ("Glow Size", Range(0, 20)) = 5.0 // サイズ（ピクセル単位）
        _GlowSamples ("Glow Quality", Range(4, 32)) = 16 // 品質（サンプル数）
        
        // -----------------------------------------------------
        // インナーグロー（内側の発光）の設定
        // -----------------------------------------------------
        [Header(Inner Glow)]
        _InnerGlow ("Inner Glow", Range(0, 1)) = 0.5 // 強さ
        _InnerGlowColor ("Inner Glow Color", Color) = (1,1,1,0.5) // 色
        
        // -----------------------------------------------------
        // アウトラインの設定
        // -----------------------------------------------------
        [Header(Outline)]
        _OutlineWidth ("Outline Width", Range(0, 10)) = 0 // 幅
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // 色
        
        // -----------------------------------------------------
        // UI Canvas用標準プロパティ
        // -----------------------------------------------------
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        // -----------------------------------------------------
        // レンダリングタグ設定
        // -----------------------------------------------------
        Tags
        {
            "Queue"="Transparent" // 透明オブジェクトとして描画
            "IgnoreProjector"="True" // プロジェクターを虫
            "RenderType"="Transparent" // 透明レンダリングタイプ
            "PreviewType"="Plane" // プレビューでは平面として表示
            "CanUseSpriteAtlas"="True" // SpriteAtlas使用可
        }
        
        // -----------------------------------------------------
        // ステンシル設定
        // -----------------------------------------------------
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        // -----------------------------------------------------
        // レンダリング状態設定
        // -----------------------------------------------------
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "UI_Glow"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            // 構造体
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 頂点Shaderからフラグメントシェーダーへの出力
            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // シェーダープロパティ
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            half4 _Color;
            half4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize;
            int _GlowSamples;
            float _InnerGlow;
            half4 _InnerGlowColor;
            float _OutlineWidth;
            half4 _OutlineColor;
            float4 _ClipRect;

            // -----------------------------------------------------
            // 頂点シェーダー
            // -----------------------------------------------------
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = TransformObjectToHClip(OUT.worldPosition.xyz);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                
                return OUT;
            }
            
            // 距離に基づくグロー強度計算
            float CalculateGlowIntensity(float distance, float maxDistance)
            {
                float normalized = distance / maxDistance;
                return exp(-normalized * normalized * 2.0);
            }
            
            // アウトライン検出
            float GetOutlineAlpha(float2 uv, float width)
            {
                if (width <= 0) return 0;
                
                float2 texelSize = _MainTex_TexelSize.xy;
                float maxAlpha = 0;
                
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        float2 offset = float2(x, y) * texelSize * width;
                        float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset).a;
                        maxAlpha = max(maxAlpha, alpha);
                    }
                }
                
                return maxAlpha;
            }

            // -----------------------------------------------------
            // フラグメントシェーダー
            // -----------------------------------------------------
            half4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                // メインテクスチャをサンプリング
                half4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // グロー効果の計算
                half4 glowAccum = half4(0, 0, 0, 0);
                float totalWeight = 0;
                
                float2 texelSize = _MainTex_TexelSize.xy;
                float glowRadius = _GlowSize * texelSize.x;
                
                // 放射状にサンプリングしてグロー効果を作成
                for (int i = 0; i < _GlowSamples; i++)
                {
                    float angle = (float(i) / float(_GlowSamples)) * 6.28318530718; // 2π
                    
                    for (int j = 1; j <= 8; j++)
                    {
                        float distance = (float(j) / 8.0) * glowRadius;
                        float2 offset = float2(cos(angle), sin(angle)) * distance;
                        float2 sampleUV = uv + offset;
                        
                        half4 sampleColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV);
                        float intensity = CalculateGlowIntensity(distance, glowRadius);
                        
                        glowAccum += sampleColor * intensity;
                        totalWeight += intensity;
                    }
                }
                
                // グロー色の正規化と適用
                if (totalWeight > 0)
                {
                    glowAccum /= totalWeight;
                    glowAccum.rgb = _GlowColor.rgb;
                    glowAccum.a *= _GlowIntensity * _GlowColor.a;
                }
                
                // アウトライン効果
                half4 outlineColor = half4(0, 0, 0, 0);
                if (_OutlineWidth > 0)
                {
                    float outlineAlpha = GetOutlineAlpha(uv, _OutlineWidth);
                    outlineColor = _OutlineColor;
                    outlineColor.a *= outlineAlpha * (1.0 - mainColor.a);
                }
                
                // インナーグロー効果
                half4 innerGlowColor = half4(0, 0, 0, 0);
                if (_InnerGlow > 0)
                {
                    float innerGlowMask = pow(mainColor.a, 1.0 - _InnerGlow);
                    innerGlowColor = _InnerGlowColor * innerGlowMask;
                }
                
                // 最終的な合成
                half4 finalColor = mainColor * IN.color;
                
                // インナーグローを適用
                finalColor.rgb = lerp(finalColor.rgb, innerGlowColor.rgb, innerGlowColor.a);
                
                // アウトラインと元の色を合成
                finalColor.rgb = lerp(finalColor.rgb, outlineColor.rgb, outlineColor.a);
                finalColor.a = max(finalColor.a, outlineColor.a);
                
                // グロー効果を加算
                finalColor.rgb += glowAccum.rgb * glowAccum.a;
                finalColor.a = max(finalColor.a, glowAccum.a);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    // -----------------------------------------------------
    // 古いハードウェアやURP未対応環境用のフォールバック用SubShader
    // -----------------------------------------------------
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "Fallback"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragFallback
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _MainTex_ST;
            half4 _Color;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(v.vertex.xyz);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            half4 fragFallback(v2f IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord);
                color *= IN.color;
                return color;
            }
            ENDHLSL
        }
    }
}