using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    private PlayerControls playerControls;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float xRotation = 0f;
    private float verticalVelocity;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float jumpForce = 4f;

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
        

        verticalVelocity += Physics.gravity.y * Time.deltaTime;


        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2;
        }

        if(playerControls.Player.Jump.WasPressedThisFrame() && controller.isGrounded)
        {
            verticalVelocity = jumpForce;
        }


        Vector3 moveDirection = transform.right * moveInput.x
            + transform.forward * moveInput.y;
        moveDirection.y = verticalVelocity;

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
       
        controller.Move(moveDirection * walkSpeed * Time.deltaTime);

       

        
    }

}
