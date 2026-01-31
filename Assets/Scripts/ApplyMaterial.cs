using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ApplyMaterial : MonoBehaviour
{
    public ComputeShader shader;
    public Material material;

    public Color solidColor = Color.red;
    public int imageSize = 256;
    
    RenderTexture colorTexture;

    int kernelID;
    uint groupSizeX;
    uint groupSizeY;

    InputSystem_Actions inputSystem;

    void Start() {
        kernelID = shader.FindKernel("CSMain");

        colorTexture = new RenderTexture(imageSize, imageSize, 0);
        colorTexture.enableRandomWrite = true;
        colorTexture.wrapMode = TextureWrapMode.Repeat;

        colorTexture.Create();
        
        shader.SetTexture(kernelID, "Result", colorTexture);
        
        shader.GetKernelThreadGroupSizes(kernelID, out groupSizeX, out groupSizeY, out _);
    }

    void Awake() {
        inputSystem = new InputSystem_Actions();
    }

    void OnEnable() {
        inputSystem.Enable();
    }

    void OnDisable() {
        inputSystem.Disable();
    }

    void Update() {
        // if (inputSystem.UI.Space.WasPressedThisFrame()) {
        // }
        float zoom = 3.0f * Mathf.Pow(0.1f, Time.time * 0.25f);
        Vector2 targetCenter = new Vector2(-0.743643f, 0.131825f);
        
        shader.SetFloat("zoom", zoom);
        shader.SetFloat("worldTime", Time.time);
        shader.SetVector("offset", targetCenter);
        shader.SetVector("solidColor", solidColor);

        shader.Dispatch(kernelID, Mathf.CeilToInt(imageSize / (float)groupSizeX),
            Mathf.CeilToInt(imageSize / (float)groupSizeY), 1);

        material.SetTexture("_BaseMap", colorTexture);
    }
}
