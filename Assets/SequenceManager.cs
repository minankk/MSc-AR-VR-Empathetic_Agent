using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class SequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class VideoSequence
    {
        public string sequenceName;
        public VideoClip[] videos;
        public string[] emotionalStates;
    }

    [Header("Video Players")]
    public VideoPlayer videoPlayerA;
    public VideoPlayer videoPlayerB;
    public VideoPlayer videoPlayerC;

    [Header("UI Elements")]
    public Button startButton;
    public TextMeshProUGUI statusText;

    [Header("Video Assignments")]
    public VideoClip happyVideo;
    public VideoClip sadVideo;
    public VideoClip angryVideo;

    [Header("Configuration")]
    public string sequenceFile = "video_sequence.txt";
    public float pauseBetweenVideos = 2.0f;

    [Header("PC Testing Options")]
    public bool enablePCTesting = true;
    public KeyCode testStartKey = KeyCode.Space;
    public KeyCode skipVideoKey = KeyCode.N;
    public KeyCode pauseResumeKey = KeyCode.P;

    private VideoClip[] videoClips;
    private string[] emotionalStates;
    private bool isPlaying = false;
    private int currentVideoIndex = 0;
    private Coroutine sequenceCoroutine;

    void Start()
    {
        // Initialize video clips array
        videoClips = new VideoClip[3] { happyVideo, sadVideo, angryVideo };

        // Setup button listener
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartSequence);
            startButton.interactable = true;
        }

        // Load sequence from file
        LoadSequenceFromFile();

        // Set initial status
        UpdateStatus("Ready. Press Start to play sequence.");

        // Log testing instructions
        if (enablePCTesting)
        {
            Debug.Log("PC Testing Enabled!");
            Debug.Log("Press " + testStartKey + " to start sequence");
            Debug.Log("Press " + skipVideoKey + " to skip current video");
            Debug.Log("Press " + pauseResumeKey + " to pause/resume");
        }
    }

    void Update()
    {
        // PC Testing controls
        if (enablePCTesting)
        {
            // Start sequence with keyboard
            if (Input.GetKeyDown(testStartKey) && !isPlaying)
            {
                StartSequence();
            }

            // Skip current video
            if (Input.GetKeyDown(skipVideoKey) && isPlaying)
            {
                SkipCurrentVideo();
            }

            // Pause/Resume
            if (Input.GetKeyDown(pauseResumeKey))
            {
                TogglePause();
            }
        }
    }

    void LoadSequenceFromFile()
    {
        string filePath = Path.Combine(Application.dataPath, sequenceFile);

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length >= 1)
            {
                string sequence = lines[0].Trim().ToUpper();

                if (sequence.Length == 3)
                {
                    emotionalStates = new string[3];

                    for (int i = 0; i < 3; i++)
                    {
                        switch (sequence[i])
                        {
                            case 'A':
                                emotionalStates[i] = "Happy";
                                break;
                            case 'B':
                                emotionalStates[i] = "Sad";
                                break;
                            case 'C':
                                emotionalStates[i] = "Angry";
                                break;
                            default:
                                emotionalStates[i] = "Unknown";
                                break;
                        }
                    }

                    Debug.Log("Loaded sequence: " + string.Join(" -> ", emotionalStates));
                    return;
                }
            }
        }

        // Default sequence if file doesn't exist or is invalid
        emotionalStates = new string[3] { "Happy", "Sad", "Angry" };
        Debug.LogWarning("Using default sequence: Happy -> Sad -> Angry");
    }

    public void StartSequence()
    {
        if (!isPlaying)
        {
            if (startButton != null)
            {
                startButton.interactable = false;
            }

            isPlaying = true;
            currentVideoIndex = 0;

            sequenceCoroutine = StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        while (currentVideoIndex < emotionalStates.Length)
        {
            string emotion = emotionalStates[currentVideoIndex];
            VideoClip clipToPlay = null;

            // Determine which clip to play based on emotion
            switch (emotion)
            {
                case "Happy":
                    clipToPlay = happyVideo;
                    break;
                case "Sad":
                    clipToPlay = sadVideo;
                    break;
                case "Angry":
                    clipToPlay = angryVideo;
                    break;
            }

            if (clipToPlay != null)
            {
                // Determine which video player to use
                VideoPlayer activePlayer = null;
                switch (currentVideoIndex % 3)
                {
                    case 0:
                        activePlayer = videoPlayerA;
                        break;
                    case 1:
                        activePlayer = videoPlayerB;
                        break;
                    case 2:
                        activePlayer = videoPlayerC;
                        break;
                }

                if (activePlayer != null)
                {
                    // Play the video
                    activePlayer.clip = clipToPlay;
                    UpdateStatus("Playing: " + emotion + " (Press N to skip)");
                    activePlayer.Play();

                    // Wait for the video to finish or skip
                    float timer = 0;
                    while (timer < clipToPlay.length && activePlayer.isPlaying)
                    {
                        timer += Time.deltaTime;
                        yield return null;

                        // Check for skip
                        if (enablePCTesting && Input.GetKeyDown(skipVideoKey))
                        {
                            activePlayer.Stop();
                            break;
                        }
                    }

                    // Pause between videos
                    if (currentVideoIndex < emotionalStates.Length - 1)
                    {
                        UpdateStatus("Pausing between videos... (Press N to continue)");

                        float pauseTimer = 0;
                        while (pauseTimer < pauseBetweenVideos)
                        {
                            pauseTimer += Time.deltaTime;
                            yield return null;

                            // Check for skip pause
                            if (enablePCTesting && Input.GetKeyDown(skipVideoKey))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            currentVideoIndex++;
        }

        // Sequence finished
        CompleteSequence();
    }

    void SkipCurrentVideo()
    {
        // Stop current video and move to next
        VideoPlayer[] players = { videoPlayerA, videoPlayerB, videoPlayerC };
        foreach (var player in players)
        {
            if (player.isPlaying)
            {
                player.Stop();
                break;
            }
        }
    }

    void TogglePause()
    {
        if (!isPlaying) return;

        VideoPlayer[] players = { videoPlayerA, videoPlayerB, videoPlayerC };
        foreach (var player in players)
        {
            if (player.isPlaying || player.isPaused)
            {
                if (player.isPlaying)
                {
                    player.Pause();
                    UpdateStatus("PAUSED - Press P to resume");
                }
                else
                {
                    player.Play();
                    UpdateStatus("Resumed playing");
                }
                break;
            }
        }
    }

    void CompleteSequence()
    {
        UpdateStatus("Sequence complete!");
        isPlaying = false;

        if (startButton != null)
        {
            startButton.interactable = true;
        }
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("Status: " + message);
    }

    // For testing in editor
    [ContextMenu("Test Sequence")]
    void TestSequence()
    {
        if (!isPlaying)
        {
            StartSequence();
        }
    }

    [ContextMenu("Skip Current Video")]
    void EditorSkipVideo()
    {
        if (isPlaying)
        {
            SkipCurrentVideo();
        }
    }
}
