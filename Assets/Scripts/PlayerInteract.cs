using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Transform parcelHoldPoint;
    [SerializeField] private float interactRange = 5f;

   
    public Parcel heldParcel;
   


    public Transform cameraPos;

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickUp();
           
        }
        Debug.DrawRay(cameraPos.transform.position, cameraPos.transform.forward * interactRange, Color.red);
    }

    private void TryPickUp()
    {
        if (heldParcel != null) return;
       

        Ray ray = new Ray(cameraPos.transform.position, cameraPos.transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Parcel parcel = hit.collider.GetComponent<Parcel>();
            

            if (parcel != null) heldParcel = parcel;
           
            if(heldParcel != null) Debug.Log("NoLongerNull");

            if (parcel != null && !parcel.isPickedUp)
            {
                Pickup(parcel);
               
            }
        }
    }

    private void Pickup(Parcel parcel)
    {
        parcel.transform.position = parcelHoldPoint.position;

        parcel.transform.parent = parcelHoldPoint;

        parcel.transform.rotation = parcelHoldPoint.rotation;
    }
}
