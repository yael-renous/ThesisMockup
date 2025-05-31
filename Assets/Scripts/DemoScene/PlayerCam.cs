using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 1.0f; // Set default sensitivity values
    public float sensY = 1.0f;

    public Transform orientation;

    float xRotation;
    float yRotation;

     private PlayerInput playerInput;
     private InputAction lookAction;

    bool isMousePressed = false;

    void Awake()
    {
        playerInput = new PlayerInput();
        lookAction = playerInput.Player.Look;
    }

    void OnEnable()
    {
        lookAction.Enable();
    }

    void OnDisable()
    {
        lookAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the left mouse button is pressed
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isMousePressed = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Check if the Esc key is pressed
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isMousePressed = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Only update camera rotation if the mouse is pressed
        if (isMousePressed && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            float mouseX = lookInput.x * Time.deltaTime * sensX;
            float mouseY = lookInput.y * Time.deltaTime * sensY;

            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}
