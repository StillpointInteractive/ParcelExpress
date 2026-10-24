using UnityEngine;
using System.Collections;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    public bool allowedToWallRun = true;
    public bool isWallRunning = false;
    public bool lookinAtWall = false;
    public float wallRunForce;
    public float maxWallRunTime;
    public float wrTimer = 2f; // There is a function called WallRunTimer. I need a variable with the same name so i have abreviated the timer variable to wrTimer.
    public float wallJumpForce = 4f;
    public float wallLeanAmount;
   

    private Coroutine wallRunningCoroutine;
   

    [Header("Detection")]
    public float wallCheckDistance = 7f;
    public float wallLookDistance = 5f;
    public float minJumpHeight;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

   public bool wallLeft;
   public bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    public Camera camera;

    private void Awake()
    {
        GameObject player = GameObject.Find("Player");
        pm = player.GetComponent<PlayerMovement>();
        orientation = player.GetComponent<Transform>();
        camera = Camera.main;
    }

    private void Update()
    {
        RotateCamera();
        CheckForWall();
        StateMachine();
        FacingWall();
       
      

        if (pm.isGrounded) allowedToWallRun = true;
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
        if (!allowedToWallRun)
            return;

        isWallRunning = true;
        pm.wallRunning = true;
        pm.allowGravity = false;
        pm.isAirborne = false;

        
        StartCoroutine(WallRunTimer());


        wallRunningCoroutine = StartCoroutine(WallRunTimer());
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
        isWallRunning = false;
        pm.wallRunning = false;
        pm.allowGravity = true;

        
        if (wallRunningCoroutine != null) StopCoroutine(wallRunningCoroutine);
    }

    private IEnumerator ToggleBoolAfterDelay(float delay, System.Action ToggleBool)
    {
        yield return new WaitForSeconds(delay);

        ToggleBool();
    }

    private IEnumerator WallRunTimer()
    {

        float timer = 0f;

        while (timer < wrTimer)
        {
            if(pm.isAirborne)
            {
                wallRunningCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(wrTimer);

        allowedToWallRun = false;
        wallRunningCoroutine = null;

    }

    private void FacingWall()
    {

        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;
       lookinAtWall = Physics.Raycast(ray, out hit, wallLookDistance, whatIsWall);
    }

    private void RotateCamera()
    {
        if (wallLeft) wallLeanAmount = -15f;
        else if (wallRight) wallLeanAmount = 15f;
        else wallLeanAmount = 0f;

       
    }

    

}
