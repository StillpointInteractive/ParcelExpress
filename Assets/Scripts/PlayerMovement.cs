using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;


public class PlayerMovement : MonoBehaviour
{

    private CharacterController controller;
    private PlayerControls playerControls;
    private Rigidbody rb;


    private Vector2 moveInput;
    private Vector2 lookInput;

    Vector3 moveDirection;

    private float xRotation = 0f;
    private float verticalVelocity;

    public bool isSprinting = false;
    public bool isWalking = false;
    public bool isCrouching = false;
    public bool isSliding = false;
    public bool isAirborne = false;

    [Header("Idle")]
    [SerializeField] private float idleSpeed = 0f;

    [Header("Movement")]
    [SerializeField] private float crouchMovementSpeed = 2.5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintAcceleration = 12f;
    [SerializeField] private float sprintDeceleration = 20f;
    [SerializeField] private float accelOrDeceleration;
    [SerializeField] private float airAcceleration = 5f;
    public float crouchSpeed = 8f;
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float targetSpeed;
   

    [Header("Sensitivity")]
    [SerializeField] private float jumpSensitivity;
    [SerializeField] private float jumpSensMultiplpication = 0.4f;
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Header("PlayerDimensions")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;
  
    [SerializeField] private float standCheckRadius = 0.3f;
    [SerializeField] private LayerMask standCheckLayers;

    [SerializeField] private float standingCameraHeight = 0.85f;
    [SerializeField] private float crouchingCameraHeight = 0.15f;
    float targetCameraHeight;

    [SerializeField] private Vector3 standingCenter = Vector3.zero;


   
   
    [SerializeField] private Vector3 airVelocity;

    [SerializeField] private Transform cameraTransform;

    [Header("Slide")]
    [SerializeField] private float slideStartSpeed = 12f;
    [SerializeField] private float slideSpeedBoost = 1.5f;
    [SerializeField] private float slideSensitivity;
    [SerializeField] private float slideSensMultiplpication = 0.2f;
    [SerializeField] private float slideFriction = 8f;
    [SerializeField] private float slideMinimumSpeed = 1f;
    [SerializeField] private float maxSlideTime = 1.2f;
    [SerializeField] private float minSprintSpeed = 10f;
    [SerializeField] private float slideCooldown = 3f;

    private Vector3 slideDirection;
    private Vector2 slideInput = new Vector2(0, 1);
    [SerializeField] private float slideSpeed;
    private float nextSlideTime = 0f;

    [Header("GroundCheck")]
    [SerializeField] private float rayCastRange = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        playerControls = new PlayerControls();

        rb = GetComponent<Rigidbody>();


        cameraTransform = Camera.main.transform;
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

        jumpSpeed = sprintSpeed;
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

        CheckIfGrounded();
       
      
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
        if (!isGrounded)
            return;


        jumpSensitivity = mouseSensitivity * jumpSensMultiplpication;

        if (playerControls.Player.Jump.WasPressedThisFrame())
        {
            airVelocity = moveDirection.normalized * currentSpeed;
            verticalVelocity = jumpForce;
           
            
        }
    }

    private void HandleMovementStates()
    {
        bool hasMovementInput = moveInput != Vector2.zero;
        bool sprintHeld = playerControls.Player.Sprint.IsPressed();
        bool crouchHeld = playerControls.Player.Crouch.IsPressed();

        isSprinting = isGrounded && hasMovementInput && sprintHeld;


        if(isGrounded && hasMovementInput && sprintHeld && crouchHeld && !isSliding) StartSlide();



        isWalking = isGrounded && hasMovementInput && !isSprinting && !isCrouching;

        isAirborne = !isGrounded;

      
    }

    private void HandleMovement()
    {
        Vector3 horizontalVelocity;

       
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

        else if (isSprinting) targetSpeed = sprintSpeed;


        else if (isCrouching) targetSpeed = crouchMovementSpeed;


        else if (isWalking) targetSpeed = walkSpeed;

        else if (isAirborne) targetSpeed = currentSpeed;
      
        else targetSpeed = walkSpeed;
       
           

        if (currentSpeed < targetSpeed) accelOrDeceleration = sprintAcceleration;

        else if (currentSpeed > targetSpeed) accelOrDeceleration = sprintDeceleration;


        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            accelOrDeceleration * Time.deltaTime);

        moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;




        if (isSliding)
        {
            horizontalVelocity = slideDirection + moveDirection * slideSpeed;
        }
        else if (isGrounded)
        {
            horizontalVelocity = moveDirection * currentSpeed;
        }
        else
        {

            horizontalVelocity = moveDirection * currentSpeed;
        }

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (isAirborne)
        {
            transform.Rotate(Vector3.up * lookInput.x * jumpSensitivity);
            xRotation -= lookInput.y * jumpSensitivity;
        }
        else if (!isSliding)
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

        if (isAirborne) return;

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

        if (moveInput != slideInput || moveInput == Vector2.zero) return;

        if (Time.time <= nextSlideTime)
        {
            return;
        }
        else
        {
            nextSlideTime = Time.time + slideCooldown;
        }

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

  private bool CheckIfGrounded()
    {

        Ray ray = new Ray(transform.position, transform.up * -1);
        RaycastHit hit;

       

        if(Physics.Raycast(ray, out hit, rayCastRange, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded= false;
        }
            return isGrounded;
    }
}



