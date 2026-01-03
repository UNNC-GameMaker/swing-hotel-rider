Shader "Custom/URP/LiquidMask"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 10)) = 2.0
        _Distortion ("Liquid Distortion", Range(0, 0.2)) = 0.05
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // 声明 URP 2D Renderer 自动生成的排序层纹理
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _BlurSize;
                float _Distortion;
                float4 _CameraSortingLayerTexture_TexelSize; // 自动获取纹理尺寸
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            // 简单的 9 宫格采样模糊
            float4 BoxBlur(float2 uv, float blurSize)
            {
                float4 col = float4(0,0,0,0);
                float2 texelSize = _CameraSortingLayerTexture_TexelSize.xy * blurSize;

                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(-texelSize.x, -texelSize.y));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(0, -texelSize.y));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(texelSize.x, -texelSize.y));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(-texelSize.x, 0));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv);
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(texelSize.x, 0));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(-texelSize.x, texelSize.y));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(0, texelSize.y));
                col += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, uv + float2(texelSize.x, texelSize.y));

                return col / 9.0;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 1. 采样遮罩 (当前 Sprite)
                float4 mask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // 如果遮罩是透明的，直接丢弃，不进行后续计算
                if(mask.a < 0.01) discard;

                // 2. 计算屏幕坐标 UV
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // 3. 扭曲效果 (基于遮罩的颜色或随机噪声，这里简单用遮罩UV偏移)
                // 让边缘产生液体的折射感
                float2 distortion = (mask.rg - 0.5) * _Distortion * mask.a;
                screenUV += distortion;

                // 4. 采样背景 (只包含 Foremost Sorting Layer 及其之下的层)
                // 这里采样的就是第一步中设置的 Texture
                float4 bgCol = BoxBlur(screenUV, _BlurSize);

                // 5. 混合颜色
                float4 finalCol = bgCol * IN.color;
                finalCol.a = mask.a; // 保持遮罩的透明度形状

                return finalCol;
            }
            ENDHLSL
        }
    }
}