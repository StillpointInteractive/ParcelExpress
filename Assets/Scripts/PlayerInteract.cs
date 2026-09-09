using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("ParcelHoldPositions")]
    [SerializeField] private Transform smallParcelHoldPoint;
    [SerializeField] private Transform mediumParcelHoldPoint;
    [SerializeField] private Transform largeParcelHoldPoint;


    [SerializeField] private float interactRange = 5f;

    private GameObject gManager;
    private ParcelManager pManager;

    public Parcel heldParcel;
    public Transform cameraPos;

    public bool currentlyLookingAtParcel;

    private void Awake()
    {

        gManager = GameObject.Find("GameManager");
        pManager = gManager.GetComponent<ParcelManager>();
    }
    private void Start()
    {
       
    }
    private void Update()
    {
        currentlyLookingAtParcel = false;
        IsLookingAtParcel();

        if (Keyboard.current.eKey.wasPressedThisFrame) TryPickUp();
  
        if (Keyboard.current.qKey.wasPressedThisFrame && heldParcel != null) Drop(heldParcel);

        

    }

    private void IsLookingAtParcel()
    {

        Ray ray = new Ray(cameraPos.transform.position, cameraPos.transform.forward);

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, interactRange))
        {




            if (hit.collider.gameObject.GetComponent<Parcel>() && heldParcel == null)
            {
                currentlyLookingAtParcel = true;
            }
            
        }




    }
    private void TryPickUp()
    {
        if (heldParcel != null) return;
       

        Ray ray = new Ray(cameraPos.transform.position, cameraPos.transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Parcel parcel = hit.collider.GetComponent<Parcel>();


            if (parcel != null && parcel.hasBeenDelivered) return;

            if (parcel != null) heldParcel = parcel;


            if (parcel != null && !parcel.isPickedUp)
            {
                Pickup(parcel);
                parcel.isPickedUp = true;
                pManager.isPickedUp_PM = true;
                pManager.currentlyHeldParcel = parcel;
               if(!parcel.hasBeenPickedUp) GameManager.Instance.StartTimer(30);
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
        pManager.isPickedUp_PM = false;
        pManager.currentlyHeldParcel = null;
        parcel.transform.SetParent(null);

        heldParcel = null;
    }
}
