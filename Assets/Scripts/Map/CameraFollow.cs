using UnityEngine;

public class CameraFollow : MonoBehaviour
{
  [Header("Target")]
  public Transform target;

  [Header("Settings")] [Tooltip("How fast the camera follows (higher = snappier).")]
  public float smoothSpeed = 5f;

  [Tooltip("Desired distance from the camera to the player.")]
  public float distance = 10f;

  private void OnEnable() {
    GameEvents.CameraTargetChanged += OnCameraTargetChanged;
  }

  private void OnDisable() {
    GameEvents.CameraTargetChanged -= OnCameraTargetChanged;
  }

  private void OnCameraTargetChanged(Transform newTarget) {
    SetTarget(newTarget, false);
  }

  public void SetTarget(Transform newTarget, bool snapToPosition = false,
                        bool recalcDirectionFromScene = true) {
    target = newTarget;
    if (!target) return;

    if (snapToPosition) {
      transform.position = target.position + (-transform.forward * distance);
    }
  }

  private void LateUpdate() {
    if (!target) return;
    Vector3 desiredPosition = target.position + (-transform.forward * distance);
    
    var smoothedPosition = Vector3.Lerp(
      transform.position,
      desiredPosition,
      smoothSpeed * Time.deltaTime
    );
    
    transform.position = smoothedPosition;
  }
}