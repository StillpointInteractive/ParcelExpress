using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gManager;
    [SerializeField] private PlayerMovement pMovement;
    [SerializeField] private AudioSource walkingFootsteps;
    [SerializeField] private AudioSource sprintingFootsteps;

    private PlayerControls playerControls;

    [Header("Footstep Audio Cues")]
    [SerializeField] private AudioSource walkingSource;
    [SerializeField] private AudioClip[] walkingClips;
    [SerializeField] private AudioSource jumpingSource;

    [Header("Movement time intervals")]
    [SerializeField] private float walkingStepInterval = 0.45f;
    [SerializeField] private float sprintingStepInterval = 0.25f;

    private float footstepTimer = 0f;
    private void InitialiseReferences()
    {
        GameObject gmanagerObj = GameObject.Find("GameManager");
        GameObject pMovementObj = GameObject.FindGameObjectWithTag("Player");

        gManager = gmanagerObj.GetComponent<GameManager>();
        pMovement = pMovementObj.GetComponent<PlayerMovement>();
    }

    private void ManageAudio()
    {
        // Handles the walking and running footsteps
        HandleFootsteps();

        if(playerControls.Player.Jump.WasPressedThisFrame())
        {
            PlayJumpingSound(); 
        }
    }

    private void PlayWalkingFootsteps()
    {
        int randomIndex = Random.Range(0, walkingClips.Length);

        walkingFootsteps.PlayOneShot(walkingClips[randomIndex]);
    }
    private void PlayJumpingSound()
    {
        jumpingSource.Play();
    }

    private void OnValidate()
    {
        InitialiseReferences();
    }
    private void Awake() 
    {
        InitialiseReferences();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void HandleFootsteps()
    {
        if(!pMovement.isWalking && !pMovement.isSprinting)
        {
            footstepTimer = 0f;
            return;
        }

        float stepInterval = pMovement.isSprinting ? sprintingStepInterval : walkingStepInterval;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0f)
        {
            PlayWalkingFootsteps();
            footstepTimer = stepInterval;
        }
    }

    // Update is called once per frame
    void Update()
    {
          ManageAudio();
        
    }
}
