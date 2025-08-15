using UnityEngine;
using System.Collections;

public class DataCurveEvents : MonoBehaviour
{
    [SerializeField] TextAsset eventFileAsset;
    private FaceController faceController;
    private Coroutine expressionRoutine;

    void Start()
    {
        // Hide skeleton and stabilize character
        DisableSkeleton();
        StabilizeCharacter();

        // Get components
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

        // Start expression playback
        expressionRoutine = StartCoroutine(ParseAndTriggerEvents());
    }

    void DisableSkeleton()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.name.Contains("Armature") ||
                renderer.name.Contains("Bone") ||
                renderer.name.Contains("Root"))
            {
                renderer.enabled = false;
            }
        }
    }

    void StabilizeCharacter()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }
    }

    IEnumerator ParseAndTriggerEvents()
    {
        string[] lines = eventFileAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Split(',');
            if (data.Length < 4) continue;

            float triggerTime = float.Parse(data[0]);
            string emotion = data[1].Trim();
            float intensity = Mathf.Clamp01(float.Parse(data[2]));
            float duration = float.Parse(data[3]);

            yield return new WaitForSeconds(triggerTime);

            faceController.setCategoricalEmotion(
                emotion,
                intensity,
                0.8f,  // Smoother fade-in
                duration,
                0.8f   // Smoother fade-out
            );
        }
    }

    void OnDisable()
    {
        if (expressionRoutine != null)
        {
            StopCoroutine(expressionRoutine);
        }
    }
}
