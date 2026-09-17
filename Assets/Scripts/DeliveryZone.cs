using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{

    public bool playerHasEnteredZone = false;
    public GameObject dropPrompt;


    private GameObject gManager;
    ParcelManager pManager;

    private void Awake()
    {
        dropPrompt = GameObject.FindWithTag("DropPrompt");

        gManager = GameObject.Find("GameManager");

        pManager = gManager.GetComponent<ParcelManager>();

        
    }

    private void Start()
    {
        dropPrompt.SetActive(false);
    }

    private void Update()
    {

       

        if (playerHasEnteredZone && pManager.isPickedUp_PM)
        {
            dropPrompt.SetActive(true);
        }
        else
        {
            dropPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Parcel parcel = other.GetComponent<Parcel>();

        if(other.gameObject.CompareTag("Player"))
        {
            playerHasEnteredZone = true;
        }
       
        if (parcel != null && !parcel.isPickedUp)
        {
            parcel.Deliver();


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerHasEnteredZone = false;
        }
    }
}
