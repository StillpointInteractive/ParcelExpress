using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gManager;
    [SerializeField] private PlayerMovement pMovement;
    [SerializeField] private CharacterController CC;
   

    private PlayerControls playerControls;

    [Header("Footstep Audio Cues")]
    [SerializeField] private AudioSource footSteps;
    [SerializeField] private AudioClip[] footStepsClip;

    [Header("Jumping audio")]
    [SerializeField] private AudioSource jumpingSource;
    [SerializeField] private AudioSource jumpingGroan;
    [SerializeField] private AudioSource landingGroan;
    [SerializeField] private AudioSource landingSound;

    [Header("Movement time intervals")]
    [SerializeField] private float walkingStepInterval = 0.45f;
    [SerializeField] private float sprintingStepInterval = 0.25f;

    [SerializeField] private bool playLandingAudio;
    private float footstepTimer = 0f;

    [SerializeField] private bool startLandingTimer;
    [SerializeField] float landingTimer = 1f;

    private void OnValidate()
    {
        InitialiseReferences();
    }
    private void Awake()
    {
        GameObject playerObject = GameObject.Find("Player");
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
    void Update()
    {
        ManageAudio();

       if(startLandingTimer)
        {
            landingTimer -= Time.deltaTime;
            if (landingTimer <= 0f) 
            {
                landingTimer = 1f; startLandingTimer = false; 
            }
        }
    }
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

        if(!CC.isGrounded)  playLandingAudio = true;
        if (playLandingAudio && CC.isGrounded) PlayLandingSound();

        if (playerControls.Player.Jump.WasPressedThisFrame() && pMovement.isGrounded) PlayJumpingSound();

       
    }
  

   

    private void PlayWalkingFootsteps()
    {
        int randomIndex = Random.Range(0, footStepsClip.Length);

        footSteps.PlayOneShot(footStepsClip[randomIndex]);
    }
    private void PlayJumpingSound()
    {
        jumpingGroan.pitch = Random.Range(0.88f, 1.12f);
        jumpingGroan.Play();
        jumpingSource.pitch = Random.Range(0.9f, 1.1f);
        jumpingSource.Play();
        playLandingAudio = true;
    }
    
    private void PlayLandingSound()
    {
        if (landingTimer < 1f) return;

        startLandingTimer = true;
        landingGroan.pitch = Random.Range(0.95f, 1.05f);
        landingGroan.Play();
        landingSound.Play();
        playLandingAudio = false;
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

  
  
}
