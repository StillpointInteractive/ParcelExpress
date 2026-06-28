using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    private PlayerControls playerControls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float mouseSensitivity = .1f;
    [SerializeField] private Transform cameraTransform;
    
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        lookInput = playerControls.Player.Look.ReadValue<Vector2>();

        Vector3 moveDirection = transform.right * moveInput.x
            + transform.forward * moveInput.y;

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -55f, 55f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
       
        controller.Move(moveDirection * walkSpeed * Time.deltaTime);

        Debug.Log(xRotation);
    }

}
