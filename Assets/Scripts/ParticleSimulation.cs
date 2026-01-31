using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParticleSimulation : MonoBehaviour
{
    public ComputeShader shader;
    public Material material;
    public int maxParticles = 100_000;
    
    ComputeBuffer particleBuffer;

    int kernelID;

    uint groupSizeX;

    RenderParams renderParams;

    const float BoundsSize = 20;

    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public Color color;
    }

    const int PARTICLE_SIZE = sizeof(float) * 10;
    
    void Start() {
        kernelID = shader.FindKernel("CSMain");
        if (kernelID < 0)
            throw new EntryPointNotFoundException();

        particleBuffer = new ComputeBuffer(maxParticles, PARTICLE_SIZE);
        
        particleBuffer.SetData(InitParticles());
        
        shader.SetBuffer(kernelID, "particleBuffer", particleBuffer);
        material.SetBuffer("particleBuffer", particleBuffer);
        
        shader.GetKernelThreadGroupSizes(kernelID, out groupSizeX, out _, out _);

        renderParams = new RenderParams(material) {
            camera = Camera.main,
            worldBounds = new Bounds(Vector3.zero, Vector3.one * BoundsSize)
        };
        
        shader.SetFloat("boundsSize", BoundsSize);
    }

    Particle[] InitParticles() {
        Particle[] particles = new Particle[maxParticles];

        for (int i = 0; i < maxParticles; i++) {
            particles[i] = new Particle {
                position = Random.insideUnitSphere * BoundsSize,
                velocity = Random.insideUnitSphere * 5,
                color = Random.ColorHSV()
            };
        }

        return particles;
    }

    void Update() {
        shader.SetFloat("deltaTime", Time.deltaTime);

        int groupAmountX = Mathf.Min(Mathf.CeilToInt(maxParticles / (float)groupSizeX), 65535);
        
        shader.Dispatch(kernelID, groupAmountX, 1, 1);
        
        Graphics.RenderPrimitives(renderParams, MeshTopology.Points, 1, maxParticles);
    }

    void OnDestroy() {
        if (particleBuffer.IsValid())
            particleBuffer.Release();
    }
}