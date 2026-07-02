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

    public bool isSprinting = false;
    public bool isWalking = false;

    [SerializeField] private float idleSpeed = 0f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float targetSpeed;
    [SerializeField] private float sprintAcceleration = 12f;
    [SerializeField] private float sprintDeceleration = 20f;
    [SerializeField] private float accelOrDeceleration;

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

        currentSpeed = walkSpeed;
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


        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2;
        

        if(playerControls.Player.Jump.WasPressedThisFrame() && controller.isGrounded)
        {
            verticalVelocity = jumpForce;
        }

        if (controller.isGrounded && playerControls.Player.Sprint.IsPressed()) isSprinting = true;
        else isSprinting = false;

        if (moveInput != Vector2.zero) isWalking = true;
        else isWalking = false;



        if (isSprinting) targetSpeed = sprintSpeed;
        else if(isWalking) targetSpeed = walkSpeed;
        else targetSpeed = idleSpeed;

        if (currentSpeed < targetSpeed) accelOrDeceleration = sprintAcceleration;

        else if (currentSpeed > targetSpeed) accelOrDeceleration = sprintDeceleration;
       


        Vector3 moveDirection = transform.right * moveInput.x
                + transform.forward * moveInput.y;

        moveDirection.y = verticalVelocity;

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -50f, 50f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelOrDeceleration * Time.deltaTime);
       
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);


       

    }

}
