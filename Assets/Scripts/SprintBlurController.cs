
using UnityEngine;

public class SprintBlurController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material sprintBlurMaterial;
   

    [Header("Blur Settings")]
    [SerializeField, Range(0f, 1f)]
    private float maxBlurStrength = 0.6f;

    [SerializeField] private float fadeInSpeed = 2f;
    [SerializeField] private float fadeOutSpeed = 3f;

    private float currentBlurStrength;
    private bool isSprinting;

    private static readonly int BlurStrengthID =
        Shader.PropertyToID("_BlurStrength");
    private void Start()
    {
        currentBlurStrength = 0f;

        if (sprintBlurMaterial != null)
            sprintBlurMaterial.SetFloat(BlurStrengthID, 0f);
    }
    private void Update()
    {
        if (sprintBlurMaterial == null)
            return;

        float targetStrength =
            isSprinting ? maxBlurStrength : 0f;

        float fadeSpeed =
            isSprinting ? fadeInSpeed : fadeOutSpeed;

        currentBlurStrength = Mathf.MoveTowards(
            currentBlurStrength,
            targetStrength,
            fadeSpeed * Time.deltaTime
        );

        sprintBlurMaterial.SetFloat(
            BlurStrengthID,
            currentBlurStrength
        );

      
       
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
       
    }

    private void OnDisable()
    {
        if (sprintBlurMaterial != null)
            sprintBlurMaterial.SetFloat(BlurStrengthID, 0f);
    }
   
}
