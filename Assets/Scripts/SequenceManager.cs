using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SimpleSequenceManager : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("Video Clips")]
    public VideoClip happyVideo;
    public VideoClip sadVideo;
    public VideoClip surprisedVideo;

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public string sequenceFile = "video_sequence.txt";   // order of A/B/C
    public float delayBeforeStart = 1.0f;
    public float pauseBetweenVideos = 2.0f;

    [Header("Condition Settings")]
    [Tooltip("Set this to C1 or C2 depending on the scene")]
    public string conditionSuffix = "C1";

    private string[] videoOrder;
    private bool isPlaying = false;
    private Coroutine videoSequenceCoroutine;
    private int currentVideoIndex = 0;

    // Lookup for CSV paths
    private Dictionary<string, string> videoToCSV;

    void Start()
    {
        BuildCSVMapping();
        LoadSequenceFromFile();
        //FindStartButton(); -> Add start as call back to the button
        UpdateStatus("VR experience ready. Press play to start");
    }

    void BuildCSVMapping()
    {
        videoToCSV = new Dictionary<string, string>()
        {
            { "A", Path.Combine(Application.dataPath, $"DataFiles/Happy_{conditionSuffix}.csv") },
            { "B", Path.Combine(Application.dataPath, $"DataFiles/Sad_{conditionSuffix}.csv") },
            { "C", Path.Combine(Application.dataPath, $"DataFiles/Surprise_{conditionSuffix}.csv") }
        };
    }

    void FindStartButton()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartSequence);
            return;
        }

        string[] possiblePaths = {
            "StartButton",
            "ControlCanvas/StartButton",
            "UI/StartButton",
            "Canvas/StartButton"
        };

        foreach (string path in possiblePaths)
        {
            GameObject buttonObj = GameObject.Find(path);
            if (buttonObj != null)
            {
                startButton = buttonObj.GetComponent<Button>();
                if (startButton != null)
                {
                    startButton.onClick.AddListener(StartSequence);
                    return;
                }
            }
        }

        Debug.LogError("Could not find the StartButton in the scene!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartSequence();
        }
    }

    void LoadSequenceFromFile()
    {
        string filePath = Path.Combine(Application.dataPath, sequenceFile);

        if (File.Exists(filePath))
        {
            string sequence = File.ReadAllText(filePath).Trim().ToUpper();

            videoOrder = new string[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                videoOrder[i] = sequence[i].ToString();
            }
        }
        else
        {
            videoOrder = new string[] { "A", "B", "C" };
        }
    }

    public void StartSequence()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            currentVideoIndex = 0;
            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
            videoSequenceCoroutine = StartCoroutine(PlayVideoSequence());
        }
    }

    IEnumerator PlayVideoSequence()
    {
        UpdateStatus("Starting in " + delayBeforeStart + " seconds...");
        yield return new WaitForSeconds(delayBeforeStart);

        for (currentVideoIndex = 0; currentVideoIndex < videoOrder.Length; currentVideoIndex++)
        {
            string videoKey = videoOrder[currentVideoIndex];
            VideoClip clipToPlay = GetVideoClip(videoKey);
            string csvPath = GetCSVFile(videoKey);

            if (clipToPlay != null)
            {
                // Load facial expression data for this video
                if (File.Exists(csvPath))
                {
                    string csvContent = File.ReadAllText(csvPath);
                    Debug.Log($"Loaded CSV for {videoKey}: {csvPath}");
                    // TODO: feed csvContent into face expression controller
                }
                else
                {
                    Debug.LogWarning($"CSV file not found: {csvPath}");
                }

                // Play video
                videoPlayer.clip = clipToPlay;
                videoPlayer.Prepare();
                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }

                videoPlayer.Play();
                UpdateStatus($"Playing video {currentVideoIndex + 1}/{videoOrder.Length}");

                float videoTimer = 0f;
                while (videoTimer < clipToPlay.length && videoPlayer.isPlaying)
                {
                    videoTimer += Time.deltaTime;
                    yield return null;
                }

                if (currentVideoIndex < videoOrder.Length - 1)
                {
                    UpdateStatus($"Next video in {pauseBetweenVideos} seconds...");
                    yield return new WaitForSeconds(pauseBetweenVideos);
                }
            }
        }

        CompleteSequence();
    }

    VideoClip GetVideoClip(string videoKey)
    {
        switch (videoKey)
        {
            case "A": return happyVideo;
            case "B": return sadVideo;
            case "C": return surprisedVideo;
            default: return null;
        }
    }

    string GetCSVFile(string videoKey)
    {
        if (videoToCSV.ContainsKey(videoKey))
        {
            return videoToCSV[videoKey];
        }
        return null;
    }

    void CompleteSequence()
    {
        isPlaying = false;
        UpdateStatus("Sequence complete! Please take off the headset and answer the questionnaire.");
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
}
