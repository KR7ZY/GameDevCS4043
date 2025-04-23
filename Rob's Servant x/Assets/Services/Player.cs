using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    private Animator animator;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject sphereInstance;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintTransitSpeed = 2f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    private float verticalVelocity = 0f;
    private float speed = 0f;

    [Header("Input")]
    private float moveInput;
    private float turnInput;

    private float sphereDistance = 100f;

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
        HandleFootsteps();
    }

    private void Movement()
    {
        GroundMovement();
        Turn();
    }

    private void GroundMovement()
    {
        Vector3 move = new Vector3(turnInput, 0, moveInput);
        move = transform.TransformDirection(move);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = Mathf.Lerp(speed, sprintSpeed, Time.deltaTime * sprintTransitSpeed);
        }
        else
        {
            speed = Mathf.Lerp(speed, moveSpeed, Time.deltaTime * sprintTransitSpeed);
        }
        move *= speed;
        move.y = VerticalForceCalculatation();
        controller.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            Vector3 currentLookDirection = playerCamera.forward;
            currentLookDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(currentLookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private float VerticalForceCalculatation()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = 0f;
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

    private void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void UpdateAnimator()
    {
        bool isWalking = Mathf.Abs(moveInput) > 0 || Mathf.Abs(turnInput) > 0;
        animator.SetBool("isWalking", isWalking);
    }

    private void UpdateSpherePosition()
    {
        if (sphereInstance != null)
        {
            Vector3 cameraPosition = playerCamera.position;
            Vector3 forwardDirection = playerCamera.forward;
            Vector3 characterForward = transform.forward;

            Vector3 desiredPosition = cameraPosition + forwardDirection * sphereDistance;

            Vector3 directionToSphere = (desiredPosition - transform.position).normalized;

            float angle = Vector3.SignedAngle(characterForward, directionToSphere, Vector3.up);

            angle = Mathf.Clamp(angle, -20f, 20f);

            Quaternion clampedRotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 clampedDirection = clampedRotation * characterForward;

            Vector3 targetPosition = transform.position + clampedDirection * sphereDistance;

            float lerpSpeed = 2f;
            sphereInstance.transform.position = Vector3.Lerp(sphereInstance.transform.position, targetPosition, Time.deltaTime * lerpSpeed);
        }
    }

    private void HandleFootsteps()
    {
        if (controller.isGrounded && (Mathf.Abs(moveInput) > 0 || Mathf.Abs(turnInput) > 0))
        {
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.clip = GetRandomFootstepClip();
                footstepAudioSource.loop = true;
                footstepAudioSource.Play();
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
        }
    }

    private AudioClip GetRandomFootstepClip()
    {
        if (footstepClips.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, footstepClips.Length);
            return footstepClips[randomIndex];
        }
        return null;
    }
}