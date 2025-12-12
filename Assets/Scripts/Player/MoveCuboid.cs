using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCuboid : MonoBehaviour
{
  private enum State { Spawning, Idle, Rotating, Falling }
  
  [Header("Movement Settings")]
  public float rotationSpeed = 200;
  public float fallSpeed = 100;

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
  private float _remainingRotationAngle;
  private Vector3 _rotationAxis;
  private float _rotationDirection;
  private Vector3 _rotationPoint;
  private bool _rotationStartedStanding;

  // state
  private State _state = State.Spawning;
  public bool FallStraight { get; set; }


  #region Debug

  private void DrawDebugLines() {
    Debug.DrawLine(centerA.position, centerB.position);
    if (_state == State.Rotating) Debug.DrawLine(transform.position, _rotationPoint, Color.blue);
  }

  #endregion

  #region Unity

  private PlayerCore PlayerCore { get; set; }
  
  private void Awake() {
    PlayerCore = GetComponent<PlayerCore>();
    Assert.IsNotNull(PlayerCore);
    
    _collider  = GetComponent<Collider>();
    _rigidbody = GetComponent<Rigidbody>();
  }

  private void Start() {
    _moveAction = InputSystem.actions.FindAction("Move");
    _groundMask = LayerMask.GetMask("Ground");
  }

  private void Update() {
    DrawDebugLines();
    
    switch (_state) {
      case State.Falling:
        PlayerCore.SetPhysicsEnabled(true);
        if (!IsGrounded()) return;
        PlayerCore.SetPhysicsEnabled(false);
        _state = State.Idle;
        SnapToGrid();
        return;
        
      case State.Rotating:
        RotationStep();
        return;
        
      case State.Spawning:
        if (!IsGrounded()) {
          _rigidbody.useGravity     = true;
          _rigidbody.freezeRotation = true;
          return;
        }
        _rigidbody.useGravity = false;
        _rigidbody.freezeRotation = false;
        _state = State.Idle;
        SnapToGrid();
        GameEvents.EmitChangeCameraTarget(transform);
        return;
        
      case State.Idle:
        if (HandleFalling()) return;
        TryBeginMovement();
        return;
    }
  }
  
  private void TryBeginMovement() {
    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (!HasMovementInput(dir)) return;
    BeginRotation(dir);
  }
  
  public void ResetState() {
    _state = State.Spawning;
    _remainingRotationAngle = 0f;

    Rigidbody rb = GetComponent<Rigidbody>();
    if (!rb) return;
    
    rb.linearVelocity  = Vector3.zero;  
    rb.angularVelocity = Vector3.zero;
    rb.useGravity  = false; 
  }

  #endregion

  #region Positioning

  private void SnapToGrid() {
    // Local lambda that snaps a single axis
    const float snapAngle = 90f;
    Func<float, float> snapAxis = angle => {
      angle = Mathf.DeltaAngle(0f, angle);
      float snapped = Mathf.Round(angle / snapAngle) * snapAngle;
      if (Mathf.Abs(snapped) < 0.01f)
        snapped = 0f;
      return snapped;
    };  

    Vector3 euler = transform.eulerAngles;

    euler.x = snapAxis(euler.x);
    euler.y = snapAxis(euler.y);
    euler.z = snapAxis(euler.z);

    transform.rotation = Quaternion.Euler(euler);
    
    Vector3 pos = transform.position;
    pos.x = Mathf.Round(pos.x * 2.0f) / 2.0f;
    pos.y = IsStanding() ? 1.0f : 0.5f;
    pos.z = Mathf.Round(pos.z * 2.0f) / 2.0f;
    transform.position = pos;
  }

  #endregion

  public void SetSpawning(bool value) {
    _state = value ? State.Spawning : State.Idle;
  }

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

  #region Movement & Rotation

  private bool HasMovementInput(Vector2 dir) {
    return Mathf.Abs(dir.x) > 0.99f || Mathf.Abs(dir.y) > 0.99f;
  }

  private void BeginRotation(Vector2 dir) {
    _state                   = State.Rotating;
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
    float step = rotationSpeed * Time.deltaTime;

    if (step > _remainingRotationAngle)
      step = _remainingRotationAngle;

    transform.RotateAround(_rotationPoint, _rotationAxis, step * _rotationDirection);
    _remainingRotationAngle -= step;

    if (!_rotationStartedStanding && !IsGrounded()) {
      StartFalling();
      return;
    }
    if (_remainingRotationAngle <= 0f) {
      _state = State.Idle;
      SnapToGrid();
    }
  }

  private bool HandleFalling() {
    if (!IsGrounded()) {
      StartFalling();
      return true;
    }

    if (PlayerCore.IsPhysicsEnabled())
      PlayerCore.SetPhysicsEnabled(false);

    return false;
  }

  private void AdjustCenterOfMass() {
    if (!IsPointGrounded(centerA, Color.red))
      _rigidbody.centerOfMass = centerA.localPosition;

    if (! IsPointGrounded(centerB, Color.blue))
      _rigidbody.centerOfMass = centerB.localPosition;
  }

  private void StartFalling() {
    bool wasSpawning = _state == State.Spawning;
    _state = State.Falling;
    
    if (wasSpawning || FallStraight) {
      _rigidbody.useGravity     = true;
      _rigidbody.freezeRotation = true;
      return;
    }

    PlayerCore.SetPhysicsEnabled(true);

    if (_rotationAxis == Vector3.zero || Mathf.Approximately(rotationSpeed, 0f))
        return;

    float radiansPerSecond = rotationSpeed * Mathf.Deg2Rad * _rotationDirection;
    _rigidbody.angularVelocity = _rotationAxis.normalized * radiansPerSecond;

    AdjustCenterOfMass();
    if (fallSound) AudioSource.PlayClipAtPoint(fallSound, transform.position);
  }

  #endregion

  
}