using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject sphereInstance;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintTransitionSpeed = 5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [SerializeField] private float sphereDistance = 100f;
    [SerializeField] private float sphereMaxHorizontalAngle = 20f;
    [SerializeField] private float sphereLerpSpeed = 2f;

    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity = 0f;
    private float currentSpeed = 0f;
    private float moveInput;
    private float turnInput;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        InputManagement();
        Movement();
        UpdateAnimator();
        UpdateSpherePosition();
    }

    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void Movement()
    {
        GroundMovement();
        TurnCharacter();
    }

    private void GroundMovement()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * sprintTransitionSpeed);

        Vector3 moveDirection = new Vector3(turnInput, 0, moveInput);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);
        moveDirection = transform.TransformDirection(moveDirection);

        Vector3 move = moveDirection * currentSpeed;
        move.y = CalculateVerticalForce();

        controller.Move(move * Time.deltaTime);
    }

    private float CalculateVerticalForce()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void TurnCharacter()
    {
        if (Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f)
        {
            if (playerCamera != null)
            {
                Vector3 cameraForwardHorizontal = playerCamera.forward;
                cameraForwardHorizontal.y = 0;
                cameraForwardHorizontal.Normalize();

                if (cameraForwardHorizontal != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(cameraForwardHorizontal);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }
            }
        }
    }

    private void UpdateAnimator()
    {
        bool isMoving = Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f;
        animator.SetBool("isWalking", isMoving && controller.isGrounded);
    }

    private void UpdateSpherePosition()
    {
        if (sphereInstance != null && playerCamera != null)
        {
            Vector3 cameraPosition = playerCamera.position;
            Vector3 cameraForward = playerCamera.forward;
            Vector3 desiredPositionInFront = cameraPosition + cameraForward * sphereDistance;

            Vector3 directionToIdealPoint = (desiredPositionInFront - transform.position).normalized;

            Vector3 characterForward = transform.forward;
            characterForward.y = 0;
            characterForward.Normalize();

            float angle = Vector3.SignedAngle(characterForward, directionToIdealPoint, Vector3.up);
            angle = Mathf.Clamp(angle, -sphereMaxHorizontalAngle, sphereMaxHorizontalAngle);

            Quaternion clampedRotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 clampedDirection = clampedRotation * characterForward;

            Vector3 targetPosition = transform.position + clampedDirection * sphereDistance;
            sphereInstance.transform.position = Vector3.Lerp(sphereInstance.transform.position, targetPosition, Time.deltaTime * sphereLerpSpeed);
        }
    }

    public void PlayFootstepSound()
    {
        if (controller.isGrounded && footstepAudioSource != null && footstepClips.Length > 0)
        {
            AudioClip clip = GetRandomFootstepClip();
            if (clip != null)
            {
                footstepAudioSource.PlayOneShot(clip);
            }
        }
    }

    private AudioClip GetRandomFootstepClip()
    {
        int randomIndex = UnityEngine.Random.Range(0, footstepClips.Length);
        return footstepClips[randomIndex];
    }
}