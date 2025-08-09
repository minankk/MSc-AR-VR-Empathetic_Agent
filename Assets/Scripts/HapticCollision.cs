using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

public class HapticCollision : MonoBehaviour {

    //public InputActionReference action;
    public InputActionReference hapticAction;
    public float _amplitude = 1.0f;
    public float _duration = 0.1f;
    public float _frequency = 0.0f;




    // Start is called before the first frame update
    void Start() {
        hapticAction.action.Enable();
    }

    private void OnTriggerEnter(Collider other) {
//        Debug.Log("HapticCollision::Trigger");
        OpenXRInput.SendHapticImpulse(hapticAction, _amplitude, _frequency, _duration);
    }
}



