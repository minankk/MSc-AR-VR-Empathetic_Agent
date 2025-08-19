using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class DataCurveEvents : MonoBehaviour
{
    [SerializeField] private TextAsset eventFileAsset;
    private FaceController faceController;

    private class EmotionEvent
    {
        public float time;
        public string emotion;
        public float intensity;
        public float duration;
    }

    private List<EmotionEvent> events = new();

    void Start()
    {
        TryGetComponent(out faceController);
        if (faceController == null)
        {
            Debug.LogError("FaceController missing!");
            return;
        }
        if (eventFileAsset == null)
        {
            Debug.LogError("No event file assigned!");
            return;
        }

        ParseCSV(eventFileAsset.text);
        StartCoroutine(RunEvents());
    }

    void ParseCSV(string csvText)
    {
        events.Clear();
        string[] lines = csvText.Split('\n');

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Split(',');
            if (data.Length < 4) continue;

            float time = float.Parse(data[0], CultureInfo.InvariantCulture);
            string emotion = data[1].Trim();
            float intensity = Mathf.Clamp01(float.Parse(data[2], CultureInfo.InvariantCulture));
            float duration = float.Parse(data[3], CultureInfo.InvariantCulture);

            events.Add(new EmotionEvent { time = time, emotion = emotion, intensity = intensity, duration = duration });
        }
    }

    IEnumerator RunEvents()
    {
        float startTime = Time.time;

        foreach (var ev in events)
        {
            float waitTime = (startTime + ev.time) - Time.time;
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            // Smooth trigger for fade in and out
            faceController.setCategoricalEmotion(
                ev.emotion,
                ev.intensity,
                0.5f,     // fade in smoother
                ev.duration,
                0.5f      // fadeout smoother
            );
        }
    }
}
