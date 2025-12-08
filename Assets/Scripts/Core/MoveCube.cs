using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls movement for a single cube when the player is separated.
/// Simplified version of MoveCuboid for individual cube movement.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCube : MonoBehaviour
{
  [Header("Movement Settings")]
  public float rotSpeed = 180f;

  [Header("Audio")]
  public AudioClip[] sounds;
  public AudioClip fallSound;

  private Collider _collider;
  private Rigidbody _rigidbody;
  private LayerMask _groundMask;
  private InputAction _moveAction;
  private InputAction _switchAction;

  // Rotation
  private bool _isRotating;
  private float _remainingRotationAngle;
  private Vector3 _rotationAxis;
  private float _rotationDirection;
  private Vector3 _rotationPoint;

  // State
  private bool _isActive;
  private MoveCube _otherCube;
  private bool _spawning = true;

  #region Unity Methods

  private void Awake() {
    _collider = GetComponent<Collider>();
    _rigidbody = GetComponent<Rigidbody>();
    SetPhysicsEnabled(false);
  }

  private void Start() {
    _moveAction = InputSystem.actions.FindAction("Move");
    _switchAction = InputSystem.actions.FindAction("Switch");
    _groundMask = LayerMask.GetMask("Ground");
  }

  private void Update() {
    // Handle switching between cubes
    if (_switchAction != null && _switchAction.WasPressedThisFrame()) {
      SwitchActive();
      return;
    }

    // Only the active cube can move
    if (!_isActive) return;

    if (_isRotating && !_spawning) {
      RotationStep();
      return;
    }

    if (HandleFalling()) return;
    SnapToGrid();

    if (_spawning) {
      _spawning = false;
      return;
    }

    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (!HasMovementInput(dir)) return;

    BeginRotation(dir);
  }

  #endregion

  #region Public API

  public void SetActive(bool active) {
    _isActive = active;
  }

  public void SetOtherCube(MoveCube other) {
    _otherCube = other;
  }

  public bool IsActive() {
    return _isActive;
  }

  #endregion

  #region Switching

  private void SwitchActive() {
    if (_otherCube != null && !_isRotating && !_otherCube._isRotating) {
      _isActive = false;
      _otherCube.SetActive(true);
    }
  }

  #endregion

  #region Positioning

  private void SnapToGrid() {
    Vector3 pos = transform.position;
    pos.x = Mathf.Round(pos.x);
    pos.y = 0.5f; // Cubes are always at height 0.5
    pos.z = Mathf.Round(pos.z);
    transform.position = pos;
  }

  #endregion

  #region Physics

  private bool IsPhysicsEnabled() {
    return !_rigidbody.freezeRotation && _rigidbody.useGravity;
  }

  private void SetPhysicsEnabled(bool value) {
    _rigidbody.freezeRotation = !value;
    _rigidbody.useGravity = value;
  }

  #endregion

  #region Grounding

  private bool IsGrounded() {
    float rayDistance = _collider.bounds.extents.y * 2f;
    Vector3 origin = transform.position;

    Debug.DrawLine(origin, origin + Vector3.down * rayDistance, Color.green);

    return Physics.Raycast(
      origin,
      Vector3.down,
      rayDistance,
      _groundMask
    );
  }

  private bool HandleFalling() {
    if (!IsGrounded()) {
      StartFalling();
      return true;
    }

    if (IsPhysicsEnabled()) {
      SetPhysicsEnabled(false);
    }

    return false;
  }

  private void StartFalling() {
    _isRotating = false;

    if (_spawning) {
      _rigidbody.useGravity = true;
      return;
    }

    SetPhysicsEnabled(true);

    if (_rotationAxis != Vector3.zero && !Mathf.Approximately(rotSpeed, 0f)) {
      float radiansPerSecond = rotSpeed * Mathf.Deg2Rad * _rotationDirection;
      _rigidbody.angularVelocity = _rotationAxis.normalized * radiansPerSecond;
    }

    if (fallSound) {
      AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }
  }

  #endregion

  #region Movement

  private bool HasMovementInput(Vector2 dir) {
    return Mathf.Abs(dir.x) > 0.99f || Mathf.Abs(dir.y) > 0.99f;
  }

  private void BeginRotation(Vector2 dir) {
    _isRotating = true;
    _remainingRotationAngle = 90f;

    // Set rotation axis and direction
    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
      _rotationAxis = Vector3.forward;
      _rotationDirection = dir.x > 0 ? -1f : 1f;
    }
    else {
      _rotationAxis = Vector3.right;
      _rotationDirection = dir.y > 0 ? 1f : -1f;
    }

    _rotationPoint = GetRotationPoint(dir);
  }

  private Vector3 GetRotationPoint(Vector2 dir) {
    Vector3 center = _collider.bounds.center;
    Vector3 extents = _collider.bounds.extents;

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

    if (step > _remainingRotationAngle) {
      step = _remainingRotationAngle;
    }

    transform.RotateAround(_rotationPoint, _rotationAxis, step * _rotationDirection);
    _remainingRotationAngle -= step;

    if (!IsGrounded()) {
      StartFalling();
      return;
    }

    if (_remainingRotationAngle <= 0f) {
      _isRotating = false;
      SnapToGrid();
    }
  }

  #endregion
}