Shader "UI/UniformOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _PixelsPerUnit ("Pixels Per Unit", Float) = 100.0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.01
        
        // UI specific properties
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
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            // Outline properties
            float4 _OutlineColor;
            float _OutlineThickness;
            float _PixelsPerUnit;
            float4 _AddColor;
            float _AlphaThreshold;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = TransformObjectToHClip(v.vertex.xyz);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                // Calculate outline thickness in UV space
                float thickness = _OutlineThickness / _PixelsPerUnit;
                
                // Sample the main texture
                float4 color = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                float maxAlpha = color.a;

                // Apply add color
                color.rgb += _AddColor.rgb;
                color.rgb = saturate(color.rgb);

                // Check if current pixel is part of sprite
                bool isCurrentPixelSprite = color.a > _AlphaThreshold;
                
                // Check surrounding pixels for outline
                bool hasNeighborSprite = false;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = thickness * float2(x, y);
                        float neighborAlpha = tex2D(_MainTex, IN.texcoord + offset).a;
                        
                        if (neighborAlpha > _AlphaThreshold)
                        {
                            hasNeighborSprite = true;
                            maxAlpha = max(maxAlpha, 1.0);
                        }
                    }
                }

                // Determine if we should draw outline
                bool shouldDrawOutline = (!isCurrentPixelSprite && hasNeighborSprite);
                
                // Calculate final color
                float4 finalColor;
                if (shouldDrawOutline)
                {
                    finalColor = _OutlineColor;
                }
                else
                {
                    finalColor = color * IN.color;
                }

                // Apply UI masking
                #ifdef UNITY_UI_CLIP_RECT
                float2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                finalColor.a *= m.x * m.y;
                #endif

                // Apply alpha clipping if enabled
                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDHLSL
        }
    }
}