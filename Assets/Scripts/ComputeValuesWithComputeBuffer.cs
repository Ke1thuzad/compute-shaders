using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ComputeValuesWithComputeBuffer : MonoBehaviour
{
    public ComputeShader shader;
    public ComputeBuffer buffer;
    
    public int[] values;
    public int maxLength = 100;

    [SerializeField] int[] result;
    
    int kernelID;

    uint groupSizeX;
    
    void Start() {
        kernelID = shader.FindKernel("CSMain");

        buffer = new ComputeBuffer(maxLength, sizeof(int));
        
        shader.SetBuffer(kernelID, "buffer", buffer);
        
        shader.GetKernelThreadGroupSizes(kernelID, out groupSizeX, out _, out _);

        ComputeValuesViaShader(new InputAction.CallbackContext());
    }

    void Awake() {
        InputSingleton.Instance.Input.UI.Space.performed += ComputeValuesViaShader;
    }

    void ComputeValuesViaShader(InputAction.CallbackContext context) {
        values = new int[maxLength];
        
        for (int i = 0; i < maxLength; i++) {
            values[i] = (int)(Random.value * 25);
        }
        
        if (values.Length > maxLength)
            values = values[..maxLength];
            
        buffer.SetData(values);
        
        shader.Dispatch(kernelID, Mathf.CeilToInt(values.Length / (float) groupSizeX), 1, 1);

        result = new int[values.Length];
        
        buffer.GetData(result);
    }

    void OnDestroy() {
        if (buffer.IsValid())
            buffer.Release();
        
        if (InputSingleton.Instance != null)
            InputSingleton.Instance.Input.UI.Space.performed -= ComputeValuesViaShader;

    }
}
