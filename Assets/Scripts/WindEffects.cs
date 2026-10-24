

using UnityEngine;

public class WindEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem windParticles;
    private PlayerMovement pMovement;

    [Header("Wind Settings")]
    [SerializeField] private float maxEmission = 18f;
    [SerializeField] private float windSpeed = 6f;
    [SerializeField] private float transitionSpeed = 3f;

    private bool isRunning;
    private float currentIntensity;

    private void Awake()
    {
        pMovement = GetComponentInParent<PlayerMovement>();
    }
    

    private void Update()
    {
        isRunning = pMovement.isSprinting;

        float targetIntensity = isRunning ? 1f : 0f;

        currentIntensity = Mathf.MoveTowards(
            currentIntensity,
            targetIntensity,
            transitionSpeed * Time.deltaTime
        );

        var emission = windParticles.emission;
        emission.rateOverTime = maxEmission * currentIntensity;

        var velocity = windParticles.velocityOverLifetime;
        velocity.z = new ParticleSystem.MinMaxCurve(
            -windSpeed * currentIntensity
        );
    }
}
