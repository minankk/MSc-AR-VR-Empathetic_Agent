using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;

public class DataCurveEvents : MonoBehaviour
{

    [SerializeField] TextAsset dataFileAsset;
    [SerializeField] TextAsset eventFileAsset;
    [SerializeField] GameObject go;
    [SerializeField] ParticleSystem ps;

    private Animator animator;
    public float Value01;

    public void Start()
    {
        animator = GetComponent<Animator>();

        Value01 = 100f;
        SetupEvents();
        SetupCurve();

        StartDataPlayback();
    }

    private void SetupEvents()
    {

        AnimationClip animationClip = animator.runtimeAnimatorController.animationClips[0];
        if (animationClip == null)
        {
            Debug.LogError("ERROR: need to add an animation to Animator");
        }
        else
        {
            Debug.Log("clip.name: " + animationClip.name);
        }

        animator.speed = 0;

        Debug.Assert(eventFileAsset != null);
        var dataLines = eventFileAsset.text.Split('\n'); // Split also works with simple arguments, no need to pass char[]
        for (int i = 1; i < dataLines.Length; i++)
        { // assuming we have a header
            if (dataLines[i].Length > 0)
            {
                var data = dataLines[i].Split(',');
                int iTime = Convert.ToInt16(data[0]);
                string strEvent = data[1];
                int iValue = Convert.ToInt16(data[2]);
                AnimationEvent animationEvent = new AnimationEvent();
                // put some parameters on the AnimationEvent
                //  - call the function called PrintEvent()
                //  - the animation on this object lasts 2 seconds
                //    and the new animation created here is
                //    set up to happen 1.3s into the animation
                animationEvent.intParameter = iValue;
                animationEvent.time = iTime;
                animationEvent.functionName = strEvent;
                animationClip.AddEvent(animationEvent);
            }
        }
    }

    private void Update()
    {

        if (animator.speed > 0)
        {
            go.transform.localScale = new Vector3(Value01 / 100f, Value01 / 100f, Value01 / 100f);
        }
    }


    public void StartDataPlayback()
    {
        Debug.Log("StartAnimationCurve");
        animator.speed = 1;
    }

    private void SetupCurve()
    {
        // get the animation clip and add the AnimationEvent
        AnimationClip animationClip = animator.runtimeAnimatorController.animationClips[0];
        if (animationClip == null)
        {
            Debug.LogError("ERROR: need to add an animation to Animator");
        }
        else
        {
            Debug.Log("clip.name: " + animationClip.name);
        }

        AnimationCurve animationCurve = new AnimationCurve();

        Debug.Assert(dataFileAsset != null);
        var dataLines = dataFileAsset.text.Split('\n'); // Split also works with simple arguments, no need to pass char[]
        int iTime = 0;
        float fValue = 0;
        for (int i = 1; i < dataLines.Length; i++)
        { // assuming we have a header
            if (dataLines[i].Length > 0)
            {
                var data = dataLines[i].Split(',');
                //iTime = Convert.ToInt16(data[0]);
                string trimmedData = data[i].Trim();
                iTime = Convert.ToInt16(trimmedData);
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                fValue = Convert.ToSingle(data[1], culture);

                //fValue = Convert.ToSingle(data[1]);
                animationCurve.AddKey(iTime, fValue);
                //                Debug.Log("iEndTime: " + iEndTime + " Value: " + fValue);
            }
        }

        animationClip.SetCurve("", typeof(DataCurveEvents), "Value01", animationCurve);
    }


    // the function to be called as an event
    public void TiggerEvent1(int i)
    {
        Debug.Log("TiggerEvent1: " + i + " called at: " + Time.time);
        ps.startColor = new Color(1f, 0f, 0f);
        ps.Play();
    }

    // the function to be called as an event
    public void TiggerEvent2(int i)
    {
        Debug.Log("TiggerEvent2: " + i + " called at: " + Time.time);
        ps.startColor = new Color(0f, 1f, 0f);
        ps.Play();
    }

    // the function to be called as an event
    public void TiggerEvent3(int i)
    {
        Debug.Log("TiggerEvent3: " + i + " called at: " + Time.time);
        ps.startColor = new Color(0f, 0f, 1f);
        ps.Play();
    }

}




