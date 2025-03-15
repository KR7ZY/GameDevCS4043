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
    float height = 5f;

    int canJump = 0;
    int jumping = 0;
    int jumpCount = 0;

    [SerializeField]
    float jumpTime = 1;

    Vector3 vectorOverride = new Vector3(0, 0, 0);

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.name == "Floor")
        {
            canJump = 1;
            print("A");
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        moveRelativeToCamera();
        jump();
    }

    private void jump()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (Input.GetAxis("Jump") == 1 && canJump == 1)
        {
            canJump = 0;
            jumping = 1;
        }
        if (jumping == 1)
        {
            if (canJump == 1)
            {
                jumping = 0;
            } else
            {
                rb.AddForce(new Vector3(0, height*1000, 0) * Time.deltaTime);
            }
        }
        jumpCount += 1;
        if (jumpCount >= jumpTime)
        {
            jumping = 0;
            jumpCount = 0;
        }
    }

    private void moveRelativeToCamera()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float playerVerticalInput = Input.GetAxis("Vertical");
        float playerHorizontalInput = Input.GetAxis("Horizontal");
        Vector3 forward = CurrentCamera.transform.forward;
        Vector3 right = CurrentCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;
        Vector3 forwardRelative = forward * playerVerticalInput;
        Vector3 rightRelative = right * playerHorizontalInput;
        Vector3 relativeMovement = forwardRelative + rightRelative;
        relativeMovement = relativeMovement.normalized;
        //this.transform.Translate(relativeMovement* Time.deltaTime * speed, Space.World);
        rb.AddForce(relativeMovement * Time.deltaTime * (speed*1000) + vectorOverride);
        vectorOverride = new Vector3(0, 0, 0);
    }
}
