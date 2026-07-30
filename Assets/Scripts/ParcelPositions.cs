using UnityEngine;

public class ParcelPositions : MonoBehaviour
{
    [SerializeField] private Transform parcelPositionHigh;
    [SerializeField] private Transform parcelPositionLow;
    [SerializeField] private Transform currentParcelTransform;

    [SerializeField] private bool isCrouching;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }
    void Start()
    {
        parcelPositionHigh.position = new Vector3(0f, 0.05f, 1.4f);
        parcelPositionLow.position = new Vector3(0f, -27f, 1.69f);

    }

    // Update is called once per frame
    void Update()
    {
        currentParcelTransform = transform;

        if (playerMovement != null) isCrouching = playerMovement.isCrouching;
       
    }
}
