using System;
using System.Numerics;
using Unity.Burst;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Controls movement for a single cube when the player is separated.
/// Simplified version of MoveCuboid for individual cube movement.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class MoveCube : MonoBehaviour
{
  private enum State
  {
    Idle,
    Rotating,
    Falling
  }

  [Header("Movement Settings")]
  public float rotSpeed = 500f;
  public bool lockUp = false;

  [Header("Audio")] public AudioClip[] sounds;
  public AudioClip fallSound;
  public AudioClip clank;
  private AudioSource _audioSource;
  private Collider _collider;
  private Rigidbody _rigidbody;
  private LayerMask _groundMask;
  private InputAction _moveAction;
  private InputAction _switchAction;

  // Rotation
  private float _remainingRotationAngle;
  private Vector3 _rotationAxis;
  private float _rotationDirection;
  private Vector3 _rotationPoint;

  // State
  private State _state = State.Idle;
  private bool _isActive;
  private bool _justActivated;
  private bool _justMoved;
  private MoveCube _otherCube;
  private PlayerCore playerCore;

  // Audio
  private int _soundIndex;

  private void Awake() {
    playerCore   = GetComponent<PlayerCore>();
    _audioSource = GetComponent<AudioSource>();
    _collider    = GetComponent<Collider>();
    _rigidbody   = GetComponent<Rigidbody>();
    SetPhysicsEnabled(false);
  }

  private void Start() {
    _moveAction   = InputSystem.actions.FindAction("Move");
    _switchAction = InputSystem.actions.FindAction("Switch");
    _groundMask   = LayerMask.GetMask("Ground");
  }

  private void Update() {
    if (_isActive && _switchAction.WasPressedThisFrame()) {
      if (_justActivated) {
        _justActivated = false;
        return;
      }

      SwitchToOtherCube();
      return;
    }

    switch (_state) {
    case State.Falling:
      playerCore.SetPhysicsEnabled(true);
      if (!_audioSource.isPlaying) PlaySound(fallSound);
      if (!IsGrounded()) return;
      playerCore.SetPhysicsEnabled(false);
      _state = State.Idle;
      SnapToGrid();
      return;
        
    case State.Rotating:
      RotationStep();
      if (_state == State.Idle)
        RotationFinished();
      return;
    
    case State.Idle:
      if (HandleFalling()) return;
      TryBeginMovement();
      return;
    }
  }
  
  public bool IsIdle() {
    return _state == State.Idle;
  }

  private void SwitchToOtherCube() {
    if (!_otherCube) return;
    _isActive = false;
    _otherCube.SetActive(true);
    GameEvents.EmitChangeCameraTarget(_otherCube.transform);
  }

  private void TryBeginMovement() {
    if (!_isActive) return;
    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (!HasMovementInput(dir)) return;
    GameEvents.EmitPlayerMoved();
    BeginRotation(dir);
  }

  public void SetActive(bool active) {
    _isActive      = active;
    _justActivated = true;
  }

  public void SetOtherCube(MoveCube other) {
    _otherCube = other;
  }

  public void SetPlayerCore(PlayerCore core) {
    playerCore = core;
  }

  public bool IsActive() {
    return _isActive;
  }
  
  private void SnapToGrid() {
    Vector3 pos = transform.position;
    pos.x              = Mathf.Round(pos.x * 2.0f) / 2.0f;
    pos.y              = 0.5f; // Cubes are always at height 0.5
    pos.z              = Mathf.Round(pos.z * 2.0f) / 2.0f;
    transform.position = pos;

    Quaternion rot = transform.rotation;
    transform.rotation = Quaternion.Euler(rot.x * 3, 0f, rot.z * 3);
  }

  private bool IsPhysicsEnabled() {
    return !_rigidbody.freezeRotation && _rigidbody.useGravity;
  }

  private void SetPhysicsEnabled(bool value) {
    _rigidbody.freezeRotation = !value;
    _rigidbody.useGravity     = value;
  }
  
  private bool IsGrounded() {
    float   rayDistance = _collider.bounds.extents.y * 2f;
    Vector3 origin      = transform.position;

    Debug.DrawLine(origin, origin + Vector3.down * rayDistance, Color.green);

    return Physics.Raycast(
      origin,
      Vector3.down,
      rayDistance,
      _groundMask
    );
  }
  
  private void RotationFinished() {
    SnapToGrid();
    PlaySound(clank);
  }

  private void PlaySound(AudioClip clip) {
    if (clip) _audioSource.PlayOneShot(clip);
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
    SetPhysicsEnabled(true);

    if (_rotationAxis != Vector3.zero && !Mathf.Approximately(rotSpeed, 0f)) {
      float radiansPerSecond = rotSpeed * Mathf.Deg2Rad * _rotationDirection;
      _rigidbody.angularVelocity = _rotationAxis.normalized * radiansPerSecond;
    }

    if (fallSound) {
      AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }

    GameEvents.EmitCubeFell();
  }
  
  private bool HasMovementInput(Vector2 dir) {
    return Mathf.Abs(dir.x) > 0.99f || Mathf.Abs(dir.y) > 0.99f;
  }

  private void BeginRotation(Vector2 dir) {
    _state                  = State.Rotating;
    _remainingRotationAngle = 90f;

    // Set rotation axis and direction
    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
      _rotationAxis      = Vector3.forward;
      _rotationDirection = dir.x > 0 ? -1f : 1f;
    }
    else {
      _rotationAxis      = Vector3.right;
      _rotationDirection = dir.y > 0 ? 1f : -1f;
    }

    _rotationPoint = GetRotationPoint(dir);
  }

  private Vector3 GetRotationPoint(Vector2 dir) {
    Vector3 center  = _collider.bounds.center;
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
      _state = State.Idle;
      SnapToGrid();
      PlayMoveSound();
    }
  }

  private void PlayMoveSound() {
    if (sounds == null || sounds.Length == 0) return;
    AudioClip clip = sounds[_soundIndex % sounds.Length];
    _soundIndex++;
    if (clip) _audioSource.PlayOneShot(clip);
  }
  
  private bool OnCollider(Collider other) {
    Vector3 posC = other.transform.position;
    bool OnPosC(Vector3 center) => MathUtils.Approximately(center.x, posC.x) &&
                                   MathUtils.Approximately(center.z, posC.z);
    Vector3 posA = transform.position;
    if (OnPosC(posA)) return true;
    return false;
  }

  public void OnTriggerStay(Collider other) {
    if (!OnCollider(other)) return;
    ITileHandler[] handlers = other.GetComponents<ITileHandler>();
    if (handlers.Length == 0) return;
    
    foreach (ITileHandler t in handlers) {
      t.OnPlayerOver(null);
    }
  }
}