using Unity.VisualScripting;
using UnityEngine;

public class Parcel : MonoBehaviour
{

    public bool isPickedUp = false;

    public bool hasBeenDelivered = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isPickedUp) rb.isKinematic = true;
        else rb.isKinematic = false;


    }

    public void Deliver()
    {
        if (hasBeenDelivered) return;
        hasBeenDelivered = true;

        GameManager.Instance.ParcelDelivered(this);
    }
}
