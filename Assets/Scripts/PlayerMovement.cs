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
    [SerializeField] private float standCheckRadius = 0.3f;
    [SerializeField] private LayerMask standCheckLayers;

    [SerializeField] private float standingCameraHeight = 0.85f;
    [SerializeField] private float crouchingCameraHeight = 0.15f;
    float targetCameraHeight;

   [SerializeField] private Vector3 standingCenter = Vector3.zero;


    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float jumpForce = 4f;

    [SerializeField] private Transform cameraTransform;

    [Header("Slide")]
    [SerializeField] private float slideStartSpeed = 12f;
    [SerializeField] private float slideSpeedBoost = 1.5f;
    [SerializeField] private float slideSensitivity;
    [SerializeField] private float slideSensMultiplpication = 0.2f;
    [SerializeField] private float slideFriction = 8f;
    [SerializeField] private float slideMinimumSpeed = 1f;
    [SerializeField] private float maxSlideTime = 1.2f;
    

    private Vector3 slideDirection;
    private Vector2 slideInput = new Vector2(0, 1);
    [SerializeField] private float slideSpeed;
    private float slideTimer;

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

        standingCenter = controller.center;

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
        GetInput();

        HandleGravity();
        HandleJump();

        HandleMovementStates();

        HandleCrouch();

        HandleMovement();

        HandleLook();

        HandleCamera();

       
    }

    private void GetInput()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        lookInput = playerControls.Player.Look.ReadValue<Vector2>();
    }

    private void HandleGravity()
    {
        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2;
    }

    private void HandleJump()
    {
        if (!controller.isGrounded)
            return;

        if (playerControls.Player.Jump.WasPressedThisFrame())
        {
            verticalVelocity = jumpForce;
        }
    }

    private void HandleMovementStates()
    {
        bool hasMovementInput = moveInput != Vector2.zero;
        bool sprintHeld = playerControls.Player.Sprint.IsPressed();
        bool crouchHeld = playerControls.Player.Crouch.IsPressed();

        isSprinting = controller.isGrounded && hasMovementInput && sprintHeld;


        if(controller.isGrounded && hasMovementInput && sprintHeld && crouchHeld && !isSliding) StartSlide();



        isWalking = controller.isGrounded && hasMovementInput && !isSprinting && !isCrouching;

      
    }

    private void HandleMovement()
    {
        if (isSliding)
        {
            slideSpeed -= slideFriction * Time.deltaTime;

            if (slideSpeed <= slideMinimumSpeed)
            {
                StopSliding();
            }

            if(!playerControls.Player.Crouch.IsPressed())
            {
                StopSliding();
            }
        }

        else if (isSprinting)
            targetSpeed = sprintSpeed;

        else if (isCrouching)
            targetSpeed = crouchMovementSpeed;

        else if (isWalking)
            targetSpeed = walkSpeed;

        else
            targetSpeed = idleSpeed;

        if (currentSpeed < targetSpeed)
            accelOrDeceleration = sprintAcceleration;
        else if (currentSpeed > targetSpeed)
            accelOrDeceleration = sprintDeceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            accelOrDeceleration * Time.deltaTime);

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        Debug.Log(moveInput);
        Vector3 horizontalVelocity;

        if (isSliding)
        {
            horizontalVelocity = slideDirection + moveDirection * slideSpeed;
        }
        else
        {
            horizontalVelocity = moveDirection * currentSpeed;
        }

        Vector3 verticalVelocityVector = Vector3.up * verticalVelocity;

        Vector3 finalVelocity = horizontalVelocity + verticalVelocityVector;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!isSliding)
        {
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
            xRotation -= lookInput.y * mouseSensitivity;
        }
        else
        { 
            transform.Rotate(Vector3.up * lookInput.x * slideSensitivity);
            xRotation -= lookInput.y * slideSensitivity;
        }

          
        xRotation = Mathf.Clamp(xRotation, -50f, 50f); 
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void HandleCrouch()
    {
        if(isSliding)
        {
            isCrouching = true;
        }
        else if (playerControls.Player.Crouch.IsPressed() && !isSprinting)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = !CanStand();
        }

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchSpeed * Time.deltaTime);

        Vector3 targetCenter = standingCenter;

        float heightDifference = standingHeight - targetHeight;

        targetCenter.y -= heightDifference / 2f;

        controller.center = Vector3.Lerp(controller.center, targetCenter, crouchSpeed * Time.deltaTime);

    }
    private bool CanStand()
    {
        Vector3 standCheckPosition = transform.position + standingCenter + Vector3.up * (standingHeight / 2f);
  

        return !Physics.CheckSphere(standCheckPosition,standCheckRadius,standCheckLayers);
        
    }

    private void HandleCamera()
    {

         targetCameraHeight = isCrouching ? crouchingCameraHeight : standingCameraHeight;

        Vector3 cameraPosition = cameraTransform.localPosition;

        cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, crouchSpeed * Time.deltaTime);

        cameraTransform.localPosition = cameraPosition;
    }



    private void StartSlide()
    {

        if (moveInput != slideInput) return;
        isSliding = true;
        isCrouching = true;

        slideDirection = transform.forward;
        slideSpeed = currentSpeed;
        slideSpeed *= slideSpeedBoost;
        slideSensitivity = mouseSensitivity * slideSensMultiplpication;

        slideSpeed -= slideFriction * Time.deltaTime;

        Vector3 cameraPosition = cameraTransform.localPosition;

        cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, crouchSpeed * Time.deltaTime);

        cameraTransform.localPosition = cameraPosition;

        Debug.Log("Slide started");
    }

    private void StopSliding()
    {
        isSliding = false;

        slideSpeed  = 0;

        Debug.Log("Slide Ended");
    }
}



