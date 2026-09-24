using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;   
    [SerializeField] private Camera cam;           

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Distance")]
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float cameraRadius = 0.2f; 

    [Header("Follow")]
    [SerializeField] private float followSmoothTime = 0.05f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    private float _yaw;
    private float _pitch = 15f;
    private Vector3 _followVelocity;

    private void Awake()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();
        _yaw = transform.eulerAngles.y;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleCursorToggle();
    //   ReadMouseInput();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pivot = target.position + targetOffset;

        transform.position = Vector3.SmoothDamp(transform.position, pivot, ref _followVelocity, followSmoothTime);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        PositionCameraWithCollision(pivot);
    }

    private void HandleCursorToggle()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

 /*   private void ReadMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue();
        
        _yaw += delta.x * mouseSensitivity * 0.01f;
        _pitch -= delta.y * mouseSensitivity * 0.01f;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }*/
    
    private void PositionCameraWithCollision(Vector3 pivot)
    {
        Vector3 desiredPosition = pivot - (transform.forward * distance);
        Vector3 direction = (desiredPosition - pivot).normalized;

        float clampedDistance = distance;

        if (Physics.SphereCast(pivot, cameraRadius, direction, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            clampedDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        cam.transform.position = pivot - (transform.forward * clampedDistance);
        cam.transform.rotation = transform.rotation;
    }
}
