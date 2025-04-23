using Unity.Mathematics; // Note: mathmatics isn't used directly here, consider removing if not needed elsewhere.
using UnityEngine;

[RequireComponent(typeof(CharacterController))] // Ensures CharacterController is present
[RequireComponent(typeof(Animator))]          // Ensures Animator is present
public class Player : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main camera Transform used for movement direction.")]
    [SerializeField] private Transform playerCamera;
    [Tooltip("Optional: An object to position relative to the player/camera.")]
    [SerializeField] private GameObject sphereInstance;
    [Tooltip("AudioSource dedicated to playing footstep sounds.")]
    [SerializeField] private AudioSource footstepAudioSource;
    [Tooltip("Array of footstep sounds to randomly choose from.")]
    [SerializeField] private AudioClip[] footstepClips;

    // Component references (cached in Start)
    private CharacterController controller;
    private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [Tooltip("How quickly the player transitions between walk and sprint speed.")]
    [SerializeField] private float sprintTransitionSpeed = 5f; // Renamed for clarity & adjusted default
    [Tooltip("How quickly the character turns to face the camera's direction.")]
    [SerializeField] private float turnSpeed = 10f; // Adjusted default for potentially faster turning
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Sphere Logic Settings")]
    [Tooltip("How far in front the sphere should ideally be positioned.")]
    [SerializeField] private float sphereDistance = 100f;
    [Tooltip("Maximum horizontal angle (degrees) the sphere can deviate from the character's forward direction.")]
    [SerializeField] private float sphereMaxHorizontalAngle = 20f;
    [Tooltip("How quickly the sphere moves to its target position.")]
    [SerializeField] private float sphereLerpSpeed = 2f;

    // Internal State Variables
    private float verticalVelocity = 0f;
    private float currentSpeed = 0f; // Renamed from 'speed' for clarity
    private float moveInput; // Vertical axis input
    private float turnInput; // Horizontal axis input

    // --- Unity Methods ---

    private void Start()
    {
        // Cache component references
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Basic null checks for required components
        if (controller == null)
            Debug.LogError("CharacterController component not found on " + gameObject.name);
        if (animator == null)
            Debug.LogError("Animator component not found on " + gameObject.name);

        // Initial cursor setup (consider moving to a Game Manager or Settings script)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        // Note: The Settings script provided earlier now handles cursor state.
    }

    private void Update()
    {
        // Order of operations can matter: Input -> Movement Logic -> Apply Movement -> Animation
        InputManagement();
        Movement(); // Handles both ground movement calculation and turning
        UpdateAnimator();
        UpdateSpherePosition(); // Update optional sphere position

        // HandleFootsteps(); // REMOVED - Footsteps should now be triggered by Animation Events
    }

    // --- Input ---

    private void InputManagement()
    {
        // Using Unity's legacy Input Manager. Consider the new Input System for more flexibility.
        moveInput = Input.GetAxis("Vertical");   // Forward/Backward
        turnInput = Input.GetAxis("Horizontal"); // Left/Right strafing input

        // JUMP input is handled directly within VerticalForceCalculation where controller.isGrounded is checked.
        // SPRINT input is handled directly within GroundMovement.

        // Reminder: Mouse input for camera rotation should be handled in a separate camera control script.
        // That script should read sensitivity from Settings.Instance.MouseSensitivity.
    }

    // --- Movement Logic ---

    private void Movement()
    {
        // Calculate and apply movement based on input
        GroundMovement();

        // Handle character rotation to face camera direction
        TurnCharacter();
    }

    private void GroundMovement()
    {
        // Determine target speed based on sprint input
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Smoothly transition to the target speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * sprintTransitionSpeed);

        // Create movement vector based on input (relative to character's orientation)
        // Note: turnInput (Horizontal axis) controls strafing, moveInput (Vertical axis) controls forward/back
        Vector3 moveDirection = new Vector3(turnInput, 0, moveInput);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f); // Prevent faster diagonal movement

        // Transform direction from local space to world space based on character's rotation
        moveDirection = transform.TransformDirection(moveDirection);

        // Apply speed
        Vector3 move = moveDirection * currentSpeed;

        // Calculate and apply vertical movement (gravity/jumping)
        move.y = CalculateVerticalForce();

        // Apply the final movement vector to the CharacterController
        controller.Move(move * Time.deltaTime);
    }

    private float CalculateVerticalForce()
    {
        // Reset vertical velocity if grounded
        if (controller.isGrounded)
        {
            // Prevent gravity from building up infinitely while grounded
            // Setting slightly negative helps keep the controller firmly grounded.
            verticalVelocity = -gravity * Time.deltaTime; // Small downward force

            // Handle Jump input
            if (Input.GetButtonDown("Jump")) // Using standard "Jump" button mapping
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
                // Optional: Trigger jump animation here
                // animator.SetTrigger("JumpTrigger");
            }
        }
        else
        {
            // Apply gravity when airborne
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void TurnCharacter()
    {
        // This logic makes the character smoothly turn to face the direction the camera is looking (ignoring vertical tilt).
        // It now runs regardless of movement input, allowing the player to turn while stationary by moving the camera.
        if (playerCamera != null)
        {
            // Get camera's forward direction, projected onto the horizontal plane
            Vector3 cameraForwardHorizontal = playerCamera.forward;
            cameraForwardHorizontal.y = 0;
            cameraForwardHorizontal.Normalize(); // Ensure it's a unit vector

            // Only turn if the camera direction is valid (not looking straight up/down)
            if (cameraForwardHorizontal != Vector3.zero)
            {
                // Calculate the target rotation based on the camera's horizontal direction
                Quaternion targetRotation = Quaternion.LookRotation(cameraForwardHorizontal);

                // Smoothly rotate the character towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
        else
        {
             Debug.LogWarning("PlayerCamera reference not set in Player script. Character turning disabled.");
        }
    }


    // --- Animation ---

    private void UpdateAnimator()
    {
        // Basic animation: Set 'isWalking' based on movement input magnitude.
        // For better results, consider using a Blend Tree based on 'currentSpeed' or input magnitude.
        bool isMoving = Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving; // Optional: Add an isSprinting bool

        animator.SetBool("isWalking", isMoving && controller.isGrounded); // Only walk/run if grounded and moving
        // animator.SetBool("isSprinting", isSprinting && controller.isGrounded); // Example for sprint animation
        // animator.SetFloat("Speed", currentSpeed / sprintSpeed); // Example for Blend Tree
        // animator.SetBool("isGrounded", controller.isGrounded); // Useful for jump/fall animations
    }

    // --- Sphere Positioning Logic ---

    private void UpdateSpherePosition()
    {
        // This positions the sphereInstance relative to the camera and character's forward direction,
        // clamping its horizontal angle relative to the character. Purpose might be targeting, companion, etc.
        if (sphereInstance != null && playerCamera != null)
        {
            // 1. Define the ideal point far in front of the camera
            Vector3 cameraPosition = playerCamera.position;
            Vector3 cameraForward = playerCamera.forward;
            Vector3 desiredPositionInFront = cameraPosition + cameraForward * sphereDistance;

            // 2. Get the direction from the character to this ideal point
            Vector3 directionToIdealPoint = (desiredPositionInFront - transform.position).normalized;

            // 3. Get the character's forward direction (horizontal plane)
            Vector3 characterForward = transform.forward;
            characterForward.y = 0;
            characterForward.Normalize();

            // 4. Calculate the horizontal angle between character forward and the direction to the ideal point
            float angle = Vector3.SignedAngle(characterForward, directionToIdealPoint, Vector3.up);

            // 5. Clamp this angle
            angle = Mathf.Clamp(angle, -sphereMaxHorizontalAngle, sphereMaxHorizontalAngle);

            // 6. Calculate the new clamped direction based on the character's forward + clamped angle
            Quaternion clampedRotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 clampedDirection = clampedRotation * characterForward; // Rotate character's forward by the clamped angle

            // 7. Calculate the final target position along this clamped direction
            Vector3 targetPosition = transform.position + clampedDirection * sphereDistance;

            // 8. Smoothly move the sphere towards the target position
            sphereInstance.transform.position = Vector3.Lerp(sphereInstance.transform.position, targetPosition, Time.deltaTime * sphereLerpSpeed);
        }
    }

    // --- Footstep Audio (Called by Animation Events) ---

    // Make sure your walk/run animations have Animation Events calling this function
    // at the points where the feet touch the ground.
    public void PlayFootstepSound() // Must be public for Animation Events
    {
        if (controller.isGrounded && footstepAudioSource != null && footstepClips.Length > 0)
        {
            // Select a random clip
            AudioClip clip = GetRandomFootstepClip();

            if (clip != null)
            {
                // Play the clip as a one-shot sound
                footstepAudioSource.PlayOneShot(clip);

                // Optional: Add slight random variation to pitch and volume for realism
                // footstepAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                // footstepAudioSource.volume = UnityEngine.Random.Range(0.9f, 1.0f);
            }
        }
    }

    private AudioClip GetRandomFootstepClip()
    {
        // Simple random selection from the array
        int randomIndex = UnityEngine.Random.Range(0, footstepClips.Length);
        return footstepClips[randomIndex];
    }

    /* // OLD FOOTSTEP LOGIC - COMMENTED OUT (Use Animation Events Instead)
    private void HandleFootsteps()
    {
        // Play looped footstep sound while moving and grounded
        bool isMoving = controller.isGrounded && (Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f);

        if (isMoving)
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.clip = GetRandomFootstepClip(); // Selects one clip to loop
                footstepAudioSource.loop = true;
                footstepAudioSource.Play();
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
                footstepAudioSource.loop = false; // Ensure loop is turned off
            }
        }
    }
    */
}