using System;
using UnityEngine;

public class InputSingleton : MonoBehaviour
{
    public static InputSingleton Instance;
    
    public InputSystem_Actions Input { get; private set; }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);

        Input = new InputSystem_Actions();
    }

    void OnEnable() {
        Input?.Enable();
    }

    void OnDisable() {
        Input?.Disable();
    }
}