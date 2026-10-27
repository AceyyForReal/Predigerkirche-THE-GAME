using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(AudioSource), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty lookAction;
    [SerializeField] private InputActionProperty jumpAction;
    [SerializeField] private InputActionProperty sprintAction;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float footstepCooldown = 0.5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Animator Mapping")]
    [SerializeField] private float deadZone = 0.15f;
    [SerializeField] private float walkValue = 1f;
    [SerializeField] private float runValue = 2f;
    [SerializeField] private float strafeValue = 1f;

    [Header("Walking Sounds")]
    [SerializeField] private AudioClip[] walkSounds;

    [Header("Collision Checks")]
    [SerializeField] private float wallCheckDistance = 0.5f;

    private AudioSource audioSource;
    private CharacterController characterController;
    private Transform playerCamera;
    private Animator animator;
    private Vector3 velocity;
    private float rotationX = 0f;
    private float lastFootstepTime = 0f;
    
    [Header("Camera")]
    [SerializeField] private Transform headBone;
    [SerializeField] private Vector3 fp_positionOffset;

    public float targetX;
    public float targetZ;
    public bool isTalkingToAquaman;

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();

        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJump;

        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        playerCamera = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * lookSpeed * mouseSensitivity;
        float mouseY = lookInput.y * lookSpeed * mouseSensitivity;

        rotationX = Mathf.Clamp(rotationX - mouseY, -90f, 90f);
        if (playerCamera != null) playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (Time.timeScale > 0 && headBone != null && playerCamera != null)
            playerCamera.transform.position = headBone.position + headBone.TransformDirection(fp_positionOffset);
    }

    private void HandleMovement()
    {
        bool isGrounded = Physics.CheckSphere(transform.position + Vector3.down * 0.1f, 0.3f, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        float horizontal = input.x;
        float vertical = input.y;
        
        bool wantsToSprint = sprintAction.action.ReadValue<float>() > 0;
        bool blockedByWall = IsBlockedByWall();

        if (blockedByWall)
        {
            horizontal = 0f;
            vertical = 0f;
        }

        bool isSprinting = wantsToSprint && !blockedByWall;

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (isGrounded && move.magnitude > 0.5f && Time.time - lastFootstepTime > footstepCooldown)
        {
            PlayRandomFootstepSound();
            lastFootstepTime = Time.time;
        }

        SetMovementAnimations(horizontal, vertical, isSprinting);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        bool isGrounded = Physics.CheckSphere(transform.position + Vector3.down * 0.1f, 0.3f, groundMask);
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private bool IsBlockedByWall()
    {
        Vector3 origin = transform.position + Vector3.up * (characterController.height * 0.5f);
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, wallCheckDistance, groundMask))
        {
            float angle = Vector3.Angle(-hit.normal, transform.forward);
            if (angle <= 45f) return true;
        }
        return false;
    }

    private void SetMovementAnimations(float horizontal, float vertical, bool isSprinting)
    {
        float tx = 0f;
        float tz = 0f;

        if (vertical > deadZone) tz = isSprinting ? runValue : walkValue;
        else if (vertical < -deadZone) tz = isSprinting ? -runValue : -walkValue;

        if (horizontal > deadZone) tx = strafeValue;
        else if (horizontal < -deadZone) tx = -strafeValue;

        targetX = tx;
        targetZ = tz;

        animator.SetFloat("MoveX", targetX, 0.05f, Time.deltaTime);
        animator.SetFloat("MoveY", targetZ, 0.05f, Time.deltaTime);
        animator.SetBool("isRunning", isSprinting);
        animator.speed = isSprinting ? 1.3f : 1f;
    }

    private void PlayRandomFootstepSound()
    {
        if (walkSounds.Length > 0)
        {
            AudioClip clip = walkSounds[Random.Range(0, walkSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}