using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq; // only used for convenience parsing but not required

public class SimpleSequenceManager : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("Video Clips")]
    public VideoClip happyVideo;
    public VideoClip sadVideo;
    public VideoClip surprisedVideo; // assign this in the Inspector

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    [Tooltip("Place a file named this inside your project's Assets folder (Application.dataPath). Format examples: \"ABC\" or \"A,B,C\"")]
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
        FindStartButton();
        UpdateStatus("VR experience ready. Press Start.");
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
            string raw = File.ReadAllText(filePath).Trim().ToUpper();

            // Accept formats: "ABC", "A,B,C", "A B C", etc.
            if (raw.Contains(","))
            {
                videoOrder = raw.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            }
            else
            {
                // remove whitespace and split into single chars
                raw = raw.Replace(" ", "");
                videoOrder = new string[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                    videoOrder[i] = raw[i].ToString();
            }

            if (videoOrder.Length == 0)
            {
                Debug.LogWarning("Sequence file parsed to zero entries; falling back to default ABC.");
                videoOrder = new string[] { "A", "B", "C" };
            }
        }
        else
        {
            Debug.LogWarning($"Sequence file not found at {filePath}. Using default A/B/C order.");
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

            if (clipToPlay != null)
            {
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                    yield return null;
                }

                videoPlayer.clip = clipToPlay;
                videoPlayer.Prepare();

                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }

                videoPlayer.Play();

                // Status shows only the video count (not emotion)
                UpdateStatus($"Playing: video {currentVideoIndex + 1}/{videoOrder.Length}");

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
            else
            {
                Debug.LogWarning($"No clip mapped for key '{videoKey}' - skipping.");
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
            case "C": return surprisedVideo;
            default: return null;
        }
    }

    void CompleteSequence()
    {
        isPlaying = false;
        UpdateStatus("Sequence complete! Please take off the headset and answer the questionnaire.");
        if (startButton != null) startButton.gameObject.SetActive(true);
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
