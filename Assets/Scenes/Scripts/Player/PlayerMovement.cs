using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{ 
    [SerializeField] private Transform cameraTransform;

    
   // move speed
  private float moveSpeed = 9f;
  private float sprintMultiplier = 1.6f;
  private float airControlMultiplier = 0.5f;
  private float rotationSpeed = 12f; 

    // gravity , jump
  private float jumpForce = 7f;
  private float groundCheckDistance = 0.2f; 
  private LayerMask groundMask = ~0;
    private float extraGravityMultiplier = 2.5f; 

 // animators
    private string speedParam = "Speed"; 
     private string groundedParam = "IsGrounded";
     private string jumpParam = "Jump";

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
        // freeze rotation so it does not fall
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
        if (keyboard == null) return;

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed) y -= 1f;
        if (keyboard.wKey.isPressed) y += 1f;

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
        Vector3 origin = transform.position + Vector3.up * (_capsule.radius * 0.5f);
        float castDistance = (_capsule.height * 0.5f) - _capsule.radius + groundCheckDistance;

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
        if (_moveInput.sqrMagnitude < 0.0001f) return Vector3.zero;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
    }

    private void ApplyMovement(Vector3 moveDirection)
    {
        float speed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
        float control = _isGrounded ? 1f : airControlMultiplier;

        Vector3 targetVelocity = moveDirection * speed * control;
        Vector3 velocity = _rb.linearVelocity;
        Vector3 velocityChange = new Vector3(targetVelocity.x - velocity.x, 0f, targetVelocity.z - velocity.z);

        _rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void ApplyRotation(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void ApplyJump()
    {
        Vector3 velocity = _rb.linearVelocity;
        velocity.y = 0f; 
        _rb.linearVelocity = velocity;

        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _animator.SetTrigger(jumpParam);
    }

    private void ApplyExtraGravity()
    {
        _rb.AddForce(Physics.gravity * extraGravityMultiplier, ForceMode.Acceleration);
    }

    private void UpdateAnimator()
    {
        float normalizedSpeed = _moveInput.magnitude * (_isSprinting ? sprintMultiplier : 1f);
        _animator.SetFloat(speedParam, normalizedSpeed);
        _animator.SetBool(groundedParam, _isGrounded);
    }
}
