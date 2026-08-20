using UnityEngine;

public class ParcelHoldBob : MonoBehaviour
{
    [SerializeField] private HeadBob headBob;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        Vector3 bob = headBob.CurrentBobOffset;

        transform.localPosition = startPosition - bob;
    }
}