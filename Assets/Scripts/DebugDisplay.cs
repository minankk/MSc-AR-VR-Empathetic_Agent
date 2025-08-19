using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour {
    Transform cam;
    public TMP_Text tmpText;
    public Image imgDebug;

    public static DebugDisplay Instance = null;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        // why is this here? DontDestroyOnLoad(gameObject);
    }


    protected void Start() {
        cam = Camera.main.transform;
    }


    void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
        transform.position = cam.forward * 150f;
    }


    public void AddText(string str) {
        string strOld = tmpText.text;
        tmpText.text = strOld + "\n" + str;

    }

    public void ShowText(string str) {
        tmpText.text = str;
    }


    public void ShowStateOn() { imgDebug.GetComponent<Image>().color = new Color32(0, 255, 0, 100); }
    public void ShowStateOff() { imgDebug.GetComponent<Image>().color = new Color32(255, 0, 0, 100); }

}