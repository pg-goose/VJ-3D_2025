using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCuboid : MonoBehaviour
{
  [Header("Movement Settings")]
  public float rotSpeed;
  public float fallSpeed;

  [Header("Audio")]
  public AudioClip[] sounds;
  public AudioClip fallSound;

  [Header("Balance Points")]
  public Transform centerA;
  public Transform centerB;

  private Collider _collider;
  private LayerMask _groundMask;
  private Rigidbody _rigidbody;
  private InputAction _moveAction;

  // rotation
  private bool _isRotating;
  private float _remainingRotationAngle;
  private Vector3 _rotationAxis;
  private float _rotationDirection;
  private Vector3 _rotationPoint;
  private bool _rotationStartedStanding;

  // state
  private bool _spawning = true;
  private bool _hasFallenOffWorld = false;

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

    if (transform.position.y < -10f && !_hasFallenOffWorld) {
      _hasFallenOffWorld = true;
      Debug.Log("[MoveCuboid] Cayó al vacío. Notificando reinicio...");
      
      if (LevelManager.Instance != null) {
        LevelManager.Instance.NotifyPlayerFell();
      }
      return; 
    }

    
    if (_hasFallenOffWorld) return; 


    if (_isRotating && ! _spawning) {
      RotationStep();
      return;
    }

    if (HandleFalling()) return;  

    SnapToGrid();

    if (_spawning) {
      _spawning = false;
      TryAdvanceLevel(); 
      return;
    }

    TryAdvanceLevel();

    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (! HasMovementInput(dir)) return;

    BeginRotation(dir);
  }
  

  public void ResetState() {
    Debug.Log("[MoveCuboid] ResetState llamado");
    
    _hasFallenOffWorld = false;
    _isRotating = false;
    _remainingRotationAngle = 0f;
    
    
    _spawning = true; 

    var rb = GetComponent<Rigidbody>();
    if (rb) {
        rb.linearVelocity = Vector3.zero;  
        rb.angularVelocity = Vector3.zero;
        
        rb.useGravity = false; 
        rb.isKinematic = true; 
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
    if (_isRotating) Debug.DrawLine(transform.position, _rotationPoint, Color.blue);
  }

  #endregion

  #region Physics Helpers

  private bool IsPhysicsEnabled() {
    return ! _rigidbody.freezeRotation && _rigidbody.useGravity;
  }

  private void SetPhysicsEnabled(bool value) {
    _rigidbody.freezeRotation = !value;
    _rigidbody.useGravity     = value;
  }

  #endregion

  #region Grounding & Standing

  private bool IsPointGrounded(Transform point, Color debugColor) {
    RaycastHit hit;
    float rayDistance = _collider.bounds.extents.y * 2f;
    Vector3 origin = point.position;

    Debug.DrawLine(origin, origin + Vector3.down * rayDistance, debugColor);

    return Physics.Raycast(origin, Vector3.down, out hit, rayDistance, _groundMask);
  }

  private bool IsGrounded() {
    bool centerAGrounded = IsPointGrounded(centerA, Color.red);
    bool centerBGrounded = IsPointGrounded(centerB, Color.blue);
    return centerAGrounded && centerBGrounded;
  }

  private bool IsStanding() {
    return Mathf.Abs(centerA.position.y - centerB.position.y) > 0.001f;
  }

  #endregion

  #region Goal Detection

  private bool HasReachedGoal() {
    if (! IsStanding()) return false;

    float rayDistance = _collider.bounds.extents.y * 2f;
    Transform lower = centerA.position.y < centerB.position.y ? centerA : centerB;

    RaycastHit hit;
    if (Physics.Raycast(lower.position, Vector3.down, out hit, rayDistance, _groundMask)) {
      return hit.collider.CompareTag("Goal");
    }
    return false;
  }

  private void TryAdvanceLevel() {
    if (IsPhysicsEnabled()) return;
    if (!HasReachedGoal()) return;

    if (LevelManager.Instance != null) {
      LevelManager.Instance.NotifyGoalReached();
    } else {
      Debug.LogWarning("[MoveCuboid] LevelManager.Instance es null.");
    }
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
    Vector3 center  = _collider.bounds.center;
    Vector3 extents = _collider.bounds.extents;

    if (dir.x > 0.99f)  return new Vector3(center.x + extents.x, center.y - extents.y, center.z);
    if (dir.x < -0.99f) return new Vector3(center.x - extents.x, center.y - extents.y, center.z);
    if (dir.y > 0.99f)  return new Vector3(center.x, center.y - extents.y, center.z + extents.z);
    if (dir.y < -0.99f) return new Vector3(center.x, center.y - extents.y, center.z - extents.z);

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
      TryAdvanceLevel(); // comprobar meta al terminar movimiento
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

    if (! IsPointGrounded(centerB, Color.blue))
      _rigidbody.centerOfMass = centerB.localPosition;
  }

  private void StartFalling() {
    _isRotating = false;

    
    if (_spawning) {
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false; 
        // Si es kinematicla gravedad no le afecta y se queda flotando.
        return;
    }

    SetPhysicsEnabled(true);

    if (_rotationAxis == Vector3.zero || Mathf.Approximately(rotSpeed, 0f))
        return;

    float radiansPerSecond = rotSpeed * Mathf.Deg2Rad * _rotationDirection;
    _rigidbody.angularVelocity = _rotationAxis.normalized * radiansPerSecond;

    AdjustCenterOfMass();
    if (fallSound) AudioSource.PlayClipAtPoint(fallSound, transform.position);
  }

  #endregion

  
}