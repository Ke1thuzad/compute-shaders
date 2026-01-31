Shader "Custom/Particles"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 2
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

            struct Particle
            {
                float3 position;
                float3 velocity;
                float4 color;
            };

            StructuredBuffer<Particle> particleBuffer;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                half4 color : TEXCOORD0;
                float size : PSIZE;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
            float _PointSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                Particle particle = particleBuffer[IN.instanceID];
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz + particle.position);
                OUT.color = particle.color;

                OUT.size = _PointSize;
                
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
