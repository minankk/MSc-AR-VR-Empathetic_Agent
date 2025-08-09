using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.UI;

public class VideoPlaybackTrigger : MonoBehaviour
{
    public VideoPlayer player;

    public static UnityEvent newVideoPlay;
    RawImage img;
    Material origMat;
    public AudioSource audioSource;
    //   public DataAnimatedCurve myClassDataAnimatedCurve;
    //   public FaceController myClassFaceController;

    private void Awake()
    {
        //newVideoPlay = new UnityEvent();

        if (newVideoPlay == null)
        {
            newVideoPlay = new UnityEvent();
        }

        newVideoPlay.AddListener(StopAllVideos);

        img = GetComponent<RawImage>();
        origMat = img.material;
        audioSource=GetComponent<AudioSource>();
        if(audioSource==null)
        {
            audioSource=gameObject.AddComponent<AudioSource>();
        }
        player.audioOutputMode = VideoAudioOutputMode.AudioSource;
        player.SetTargetAudioSource(0, audioSource);
    }

    //public void Awake() {
    //    player = GetComponent<VideoPlayer>(); 
    //}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("VideoPlaybackTrigger::Trigger");
        if (player.isPlaying)
        {
            player.Pause();
            img.material = origMat;
        }
        else
        {
            //gameObject.BroadcastMessage("StopAllVideos");
            //Material mat = new Material();// Instantiate(img.material);
            img.material = null;
            VideoPlaybackTrigger.newVideoPlay.Invoke();
            //myClassInstance = GetComponent<DataAnimatedCurve>();
            //VideoMonitor.playVideoEvent .Invoke(img.texture.name);
            VideoMonitor.setTextureEvent.Invoke(img.texture);
            //   myClassDataAnimatedCurve = new DataAnimatedCurve();
            //   myClassFaceController = new FaceController();
            //   Debug.Log("origMat:", origMat);
            //   Debug.Log("other:" + img.mainTexture);
            //   string value = img.mainTexture.ToString();
            //   string actname = "";
            //   if (value.ToString().Contains("VideoRenderTexture 1"))
            //   {
            //       actname = "Anger";
            //   }
            //   else if (value.ToString().Contains("VideoRenderTexture 2"))
            //   {
            //       actname = "Contempt";
            //   }
            //   else if (value.ToString().Contains("VideoRenderTexture 3"))
            //   {
            //       actname = "Fear";
            //   }
            //   else if (value.ToString().Contains("VideoRenderTexture 4"))
            //   {
            //       actname = "Happiness";
            //   }
            //   else if (value.ToString().Contains("VideoRenderTexture 5"))
            //   {
            //       actname = "Sadness";
            //   }
            //   else if (value.ToString().Contains("VideoRenderTexture 6"))
            //   {
            //       actname = "Surprise";
            //   }
            //   Debug.Log("actname:" + actname);
            player.Play();
            //myClassDataAnimatedCurve.Start();


        }
    }

    public void StopAllVideos()
    {
        //        Debug.Log("StopAllVideos");

        if (player.isPlaying)
        {
            player.Pause();
            //          Debug.Log("STOPPING: " + player.clip.name);
            img.material = origMat;

        }


    }



}
