using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("Mouse")]
    [SerializeField] private CursorLockMode cursorLockMode = CursorLockMode.Locked;
    [SerializeField] private bool cursorVisible = false;

    private void Update()
    {
        MouseSettings();
    }

    void MouseSettings()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            cursorLockMode = CursorLockMode.None;
            cursorVisible = true;
        }
        else
        {
            cursorLockMode = CursorLockMode.Locked;
            cursorVisible = false;
        }
        Cursor.lockState = cursorLockMode;
        Cursor.visible = cursorVisible;
    }
}
