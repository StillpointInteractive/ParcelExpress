using UnityEngine;

public class ParcelPositions : MonoBehaviour
{
    [SerializeField] private Vector3 parcelPositionHigh;
    [SerializeField] private Vector3 parcelPositionLow;
    [SerializeField] private Transform currentParcelTransform;

    [SerializeField] private bool isCrouching;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }
    void Start()
    {
        parcelPositionHigh = new Vector3(0f, 0.05f, 1.4f);
        parcelPositionLow = new Vector3(0f, -0.47f, 1.69f);

    }

    // Update is called once per frame
    void Update()
    {
        currentParcelTransform = transform;

        if (playerMovement != null) isCrouching = playerMovement.isCrouching;

        ParcelCrouch();
    }

    private void ParcelCrouch()
    {
        Vector3 targetPosition = isCrouching ? parcelPositionLow : parcelPositionHigh;


        if (isCrouching)
        {
         
            currentParcelTransform.localPosition = Vector3.Lerp(currentParcelTransform.localPosition, targetPosition, playerMovement.crouchSpeed * Time.deltaTime);
        }
        else
        {
          
            currentParcelTransform.localPosition = Vector3.Lerp(currentParcelTransform.localPosition, targetPosition, playerMovement.crouchSpeed * Time.deltaTime);
        }
    }
}
