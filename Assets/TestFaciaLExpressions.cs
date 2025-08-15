using UnityEngine;

public class TestFacialExpressions : MonoBehaviour {
    void Start() {
        // Test a smile (Happiness at full intensity for 2 seconds)
        GetComponent<FaceController>().setCategoricalEmotion("Happiness", 1.0f, 0.3f, 2.0f, 0.3f);
    }
}
