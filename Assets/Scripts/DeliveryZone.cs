using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Parcel parcel = other.GetComponent<Parcel>();

        if (parcel != null && !parcel.isPickedUp)
        {
            parcel.Deliver();

            
        }
    }
}
