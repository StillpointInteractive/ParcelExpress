using Unity.VisualScripting;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private float maxFallVelocity = -30f;

    public Transform respawnPoint;

    private GameObject player;
    private PlayerMovement playerMovement;

    [SerializeField] private GameObject respawnPointObj;

    private void Awake()
    {
        player = GameObject.Find("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       

        if (playerMovement.verticalVelocity <= maxFallVelocity && respawnPoint != null)
        {
            Debug.Log("RESPAWNING!");
            RespawnPlayer();
        }
      
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
           if(other.gameObject.CompareTag("RespawnPoint"))
            {
                respawnPointObj = other.GameObject();
                respawnPoint = respawnPointObj.transform;
            }
        }
    }

    private void RespawnPlayer()
    {
        player.transform.position = respawnPoint.transform.position;

    }
}
