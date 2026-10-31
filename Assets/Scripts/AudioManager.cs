using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gManager;
    [SerializeField] private PlayerMovement pMovement;
    [SerializeField] private AudioSource walkingFootsteps;
    [SerializeField] private AudioSource sprintingFootsteps;
    private void InitialiseReferences()
    {
        GameObject gmanagerObj = GameObject.Find("GameManager");
        GameObject pMovementObj = GameObject.FindGameObjectWithTag("Player");

        gManager = gmanagerObj.GetComponent<GameManager>();
        pMovement = pMovementObj.GetComponent<PlayerMovement>();
    }

    private void ManageAudio()
    {
        // manage walking audio
        if (pMovement.isWalking)
        {
           if(!walkingFootsteps.isPlaying) walkingFootsteps.Play(); 
        }
        else walkingFootsteps.Stop();

        // manage sprinting audio
        if (pMovement.isSprinting)
        {
            if (!sprintingFootsteps.isPlaying) sprintingFootsteps.Play();
        }
        else sprintingFootsteps.Stop();
    }

    private void OnValidate()
    {
        InitialiseReferences();
    }
    private void OnAwake() 
    {
        InitialiseReferences();
    }
  

    // Update is called once per frame
    void Update()
    {
        ManageAudio();
    }
}
