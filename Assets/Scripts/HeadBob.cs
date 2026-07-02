using UnityEngine;

public class HeadBob : MonoBehaviour
{
    private Vector3 startPosition;
    private float wavePosition;
    private float bobSpeed = 2f;

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
        wavePosition += Time.deltaTime * bobSpeed;


        float bobAmount = Mathf.Sin(wavePosition);
        float bobHeight = bobAmount * 0.1f;

        transform.localPosition = new Vector3(startPosition.x, 
            startPosition.y + bobHeight,
            startPosition.z
            );
    }
}
