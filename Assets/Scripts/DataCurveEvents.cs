using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

public class DataCurveEvents : MonoBehaviour
{
    [System.Serializable]
    public class VideoCSVPair
    {
        public VideoClip videoClip;
        public TextAsset csvFile;
        public string emotionName;
    }

    [Header("Video and CSV Pairs")]
    public VideoCSVPair[] videoCSVPairs;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public float delayBeforeStart = 1.0f;
    public float pauseBetweenVideos = 2.0f;
    public float intensityMultiplier = 1.5f; // scale emotion strength globally

    private FaceController faceController;
    private List<EmotionEvent> currentEvents = new List<EmotionEvent>();
    private bool isPlaying = false;
    private Coroutine videoSequenceCoroutine;

    private class EmotionEvent
    {
        public float time;
        public string emotion;
        public float intensity;
        public float duration;
    }

    void Start()
    {
        FindFaceController();

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartVideoSequence);
        }

        UpdateStatus("Ready to start");
    }

    void FindFaceController()
    {
        faceController = GetComponent<FaceController>();
        if (faceController == null)
            faceController = GetComponentInChildren<FaceController>();
        if (faceController == null)
            faceController = GetComponentInParent<FaceController>();
        if (faceController == null)
            faceController = FindObjectOfType<FaceController>();

        if (faceController == null)
        {
            Debug.LogError("No FaceController found!");
        }
    }

    public void StartVideoSequence()
    {
        if (!isPlaying && videoCSVPairs != null && videoCSVPairs.Length > 0)
        {
            isPlaying = true;
            startButton.gameObject.SetActive(false);
            videoSequenceCoroutine = StartCoroutine(PlayAllVideosWithExpressions());
        }
    }

    IEnumerator PlayAllVideosWithExpressions()
    {
        UpdateStatus("Starting in " + delayBeforeStart + " seconds...");
        yield return new WaitForSeconds(delayBeforeStart);

        foreach (var pair in videoCSVPairs)
        {
            if (pair.videoClip != null && pair.csvFile != null)
            {
                // Load CSV for this video
                ParseCSV(pair.csvFile.text);

                // Play the video
                videoPlayer.clip = pair.videoClip;
                videoPlayer.Prepare();

                while (!videoPlayer.isPrepared)
                    yield return null;

                videoPlayer.Play();
                UpdateStatus("Playing: " + pair.emotionName);

                // Run emotion driver
                Coroutine expressionCoroutine = StartCoroutine(RunEventsForCurrentVideo());

                // Wait for video to complete
                yield return new WaitForSeconds((float)pair.videoClip.length);

                // Stop expressions
                if (expressionCoroutine != null)
                    StopCoroutine(expressionCoroutine);

                // Reset face
                if (faceController != null)
                    faceController.ClearBoneRotations();

                // Pause between videos (except after last one)
                if (pair != videoCSVPairs[videoCSVPairs.Length - 1])
                {
                    UpdateStatus("Next video in " + pauseBetweenVideos + " seconds...");
                    yield return new WaitForSeconds(pauseBetweenVideos);
                }
            }
        }

        CompleteSequence();
    }

    IEnumerator RunEventsForCurrentVideo()
    {
        double videoStartTime = videoPlayer.time;

        while (videoPlayer.isPlaying)
        {
            double currentTime = videoPlayer.time - videoStartTime;

            foreach (var ev in currentEvents)
            {
                if (currentTime >= ev.time && currentTime <= ev.time + ev.duration)
                {
                    float progress = (float)((currentTime - ev.time) / ev.duration);

                    // Smooth fade-in/out (0→1→0)
                    float weight = Mathf.Sin(progress * Mathf.PI) * ev.intensity * intensityMultiplier;
                    weight = Mathf.Clamp01(weight);

                    if (faceController != null)
                    {
                        faceController.setCategoricalEmotion(ev.emotion, weight);
                    }
                }
            }

            yield return null;
        }
    }

    void ParseCSV(string csvText)
    {
        currentEvents.Clear();
        string[] lines = csvText.Split('\n');

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Split(',');
            if (data.Length < 4) continue;

            float time = float.Parse(data[0], CultureInfo.InvariantCulture) * 100f;
            string emotion = data[1].Trim();
            float intensity = Mathf.Clamp01(float.Parse(data[2], CultureInfo.InvariantCulture));
            float duration = float.Parse(data[3], CultureInfo.InvariantCulture);

            currentEvents.Add(new EmotionEvent {
                time = time,
                emotion = emotion,
                intensity = intensity,
                duration = duration
            });
        }
    }

    void CompleteSequence()
    {
        isPlaying = false;
        startButton.gameObject.SetActive(true);
        UpdateStatus("Sequence complete! Please take off the headset.");
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    // For testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartVideoSequence();
        }
    }
}
