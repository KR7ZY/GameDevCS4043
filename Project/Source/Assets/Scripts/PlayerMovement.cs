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

    bool canJump = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        moveRelativeToCamera();
        if (Input.GetAxis("Jump") == 1 && canJump) {
            canJump = false;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(0, 1, 0) * Time.deltaTime * height);
            print("b");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Floor")
        {
            canJump = true;
            print("A");
        }
    }

    private void moveRelativeToCamera()
    {
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
        this.transform.Translate(relativeMovement* Time.deltaTime * speed, Space.World);
    }
}
