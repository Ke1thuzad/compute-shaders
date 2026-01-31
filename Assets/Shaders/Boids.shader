Shader "Custom/Boids"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                uint id : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            struct Boid
            {
                float3 position;
                float3 velocity;
                float3 separation;
                float3 alignment;
                float3 cohesion;
            };

            StructuredBuffer<Boid> newBoidBuffer;


            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 rotatedPos = IN.positionOS.xyz;

                Boid boid = newBoidBuffer[IN.id];

                if (length(boid.velocity) > HALF_EPS)
                {
                    float3 forward = normalize(boid.velocity);
                    float3 up = float3(0, 1, 0);
                    
                    if (abs(dot(forward, up)) > 0.99f)
                        up = float3(1, 0, 0);
                    
                    float3 right = normalize(cross(up, forward));

                    up = normalize(cross(forward, right));

                    float3x3 lookAt = float3x3(right, forward, up);

                    rotatedPos = mul(rotatedPos, lookAt);
                }
                
                OUT.positionHCS = TransformObjectToHClip(rotatedPos + boid.position);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return float4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
}
