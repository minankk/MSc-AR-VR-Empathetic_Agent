using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExampleClass : MonoBehaviour {
    AudioSource audioSource;


    private void OnTriggerEnter(Collider other) {
        Debug.Log("ExampleClass::Trigger");
        Debug.Log(other.gameObject.name);
    }



}