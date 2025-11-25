using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCuboid : MonoBehaviour
{
  [Header("Movement Settings")]
  public float rotSpeed; // Rotation speed in degrees per second
  public float fallSpeed; // Fall speed in the Y direction (currently unused)

  [Header("Audio")] public AudioClip[] sounds; // Sounds to play when the cube rotates
  public AudioClip fallSound; // Sound to play when the cube starts falling

  [Header("Balance Points")] public Transform centerA;
  public Transform centerB;

  private Collider _collider;
  private Rigidbody _rigidbody;
  private InputAction _moveAction;
  private LayerMask _groundMask;

  // rotation
  private bool _isRotating;
  private Vector3 _rotationPoint;
  private Vector3 _rotationAxis;
  private float _remainingRotationAngle;
  private float _rotationDirection; // +1 or -1
  private bool _rotationStartedStanding;

  #region Unity

  private void Awake() {
    _collider  = GetComponent<Collider>();
    _rigidbody = GetComponent<Rigidbody>();
    SetPhysicsEnabled(false);
  }

  private void Start() {
    _moveAction = InputSystem.actions.FindAction("Move");
    _groundMask = LayerMask.GetMask("Ground");
  }

  private void Update() {
    DrawDebugLines();
    if (_isRotating) {
      RotationStep();
      return;
    }

    if (HandleFalling()) {
      return;
    }

    SnapToGrid();
    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (!HasMovementInput(dir))
      return;
    BeginRotation(dir);
  }

  #endregion

  #region Physics Helpers

  private bool IsPhysicsEnabled() {
    return !_rigidbody.freezeRotation && _rigidbody.useGravity;
  }

  private void SetPhysicsEnabled(bool value) {
    _rigidbody.freezeRotation = !value;
    _rigidbody.useGravity     = value;
  }

  #endregion

  #region Grounding & Standing

  private bool IsPointGrounded(Transform point, Color debugColor) {
    RaycastHit hit;
    float      rayDistance = _collider.bounds.extents.y * 2f;
    Vector3    origin      = point.position;

    Debug.DrawLine(origin, origin + Vector3.down * rayDistance, debugColor);

    return Physics.Raycast(
      origin,
      Vector3.down,
      out hit,
      rayDistance,
      _groundMask
    );
  }

  private bool IsGrounded() {
    bool centerAGrounded = IsPointGrounded(centerA, Color.red);
    bool centerBGrounded = IsPointGrounded(centerB, Color.blue);
    return centerAGrounded && centerBGrounded;
  }

  private bool IsStanding() {
    return Mathf.Abs(Mathf.Abs(centerA.position.y) - Mathf.Abs(centerB.position.y)) >
           0.001f;
  }

  #endregion

  #region Movement & Rotation
  
  private bool HasMovementInput(Vector2 dir) {
    return Mathf.Abs(dir.x) > 0.99f || Mathf.Abs(dir.y) > 0.99f;
  }

  private void BeginRotation(Vector2 dir) {
    _isRotating              = true;
    _remainingRotationAngle  = 90f;
    _rotationStartedStanding = IsStanding();
    
    // set rotation axis and direction
    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
      _rotationAxis      = Vector3.forward;
      _rotationDirection = dir.x > 0 ? -1f : 1f;
    } else {
      _rotationAxis      = Vector3.right;
      _rotationDirection = dir.y > 0 ? 1f : -1f;
    }
    _rotationPoint = GetRotationPoint(dir);
  }

  private Vector3 GetRotationPoint(Vector2 dir) {
    Vector3 center  = _collider.bounds.center;  // world-space center
    Vector3 extents = _collider.bounds.extents; // half-size in world units

    if (dir.x > 0.99f)
      return new Vector3(center.x + extents.x, center.y - extents.y, center.z);
    if (dir.x < -0.99f)
      return new Vector3(center.x - extents.x, center.y - extents.y, center.z);

    if (dir.y > 0.99f)
      return new Vector3(center.x, center.y - extents.y, center.z + extents.z);
    if (dir.y < -0.99f)
      return new Vector3(center.x, center.y - extents.y, center.z - extents.z);

    return center;
  }

  private void RotationStep() {
    float step = rotSpeed * Time.deltaTime;

    if (step > _remainingRotationAngle)
      step = _remainingRotationAngle;
    
    transform.RotateAround(_rotationPoint, _rotationAxis, step * _rotationDirection);
    _remainingRotationAngle -= step;
    
    if (!_rotationStartedStanding && !IsGrounded()) {
      StartFalling();
      return;
    }
    if (_remainingRotationAngle <= 0f) {
      _isRotating = false;
      SnapToGrid();
    }
  }

  private bool HandleFalling() {
    if (!IsGrounded()) {
      StartFalling();
      return true;
    }

    if (IsPhysicsEnabled())
      SetPhysicsEnabled(false);

    return false;
  }
  
  private void AdjustCenterOfMass() {
    if (!IsPointGrounded(centerA, Color.red))
      _rigidbody.centerOfMass = centerA.localPosition;

    if (!IsPointGrounded(centerB, Color.blue))
      _rigidbody.centerOfMass = centerB.localPosition;
  }

  private void StartFalling() {
    _isRotating = false;
    SetPhysicsEnabled(true);
    
    // not rotating, no direction to continue in
    if (_rotationAxis == Vector3.zero || Mathf.Approximately(rotSpeed, 0f))
      return;

    float radiansPerSecond = rotSpeed * Mathf.Deg2Rad * _rotationDirection;
    _rigidbody.angularVelocity = _rotationAxis.normalized * radiansPerSecond;

    AdjustCenterOfMass();
    if (fallSound) {
      AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }
  }

  #endregion

  #region Positioning

  private void SnapToGrid() {
    Vector3 pos = transform.position;

    pos.x = Mathf.Round(pos.x * 2.0f) / 2.0f;
    pos.y = IsStanding() ? 1.0f : 0.5f;
    pos.z = Mathf.Round(pos.z * 2.0f) / 2.0f;

    transform.position = pos;
  }

  #endregion

  #region Debug

  private void DrawDebugLines() {
    Debug.DrawLine(centerA.position, centerB.position);
    if (_isRotating) {
      Debug.DrawLine(transform.position, _rotationPoint, Color.blue);
    }
  }

  #endregion
}