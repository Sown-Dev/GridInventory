Shader "Custom/UniformOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _OutlineThickness ("Outline Thickness", Float) = 0.0 // Default to 0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.01
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "CanUseSpriteAtlas"="True" 
        }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "OUTLINE"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Required for ddx/ddy support
            #pragma target 3.0 

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
                float4 texColor = tex2D(_MainTex, i.uv);
                float maxAlpha = texColor.a;

                texColor.rgb += _AddColor.rgb;
                texColor.rgb = saturate(texColor.rgb);

                bool isCurrentPixelSprite = texColor.a > _AlphaThreshold;
                bool hasNeighborSprite = false;
                
                // Get the UV change per 1 screen pixel
                float2 dx = ddx(i.uv) * _OutlineThickness;
                float2 dy = ddy(i.uv) * _OutlineThickness;

                float2 offsets[8] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0),                float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };

                for (int j = 0; j < 8; j++)
                {
                    float2 uvOffset = dx * offsets[j].x + dy * offsets[j].y;
                    float neighborAlpha = tex2D(_MainTex, i.uv + uvOffset).a;
                    
                    if (neighborAlpha > _AlphaThreshold)
                    {
                        hasNeighborSprite = true;
                        maxAlpha = max(maxAlpha, 1.0);
                    }
                }

                // Only draw outline if thickness is greater than 0
                bool shouldDrawOutline = (!isCurrentPixelSprite && hasNeighborSprite) && (_OutlineThickness > 0.0);
                
                float4 finalColor;
                if (shouldDrawOutline)
                {
                    finalColor = _OutlineColor;
                }
                else
                {
                    finalColor = texColor * i.color;
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}