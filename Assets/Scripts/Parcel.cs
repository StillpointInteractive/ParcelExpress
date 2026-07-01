using Unity.VisualScripting;
using UnityEngine;

public class Parcel : MonoBehaviour
{
    
    public bool isPickedUp = false;

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
}
