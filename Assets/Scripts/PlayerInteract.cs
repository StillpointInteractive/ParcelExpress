using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Transform smallParcelHoldPoint;
    [SerializeField] private Transform mediumParcelHoldPoint;
    [SerializeField] private Transform largeParcelHoldPoint;

    [SerializeField] private float interactRange = 5f;

   
    public Parcel heldParcel;
   


    public Transform cameraPos;

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame) TryPickUp();
  
        if (Keyboard.current.qKey.wasPressedThisFrame && heldParcel != null) Drop(heldParcel);
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
               parcel.isPickedUp = true;
            }
        }
    }

    private void Pickup(Parcel parcel)
    {

        if(parcel.CompareTag("SmallParcel"))
        {
            parcel.transform.position = smallParcelHoldPoint.position;
            parcel.transform.parent = smallParcelHoldPoint;
           
        }
        else if (parcel.CompareTag("MediumParcel"))
        {
            parcel.transform.position = mediumParcelHoldPoint.position;
            parcel.transform.parent = mediumParcelHoldPoint;
        }
        else if (parcel.CompareTag("LargeParcel"))
        {
            parcel.transform.position = largeParcelHoldPoint.position;
            parcel.transform.parent = largeParcelHoldPoint;
        }
        else if (parcel.CompareTag("OversizedParcel"))
        {
            parcel.transform.position = largeParcelHoldPoint.position;
            parcel.transform.parent = largeParcelHoldPoint;
        }

        parcel.transform.forward = transform.forward;
       



    }

    private void Drop(Parcel parcel)
    {
        parcel.isPickedUp = false;

        parcel.transform.SetParent(null);

        heldParcel = null;
    }
}
