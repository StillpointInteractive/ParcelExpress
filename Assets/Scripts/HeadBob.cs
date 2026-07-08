using UnityEngine;

public class HeadBob : MonoBehaviour
{
    private Vector3 startPosition;
    private float wavePosition;
    [SerializeField] private float idleBobSpeed = 2f;
    [SerializeField] private float walkBobSpeed = 3f;
    [SerializeField] private float sprintBobSpeed = 4f;
    [SerializeField] private float currentBobSpeed = 0f;

    [SerializeField] private float idleBobHeight = 0.05f;
    [SerializeField] private float walkBobHeight = 0.08f;
    [SerializeField] private float sprintBHeight = 0.12f;
    [SerializeField] private float currentBobHeight = 0f;

    [SerializeField] private float idleBobWidth = 0.05f;
    [SerializeField] private float walkBobWidth = 0.07f;
    [SerializeField] private float sprintBobWidth = 0.09f;
    [SerializeField] private float currentBobWidth = 0f;

    private PlayerMovement PlayerMovement;

   

    private void Awake()
    {
        PlayerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Start()
    {
        startPosition = transform.localPosition;

       
    }

    private void Update()
    {

        if(PlayerMovement != null)
        {
            if (PlayerMovement.isWalking)
            {
                currentBobSpeed = walkBobSpeed;
                currentBobHeight = walkBobHeight;
                currentBobWidth = walkBobWidth;
            }
            else if (PlayerMovement.isSprinting)
            {
                currentBobSpeed = sprintBobSpeed;
                currentBobHeight = walkBobHeight;
                currentBobWidth = sprintBobWidth;
            }
            else
            {
                currentBobSpeed = idleBobSpeed;
                currentBobHeight = idleBobHeight;
                currentBobWidth = idleBobWidth;
            }
        }
        
         wavePosition += Time.deltaTime * currentBobSpeed;


        float verticalBob = Mathf.Sin(wavePosition) * currentBobHeight;
        float horizontalBob = Mathf.Sin(wavePosition * 0.5f) * currentBobWidth;

     Vector3 targetPosition = new Vector3(startPosition.x = horizontalBob, startPosition.y + verticalBob,startPosition.y);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
    }
}
