Shader "Custom/UniformOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        // Assuming a uniform Pixels Per Unit for illustration purposes
        _PixelsPerUnit ("Pixels Per Unit", Float) = 100.0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "OUTLINE"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _PixelsPerUnit;
            float4 _AddColor;
            float _AlphaThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Adjust outline thickness relative to the sprite's size in the world
                float thickness = _OutlineThickness / _PixelsPerUnit;

                // Sample the texture
                float4 texColor = tex2D(_MainTex, i.uv);
                float maxAlpha = texColor.a;

                texColor.rgb += _AddColor.rgb;

                // Ensure we don't go below zero in any channel
                texColor.rgb = max(texColor.rgb, 0);
                texColor.rgb = min(texColor.rgb, 1);

                // Check if current pixel is part of sprite (any alpha above threshold)
                bool isCurrentPixelSprite = texColor.a > _AlphaThreshold;
                
                // Examining surrounding pixels - treat any pixel with alpha > threshold as sprite
                bool hasNeighborSprite = false;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = thickness * float2(x, y);
                        float neighborAlpha = tex2D(_MainTex, i.uv + offset).a;
                        
                        // If any neighbor has alpha above threshold, mark as having sprite neighbor
                        if (neighborAlpha > _AlphaThreshold)
                        {
                            hasNeighborSprite = true;
                            maxAlpha = max(maxAlpha, 1.0);
                        }
                    }
                }

                // Draw outline if:
                // 1. Current pixel is transparent/empty AND has sprite neighbors (external outline)
                // 2. Current pixel is sprite but has non-sprite neighbors (internal outline edge)
                bool shouldDrawOutline = (!isCurrentPixelSprite && hasNeighborSprite);
                
                float4 outlineColor = _OutlineColor;
                outlineColor.a = shouldDrawOutline ? 1.0 : 0.0;
                
                // If we're drawing outline, use outline color (always solid), otherwise use sprite color with alpha
                float4 finalColor;
                if (shouldDrawOutline)
                {
                    finalColor = outlineColor; // Outline is always solid, not affected by sprite alpha
                }
                else
                {
                    finalColor = texColor * i.color; // Apply sprite renderer's color/alpha only to the sprite
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}