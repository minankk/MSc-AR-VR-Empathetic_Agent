using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SimpleSequenceManager : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer; // Drag your VideoPlayer component here

    [Header("Video Clips")]
    public VideoClip happyVideo;
    public VideoClip sadVideo;
    public VideoClip angryVideo;

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public string sequenceFile = "video_sequence.txt";
    public float delayBeforeStart = 1.0f;
    public float pauseBetweenVideos = 2.0f;

    private string[] videoOrder;
    private bool isPlaying = false;

    void Start()
    {
        // Load the sequence from file
        LoadSequenceFromFile();

        // Setup button
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartSequence);
        }

        UpdateStatus("Ready. Press Start.");
    }

    void Update()
    {
        // Simple PC testing - Space to start
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

            if (sequence.Length == 3)
            {
                videoOrder = new string[3];
                for (int i = 0; i < 3; i++)
                {
                    videoOrder[i] = sequence[i].ToString();
                }
                Debug.Log("Loaded sequence: " + sequence);
                return;
            }
        }

        // Default fallback
        videoOrder = new string[] { "A", "B", "C" };
        Debug.Log("Using default sequence: ABC");
    }

    public void StartSequence()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            startButton.interactable = false;
            StartCoroutine(PlayVideoSequence());
        }
    }

    IEnumerator PlayVideoSequence()
    {
        // Initial delay
        UpdateStatus("Starting in " + delayBeforeStart + " seconds...");
        yield return new WaitForSeconds(delayBeforeStart);

        // Play each video in sequence
        foreach (string videoKey in videoOrder)
        {
            VideoClip clipToPlay = GetVideoClip(videoKey);

            if (clipToPlay != null)
            {
                // Play the video
                videoPlayer.clip = clipToPlay;
                videoPlayer.Play();
                UpdateStatus("Playing: " + GetEmotionName(videoKey));

                // Wait for video to complete
                yield return new WaitForSeconds((float)clipToPlay.length);

                // Pause between videos (except after last one)
                if (videoKey != videoOrder[videoOrder.Length - 1])
                {
                    UpdateStatus("Next video in " + pauseBetweenVideos + " seconds...");
                    yield return new WaitForSeconds(pauseBetweenVideos);
                }
            }
        }

        // Sequence complete
        CompleteSequence();
    }

    VideoClip GetVideoClip(string videoKey)
    {
        switch (videoKey)
        {
            case "A": return happyVideo;
            case "B": return sadVideo;
            case "C": return angryVideo;
            default: return null;
        }
    }

    string GetEmotionName(string videoKey)
    {
        switch (videoKey)
        {
            case "A": return "Happy";
            case "B": return "Sad";
            case "C": return "Angry";
            default: return "Unknown";
        }
    }

    void CompleteSequence()
    {
        isPlaying = false;
        startButton.interactable = true;
        UpdateStatus("Sequence complete! Ready to start again.");
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
