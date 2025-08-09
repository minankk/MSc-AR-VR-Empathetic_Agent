using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Specialized;

public class ActionToVector2FPS : MonoBehaviour {
    public InputActionReference actionReference;
    public CharacterController characterController;
    public GameObject head;

    Vector3 moveDirection = Vector3.zero;

    void Update() {
        if (actionReference != null && actionReference.action != null) {
            Vector2 value = actionReference.action.ReadValue<Vector2>();
            Vector3 forward = head.transform.forward;
            characterController.SimpleMove(forward * value.y);

        }
    }
}
