using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Gravity")]
    [SerializeField] private float extraGravityMultiplier = 2.5f;

    [Header("Step Climbing")]
    [SerializeField] private bool enableStepClimbing = true;

    [SerializeField] private float maxStepHeight = 0.45f;

    [SerializeField] private float stepCheckDistance = 0.45f;

    [SerializeField] private float stepUpOffset = 0.05f;
    [SerializeField] private LayerMask stepMask = ~0;

    [Header("Animator")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string jumpParam = "Jump";

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody _rb;
    private CapsuleCollider _capsule;
    private Animator _animator;

    private Vector2 _moveInput;
    private bool _jumpRequested;
    private bool _isSprinting;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();

        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        ReadInput();
        CheckGrounded();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = GetCameraRelativeDirection();

        if (enableStepClimbing && _isGrounded)
        {
            TryStepUp(moveDirection);
        }

        ApplyMovement(moveDirection);
        ApplyRotation(moveDirection);
        ApplyExtraGravity();

        if (_jumpRequested)
        {
            ApplyJump();
            _jumpRequested = false;
        }
    }

    private void ReadInput()
    {
        var keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed)
            x -= 1f;

        if (keyboard.dKey.isPressed)
            x += 1f;

        if (keyboard.sKey.isPressed)
            y -= 1f;

        if (keyboard.wKey.isPressed)
            y += 1f;

        _moveInput.x = x;
        _moveInput.y = y;

        _isSprinting = keyboard.leftShiftKey.isPressed;

        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _jumpRequested = true;
        }
    }

    private void CheckGrounded()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * (_capsule.radius * 0.5f);

        float castDistance =
            (_capsule.height * 0.5f) -
            _capsule.radius +
            groundCheckDistance;

        _isGrounded = Physics.SphereCast(
            origin,
            _capsule.radius * 0.9f,
            Vector3.down,
            out _,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private Vector3 GetCameraRelativeDirection()
    {
        if (_moveInput.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        if (cameraTransform == null)
            return new Vector3(
                _moveInput.x,
                0f,
                _moveInput.y
            ).normalized;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return (
            camForward * _moveInput.y +
            camRight * _moveInput.x
        ).normalized;
    }

    private void ApplyMovement(Vector3 moveDirection)
    {
        float speed =
            moveSpeed *
            (_isSprinting ? sprintMultiplier : 1f);

        float control =
            _isGrounded
                ? 1f
                : airControlMultiplier;

        Vector3 targetVelocity =
            moveDirection * speed * control;

        Vector3 velocity = _rb.linearVelocity;

        Vector3 velocityChange =
            new Vector3(
                targetVelocity.x - velocity.x,
                0f,
                targetVelocity.z - velocity.z
            );

        _rb.AddForce(
            velocityChange,
            ForceMode.VelocityChange
        );
    }

    private void ApplyRotation(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                moveDirection,
                Vector3.up
            );

        _rb.MoveRotation(
            Quaternion.Slerp(
                _rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }

    private void ApplyJump()
    {
        Vector3 velocity = _rb.linearVelocity;

        velocity.y = 0f;

        _rb.linearVelocity = velocity;

        _rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );

        if (_animator != null)
            _animator.SetTrigger(jumpParam);
    }

    private void ApplyExtraGravity()
    {
        _rb.AddForce(
            Physics.gravity * extraGravityMultiplier,
            ForceMode.Acceleration
        );
    }


    private void TryStepUp(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.0001f)
            return;

        moveDirection.y = 0f;
        moveDirection.Normalize();

        float radius = _capsule.radius * 0.9f;

        float lowerRayHeight = 0.05f;

        Vector3 lowerOrigin =
            transform.position +
            Vector3.up * lowerRayHeight;

        if (!Physics.Raycast(
                lowerOrigin,
                moveDirection,
                out RaycastHit lowerHit,
                stepCheckDistance,
                stepMask,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (Vector3.Angle(lowerHit.normal, Vector3.up) < 5f)
            return;

        Vector3 upperOrigin =
            transform.position +
            Vector3.up * (maxStepHeight + stepUpOffset);

        if (Physics.Raycast(
                upperOrigin,
                moveDirection,
                out _,
                stepCheckDistance,
                stepMask,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }
  Vector3 stepTopOrigin =
            transform.position +
            moveDirection * stepCheckDistance +
            Vector3.up * (maxStepHeight + 0.2f);

        if (!Physics.Raycast(
                stepTopOrigin,
                Vector3.down,
                out RaycastHit stepTopHit,
                maxStepHeight + 0.4f,
                stepMask,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        float targetY =
            stepTopHit.point.y +
            (_capsule.height * 0.5f) +
            stepUpOffset;

        float currentBottom =
            transform.position.y -
            (_capsule.height * 0.5f);

        float stepHeight =
            targetY - currentBottom;

        if (stepHeight <= 0.01f ||
            stepHeight > maxStepHeight)
        {
            return;
        }

        // ارفع اللاعب
        Vector3 newPosition = _rb.position;

        newPosition.y += stepHeight;

        _rb.position = newPosition;
    }
    

    private void UpdateAnimator()
    {
        if (_animator == null)
            return;

        float normalizedSpeed =
            _moveInput.magnitude *
            (_isSprinting ? sprintMultiplier : 1f);

        _animator.SetFloat(
            speedParam,
            normalizedSpeed
        );

        _animator.SetBool(
            groundedParam,
            _isGrounded
        );
    }

    public void TextBButton()
    {
        Debug.Log("working bro wtf");
    }
}