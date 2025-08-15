using UnityEngine;
using System;

public class DataCurveEvents : MonoBehaviour
{
    [SerializeField] TextAsset eventFileAsset; // Assign Happy_C1.csv here
    private FaceController faceController;
    private float startTime;

    void Start()
    {
        faceController = GetComponent<FaceController>();
        if (faceController == null)
        {
            Debug.LogError("FaceController component missing!");
            return;
        }

        if (eventFileAsset == null)
        {
            Debug.LogError("No event file assigned!");
            return;
        }

        startTime = Time.time;
        ParseAndScheduleEvents();
    }

    void ParseAndScheduleEvents()
    {
        var dataLines = eventFileAsset.text.Split('\n');

        // Skip header row (i=0)
        for (int i = 1; i < dataLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(dataLines[i])) continue;

            var data = dataLines[i].Split(',');
            if (data.Length < 4) continue;

            float time = float.Parse(data[0]);
            string emotion = data[1].Trim();
            float intensity = float.Parse(data[2]);
            float duration = float.Parse(data[3]);

            StartCoroutine(TriggerExpression(time, emotion, intensity, duration));
        }
    }

    System.Collections.IEnumerator TriggerExpression(float triggerTime, string emotion, float intensity, float duration)
    {
        yield return new WaitForSeconds(triggerTime);
        faceController.setCategoricalEmotion(emotion, intensity, 0.3f, duration, 0.3f);
    }
}
