using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    private bool isZenitalView = true;
    private Quaternion initialRotation;

    private void Start()
    {
        inputManager.OnToggleView += ToggleViewHandler;
        initialRotation = transform.rotation;
    }

    private void ToggleViewHandler()
    {
        isZenitalView = !isZenitalView;
    }

    void LateUpdate()
    {
        if (isZenitalView)
        {
            transform.rotation = initialRotation;
        }
        else
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
