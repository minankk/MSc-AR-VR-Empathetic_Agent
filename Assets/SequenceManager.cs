using System.Collections;
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
    public VideoClip angryVideo;

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public string sequenceFile = "video_sequence.txt";
    public float delayBeforeStart = 1.0f;
    public float pauseBetweenVideos = 2.0f;

    [Header("Testing Shortcuts")]
    public KeyCode skipVideoKey = KeyCode.N;
    public KeyCode skipAllKey = KeyCode.M;

    private string[] videoOrder;
    private bool isPlaying = false;
    private Coroutine videoSequenceCoroutine;
    private int currentVideoIndex = 0;

    void Start()
    {
        LoadSequenceFromFile();

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartSequence);
        }

        UpdateStatus("VR experience ready. Press Start.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isPlaying)
        {
            StartSequence();
        }

        if (Input.GetKeyDown(skipVideoKey) && isPlaying)
        {
            SkipCurrentVideo();
        }

        if (Input.GetKeyDown(skipAllKey) && isPlaying)
        {
            SkipAllVideos();
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
                return;
            }
        }

        videoOrder = new string[] { "A", "B", "C" };
    }

    public void StartSequence()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            currentVideoIndex = 0;
            startButton.gameObject.SetActive(false);
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

            if (clipToPlay != null)
            {
                // Stop any currently playing video first
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                    yield return null; // Wait one frame
                }

                // Setup and play the new video
                videoPlayer.clip = clipToPlay;
                videoPlayer.Prepare();

                // Wait for video to be ready
                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }

                videoPlayer.Play();
                UpdateStatus("Playing: " + GetEmotionName(videoKey) + " (" + (currentVideoIndex + 1) + "/3) ");

                // Wait for video to complete
                float videoTimer = 0f;
                while (videoTimer < clipToPlay.length && videoPlayer.isPlaying)
                {
                    videoTimer += Time.deltaTime;
                    yield return null;
                }

                // Pause between videos (except after last one)
                if (currentVideoIndex < videoOrder.Length - 1)
                {
                    UpdateStatus("Next video in " + pauseBetweenVideos + " seconds... ");

                    float pauseTimer = 0f;
                    while (pauseTimer < pauseBetweenVideos)
                    {
                        pauseTimer += Time.deltaTime;
                        yield return null;
                    }
                }
            }
        }

        CompleteSequence();
    }

    void SkipCurrentVideo()
    {
        if (isPlaying && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            Debug.Log("Skipped current video");
            // The coroutine will continue naturally to the next video
        }
    }

    void SkipAllVideos()
    {
        if (isPlaying)
        {
            if (videoSequenceCoroutine != null)
            {
                StopCoroutine(videoSequenceCoroutine);
            }
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            CompleteSequence();
            UpdateStatus("All videos skipped. Please take off the headset and answer the questionnaire.");
        }
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
