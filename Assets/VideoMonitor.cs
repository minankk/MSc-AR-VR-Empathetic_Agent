using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.UI;


[System.Serializable]
public class MyStringEvent : UnityEvent<string> {
}

[System.Serializable]
public class MyTexEvent : UnityEvent<Texture> {
}



public class VideoMonitor : MonoBehaviour {
    // Start is called before the first frame update

    //--public VideoPlayer player;
    //--    public VideoClip[] clips;
    //--    public static MyStringEvent playVideoEvent;
    public static MyTexEvent setTextureEvent;
    RawImage img;


    void Start() {
        //if (playVideoEvent == null) {
        //    playVideoEvent = new MyStringEvent();
        //}

        if (setTextureEvent == null) {
            setTextureEvent = new MyTexEvent();
        }

//        playVideoEvent.AddListener(playVideo);
        setTextureEvent.AddListener(setVideoTexture);
        img = GetComponent<RawImage>();

    }

    // Update is called once per frame
    void Update() {

    }

    public void setVideoTexture(Texture tex){
        Debug.Log("VideoMonitor::setTexture");
        img.texture = tex;
    }


    //private void playVideo(string str) {
    //    Debug.Log("VideoMonitor::playVideo" + str);
    //    //foreach (VideoClip vc in clips){
    //    //    if (vc.name == str) {
    //    //        player.clip = vc;
    //    //        player.Play();
    //    //        return;
    //    //    }
    //    //}
    //}
}