using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Camera CurrentCamera;
    [SerializeField]
    float speed = 10f;
    [SerializeField]
    float sprintMultiplier = 1.3f;
    [SerializeField]
    float acceleration = 10f;
    [SerializeField]
    float deceleration = 10f;
    [SerializeField]
    float jumpForce = 5f;
    [SerializeField]
    LayerMask groundLayer;
    [SerializeField]
    float groundCheckDistance = 0.2f;

    Vector3 movementInput = Vector3.zero;
    Vector3 currentVelocity = Vector3.zero;
    Rigidbody rb;
    Animator animator;
    bool isGrounded;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Enable interpolation for smoother movement

        animator = GetComponent<Animator>(); // Get the Animator component
    }

    private void Update()
    {
        HandleInput();
        CheckGroundStatus();
    }

    private void FixedUpdate()
    {
        MoveRelativeToCamera();
        RotateToCameraDirection();
        UpdateAnimator();
    }

    private void HandleInput()
    {
        float playerVerticalInput = Input.GetAxisRaw("Vertical");
        float playerHorizontalInput = Input.GetAxisRaw("Horizontal");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 forward = CurrentCamera.transform.forward;
        Vector3 right = CurrentCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;
        Vector3 forwardRelative = forward * playerVerticalInput;
        Vector3 rightRelative = right * playerHorizontalInput;
        movementInput = (forwardRelative + rightRelative).normalized * (isSprinting ? sprintMultiplier : 1f);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    private void MoveRelativeToCamera()
    {
        Vector3 targetVelocity = movementInput * speed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, (movementInput == Vector3.zero ? deceleration : acceleration) * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    private void RotateToCameraDirection()
    {
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(CurrentCamera.transform.forward.x, 0, CurrentCamera.transform.forward.z));
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, Time.fixedDeltaTime * acceleration));
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void CheckGroundStatus()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f; // Slightly above the player's position
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        // Debug logs and visualization
        Debug.DrawRay(origin, Vector3.down * (groundCheckDistance + 0.1f), isGrounded ? Color.green : Color.red);
        Debug.Log("Grounded: " + isGrounded);
    }

    private void UpdateAnimator()
    {
        float speed = movementInput.magnitude * this.speed;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
    }
}