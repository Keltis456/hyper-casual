Shader "Custom/GPUGrassURP"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.3, 0.7, 0.3, 1)
        _BladeHeight("Blade Height", Float) = 1.0
        _ScaleRange("Scale Variation", Float) = 0.3
        _ColorVariation("Color Variation", Color) = (0.05, 0.05, 0.05, 0)
        _WindStrength("Wind Strength", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            Name "GrassPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            

            float4 _BaseColor;
            float4 _ColorVariation;
            float _BladeHeight;
            float _ScaleRange;
            float _WindStrength;

            struct GrassBlade {
                float3 position;
                float seed;
                float cut;
                uint chunkIndex;
                float rotation;
                float3 padding; // padding for 16-byte alignment
            };

            struct Attributes {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float4 color : COLOR;
            };
            StructuredBuffer<GrassBlade> _BladeBuffer;
            StructuredBuffer<float4x4> _ChunkToWorldBuffer;
            float hash(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                GrassBlade blade = _BladeBuffer[IN.instanceID];

                if (blade.cut > 0.99)
                {
                    OUT.positionCS = float4(10e5, 10e5, 10e5, 1);
                    OUT.color = float4(0, 0, 0, 0);
                    return OUT;
                }

                float seed = blade.seed;

                float scale = 1.0 + _ScaleRange * (hash(seed * 17.77) * 2.0 - 1.0);
                float3 tint = _BaseColor.rgb + _ColorVariation.rgb * (hash(seed * 31.3) * 2.0 - 1.0);
                tint = saturate(tint);

                float wind = sin(_Time.y * 2.0 + blade.position.x * 0.2 + blade.position.z * 0.3) * _WindStrength;
                float3 windOffset = float3(wind * IN.positionOS.y, 0, 0);

                float3 pos = IN.positionOS + windOffset;

                // Apply rotation
                float s = sin(blade.rotation);
                float c = cos(blade.rotation);
                float3 rotated;
                rotated.x = pos.x * c - pos.z * s;
                rotated.z = pos.x * s + pos.z * c;
                rotated.y = pos.y;

                rotated.y *= _BladeHeight * scale;

                float3 localBlade = blade.position + rotated;

                float3 worldPos = mul(_ChunkToWorldBuffer[blade.chunkIndex], float4(localBlade, 1.0)).xyz;

                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.color = float4(tint, 1.0);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(IN.color.rgb, 1);
            }
            ENDHLSL
        }
    }
}