using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance = 5f;
    public float minJumpHeight;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

   [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;

    private void Awake()
    {
        GameObject player = GameObject.Find("Player");
        pm = player.GetComponent<PlayerMovement>();
        orientation = player.GetComponent<Transform>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.wallRunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        wallLeft = Physics.Raycast(transform.position, orientation.right * -1, out leftWallHit, wallCheckDistance, whatIsWall);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
    }

    private void StateMachine()
    {
        if ((wallLeft || wallRight) && !pm.isGrounded)
        {
            if (!pm.wallRunning)
            {
                StartWallRun();
            }
        }
        else
        {
            if(pm.wallRunning)
            {
                StopWallRun();
            }
        }
    }
    private void StartWallRun()
    {
       pm.wallRunning = true;
        pm.allowGravity = false;
        Debug.Log("WallRunning started");
    }

    private void WallRunningMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        pm.moveDirection = wallForward;

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
    }

    private void StopWallRun()
    {
        pm.wallRunning = false;
        pm.allowGravity = true;
    }

}
