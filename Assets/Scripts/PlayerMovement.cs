using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
    public bool isCrouching = false;
    public bool isSliding = false;

    [SerializeField] private float idleSpeed = 0f;
    [SerializeField] private float crouchMovementSpeed = 2.5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float targetSpeed;
    [SerializeField] private float sprintAcceleration = 12f;
    [SerializeField] private float sprintDeceleration = 20f;
    [SerializeField] private float accelOrDeceleration;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;
    [SerializeField] private float crouchSpeed = 8f;

    [SerializeField] private float standingCameraHeight = 0.85f;
    [SerializeField] private float crouchingCameraHeight = 0.35f;
    [SerializeField] private Vector3 standingCenter = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0f, 0.6f, 0f);

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

        currentSpeed = idleSpeed;

        standingHeight = controller.height;

        Vector3 cameraPos = cameraTransform.localPosition;
        standingCameraHeight = cameraPos.y;

        cameraTransform.localPosition = cameraPos;
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

        if(playerControls.Player.Crouch.IsPressed() && !isSprinting) isCrouching = true;

        else isCrouching = false;

        if (playerControls.Player.Crouch.IsPressed() && isSprinting) isSliding = true;

        else isSliding = false;

        isSprinting = controller.isGrounded && moveInput != Vector2.zero && playerControls.Player.Sprint.IsPressed();



        if (moveInput != Vector2.zero && !isSprinting) isWalking = true;
        else isWalking = false;



        if (isCrouching)
            targetSpeed = crouchMovementSpeed; 
        else if (isSprinting)
            targetSpeed = sprintSpeed;
        else if (isWalking)
            targetSpeed = walkSpeed;
        else
            targetSpeed = idleSpeed;

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

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSpeed * Time.deltaTime);

        //Vector3 targetCenter = isCrouching ? crouchingCenter : standingCenter;

        //controller.center = Vector3.Lerp(controller.center, targetCenter, crouchSpeed * Time.deltaTime);

        float targetCameraHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;

        Vector3 cameraPosition = cameraTransform.localPosition;

        cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, crouchSpeed * Time.deltaTime);

       
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);


       

    }

}
