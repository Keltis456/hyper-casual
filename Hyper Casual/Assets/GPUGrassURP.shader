Shader "Custom/GPUGrassURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.3, 0.7, 0.3, 1)
        _WindStrength ("Wind Strength", Float) = 0.2
        _BladeHeight ("Blade Height", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float4> _PositionBuffer;

            float4 _BaseColor;
            float _WindStrength;
            float _BladeHeight;
            float4x4 _LocalToWorld;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float4 color      : COLOR;
            };

            float hash(float n) {
                return frac(sin(n) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float4 blade = _PositionBuffer[IN.instanceID];
                float3 offset = blade.xyz;
                float seed = blade.w;

                // Wind effect
                float wind = sin(_Time.y * 2.0 + offset.x * 0.2 + offset.z * 0.3) * _WindStrength;
                float3 windOffset = float3(wind * IN.positionOS.y, 0, 0);

                // Scale grass height
                float scale = lerp(0.8, 1.2, hash(seed * 13.13));
                float3 localBlade = IN.positionOS + windOffset;
                localBlade.y *= _BladeHeight * scale;

                float3 localPos = blade.xyz + localBlade;
                float3 posWS = mul(_LocalToWorld, float4(localPos, 1.0)).xyz;

                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.normalWS = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, IN.normalOS));
                OUT.color = float4(_BaseColor.rgb, 1.0);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return IN.color;
            }
            ENDHLSL
        }
    }
}