using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // La distancia ideal

    private bool _offsetCalculated = false;

    private void Start()
    {
        
        if (target != null)
        {
            CalculateOffset();
        }
    }

    public void SetTarget(Transform newTarget, bool snapToPosition = false)
    {
        target = newTarget;

        
        if (!_offsetCalculated && target != null)
        {
            CalculateOffset();
        }

        
        if (snapToPosition && target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void CalculateOffset()
    {
        offset = transform.position - target.position;
        _offsetCalculated = true;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}