using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; 

    [Header("Settings")]
    public float smoothSpeed = 0.125f; 
    public Vector3 offset; 

    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        
        if (target != null)
            offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        
        Vector3 desiredPosition = target.position + offset;
        
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        transform.position = smoothedPosition;
    }
}