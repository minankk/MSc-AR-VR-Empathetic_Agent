using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics.CodeAnalysis;

public class DataAnimatedCurve : MonoBehaviour
{
    [SerializeField] AnimationCurve animationCurve;
    //[SerializeField] GameObject character;
    [SerializeField] GameObject go;
    [SerializeField] TextAsset dataFileAsset;

    public float curveDeltaTime;
    public int iEndTime;
    public string actionName_ = "";
    public TextAsset DataFileAsset { get => dataFileAsset; set => dataFileAsset = value; }
    public GameObject Go { get => go; set => go = value; }

    //private FaceController FC;
    public FaceController myClassInstance;

    public void Start()
    {
        //gameObject.AddComponent<DataAnimatedCurve>();
        //myClassInstance = new FaceController();
        myClassInstance = GameObject.FindAnyObjectByType<FaceController>();
        curveDeltaTime = 0f;
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        StartCoroutine(RunAnimationCurve());
    }

    public void actionName(string name)
    {

        //Debug.Log("ActionNameDetected:" + name);
        actionName_ = name;
        Debug.Log("ActionNameDetected:" + actionName_);

        //Start();
        //if (actionName_ == "Anger")
        //{
        //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
        //    StartCoroutine(RunAnimationCurve());
        //}
    }

    //    Start();
    //    Debug.Log("ActionNameDetected:" + name);
    //    //myClassInstance = new FaceController();
    //    //curveDeltaTime = 0f;
    //    //myClassInstance.ClearBoneRotations();
    //    ////myClassInstance = GetComponent<FaceController>();
    //    ////curveDeltaTime = 0f;
    //    //if (name == "Anger")
    //    //{
    //    //    Debug.Log("ActionStaRTED:" + name);
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Happiness")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Sadness")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Surprise")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Fear")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Disgust")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //    //else if (name == "Contempt")
    //    //{
    //    //    ProcessDataCoroutine("C:/Users/220262172/Unity project/Indoor-VR-URP-Template-Kanni/Assets/Scripts/Anger_Contempt.csv");
    //    //    StartCoroutine(RunAnimationCurve());
    //    //}
    //}
    //}
    public void ProcessDataCoroutine(string filePath)
    {
        using (StreamReader sr = File.OpenText(filePath))
        {
            string s;
            int i = 0;
            while ((s = sr.ReadLine()) != null)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                ProcessLine(s);


                // Process the line here
                // Wait for a specified time before processing the next line
                //yield return new WaitForSeconds(1.0f); // Adjust the time as needed
            }
        }


    }
    public void ProcessLine(string line)
    {
        //myClassInstance = GetComponent<FaceController>();
        Debug.Log("Processing line: " + line);
        // myClassInstance = new FaceController();
        myClassInstance = GameObject.FindAnyObjectByType<FaceController>();
        myClassInstance.ClearBoneRotations();
        //Console.WriteLine(line);
        var data = line.Split(',');

        int iTime = Convert.ToInt16(data[0]);
        float[] values = {
                        Convert.ToSingle(data[1]),
                        Convert.ToSingle(data[2]),
                        Convert.ToSingle(data[3]),
                        Convert.ToSingle(data[4]),
                        Convert.ToSingle(data[5]),
                        Convert.ToSingle(data[6]),
                        Convert.ToSingle(data[7])
                    };

        float maxValue = values[0];
        int maxIndex = 1; // Start with index 1 assuming data[1] has the first value

        // Loop through the values starting from the second element
        for (int j = 1; j < values.Length; j++)
        {
            if (values[j] > maxValue)
            {
                maxValue = values[j];
                maxIndex = j + 1; // Add 1 to match data[x] indexing
            }
        }
        string columnName = "";
        if (maxIndex == 1)
            columnName = "Happiness";
        else if (maxIndex == 2)
            columnName = "Sadness";
        else if (maxIndex == 3)
            columnName = "Surprise";
        else if (maxIndex == 4)
            columnName = "Fear";
        else if (maxIndex == 5)
            columnName = "Anger";
        else if (maxIndex == 6)
            columnName = "Disgust";
        else if (maxIndex == 7)
            columnName = "Contempt";

        if (columnName == "Happiness")
        {
            maxValue = 10 + maxValue;
        }
        else if (columnName == "Sadness")
        {
            maxValue = 11 + maxValue;
        }
        else if (columnName == "Surprise")
        {
            maxValue = 12 + maxValue;
        }
        else if (columnName == "Fear")
        {
            maxValue = 13 + maxValue;
        }
        else if (columnName == "Anger")
        {
            maxValue = 14 + maxValue;
        }
        else if (columnName == "Disgust")
        {
            maxValue = 15 + maxValue;
        }
        else if (columnName == "Contempt")
        {
            maxValue = 16 + maxValue;
        }


        Debug.Log("Max Value" + maxValue);
        Debug.Log("iTime" + iTime);
        //animationCurve = new AnimationCurve();
        animationCurve.AddKey(iTime, maxValue);
        iEndTime = iTime;
    }

    public IEnumerator RunAnimationCurve()
    {
        Debug.Log("RunAnimationCurve");


        while (curveDeltaTime < iEndTime)
        {
            curveDeltaTime += Time.deltaTime;
            float fValue = animationCurve.Evaluate(curveDeltaTime);
            Debug.Log("TIME " + curveDeltaTime + " value: " + fValue);
            //fValue /= 100f;
            //TEMP FC.setCategoricalEmotion("Anger", fValue);
            //-----------------------------------------------------------------------
            string columnName = "";
            if (fValue >= 10 && fValue <= 10.9)
            {
                columnName = "Happiness";
                fValue -= 10;
            }
            else if (fValue >= 11 && fValue <= 11.9)
            {
                columnName = "Sadness";
                fValue -= 11;
            }
            else if (fValue >= 12 && fValue <= 12.9)
            {
                columnName = "Surprise";
                fValue -= 12;
            }
            else if (fValue >= 13 && fValue <= 13.9)
            {
                columnName = "Fear";
                fValue -= 13;
            }
            else if (fValue >= 14 && fValue <= 14.9)
            {
                columnName = "Anger";
                fValue -= 14;
            }
            else if (fValue >= 15 && fValue <= 15.9)
            {
                columnName = "Disgust";
                fValue -= 15;
            }
            else if (fValue >= 16 && fValue <= 16.9)
            {
                columnName = "Contempt";
                fValue -= 16;
            }
            Debug.Log("emotion: " + columnName + ", value: " + fValue);
            myClassInstance.setCategoricalEmotion(columnName, fValue);
            yield return null;
        }
        yield return null;
    }


}