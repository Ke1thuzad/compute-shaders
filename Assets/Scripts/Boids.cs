using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boids : MonoBehaviour
{
    static readonly int OldBoidBuffer = Shader.PropertyToID("oldBoidBuffer");
    static readonly int NewBoidBuffer = Shader.PropertyToID("newBoidBuffer");
    static readonly int DT = Shader.PropertyToID("dt");
    
    public ComputeShader shader;
    public Material material;
    public Mesh boidMesh;

    public int maxBoids = 2;
    public float neighbourRadius = 2;
    public float steerSpeed = 2f;
    public float maxVelocityMagnitude = 20f;
    public float minVelocityMagnitude = 0.5f;
    public float fovAngle = -0.8f;

    public float boundsSize = 10;

    int kernelID;
    uint groupSizeX;

    ComputeBuffer oldBoidBuffer;
    ComputeBuffer newBoidBuffer;

    RenderParams renderParams;

    struct Boid
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 separation;
        public Vector3 alignment;
        public Vector3 cohesion;

        public Boid(Vector3 position, Vector3 velocity) {
            this.position = position;
            this.velocity = velocity;
            separation = Vector3.zero;
            alignment = Vector3.zero;
            cohesion = Vector3.zero;
        }

        public static int Size() => sizeof(float) * 3 * 5;
    }

    Boid[] boids;
    
    void Start() {
        boids = new Boid[maxBoids];
        for (int i = 0; i < maxBoids; ++i) {
            boids[i] = new Boid(Vector3.zero, Random.insideUnitSphere * 200);
        }

        oldBoidBuffer = new ComputeBuffer(maxBoids, Boid.Size());
        newBoidBuffer = new ComputeBuffer(maxBoids, Boid.Size());

        kernelID = shader.FindKernel("CSMain");
        if (kernelID < 0)
            throw new UnityException();
        
        oldBoidBuffer.SetData(boids);
        
        shader.SetBuffer(kernelID, OldBoidBuffer, oldBoidBuffer);
        shader.SetBuffer(kernelID, NewBoidBuffer, newBoidBuffer);
        material.SetBuffer(NewBoidBuffer, newBoidBuffer);
        
        shader.SetInt("maxBoids", maxBoids);
        shader.SetFloat("neighbourRadius", neighbourRadius);
        shader.SetFloat("steerSpeed", steerSpeed);
        shader.SetFloat("maxVelocityMagnitude", maxVelocityMagnitude);
        shader.SetFloat("minVelocityMagnitude", minVelocityMagnitude);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetFloat("fovAngle", fovAngle);
        
        shader.GetKernelThreadGroupSizes(kernelID, out groupSizeX, out _, out _);

        renderParams = new RenderParams(material) {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * boundsSize)
        };
    }

    void Update() {
        shader.SetFloat(DT, Time.deltaTime);
        
        shader.Dispatch(kernelID, Mathf.CeilToInt(maxBoids / (float)groupSizeX), 1, 1);
        
        Graphics.RenderMeshPrimitives(renderParams, boidMesh, 0, maxBoids);

        (oldBoidBuffer, newBoidBuffer) = (newBoidBuffer, oldBoidBuffer);
        
        shader.SetBuffer(kernelID, OldBoidBuffer, oldBoidBuffer);
        shader.SetBuffer(kernelID, NewBoidBuffer, newBoidBuffer);
        material.SetBuffer(NewBoidBuffer, oldBoidBuffer);
    }

    void OnDestroy() {
        if (oldBoidBuffer.IsValid())
            oldBoidBuffer.Release();
        
        if (newBoidBuffer.IsValid())
            newBoidBuffer.Release();
    }
}
