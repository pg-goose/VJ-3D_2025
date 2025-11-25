using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class MoveCuboid : MonoBehaviour
{
  private Collider _collider;

  private InputAction
    _moveAction; // Input action to capture player movement (WASD + cursor keys)

  private bool _bMoving;  // Is the object in the middle of moving?
  private bool _bFalling; // Is the object falling?

  public float rotSpeed;  // Rotation speed in degrees per second
  public float fallSpeed; // Fall speed in the Y direction

  private Vector3
    _rotPoint,
    _rotAxis; // Rotation movement is performed around the line formed by rotPoint and rotAxis

  private float
    _rotRemainder; // The angle that the cube still has to rotate before the current movement is completed

  private float
    _rotDir; // Has rotRemainder to be applied in the positive or negative direction?

  private LayerMask _layerMask; // LayerMask to detect raycast hits with ground tiles only

  public AudioClip[] sounds;  // Sounds to play when the cube rotates
  public AudioClip fallSound; // Sound to play when the cube starts falling

  public Transform centerA, centerB;

  private bool IsGrounded() {
    RaycastHit hit;
    float      rayDistance = _collider.bounds.extents.y;
    Debug.DrawLine(transform.position, transform.position + Vector3.down * rayDistance,
                   Color.red);

    return Physics.Raycast(
      transform.position,
      Vector3.down,
      out hit,
      rayDistance + 0.1f,
      _layerMask
    );
  }

  private bool IsStanding() {
    return Mathf.Abs(Mathf.Abs(centerA.position.y) - Mathf.Abs(centerB.position.y)) > 0.001f;
  }
  
  private Vector3 GetRotPoint(Vector2 dir) {
    Vector3 center  = _collider.bounds.center;  // world-space center
    Vector3 extents = _collider.bounds.extents; // half-size in world units

    switch (dir.x) {
    case > 0.99f:
      return new Vector3(center.x + extents.x, center.y - extents.y, center.z);
    case < -0.99f:
      return new Vector3(center.x - extents.x, center.y - extents.y, center.z);
    }

    switch (dir.y) {
    case > 0.99f:
      return new Vector3(center.x, center.y - extents.y, center.z + extents.z);
    case < -0.99f:
      return new Vector3(center.x, center.y - extents.y, center.z - extents.z);
    default:
      return center;
    }
  }

  private void ClampPosition() {
    Vector3 pos = transform.position;
    pos.x = Mathf.Round(pos.x * 2.0f) / 2.0f;
    pos.y = IsStanding() ? 1.0f : 0.5f;
    pos.z = Mathf.Round(pos.z * 2.0f) / 2.0f;
    transform.position = pos;
  }

  private void Awake() {
    _collider = GetComponent<Collider>();
  }

  private void Start() {
    _moveAction = InputSystem.actions.FindAction("Move");
    _layerMask  = LayerMask.GetMask("Ground");
  }

  private void Update() {
    Debug.DrawLine(centerA.position, centerB.position);
    Debug.DrawLine(transform.position, _rotPoint, Color.blue);

    if (_bMoving) {
      float amount = rotSpeed * Time.deltaTime;
      if (amount > _rotRemainder) {
        amount   = _rotRemainder;
        _bMoving = false;
      }

      transform.RotateAround(_rotPoint, _rotAxis, amount * _rotDir);
      _rotRemainder -= amount;
      if (!_bMoving) ClampPosition();
      return;
    }

    if (!IsGrounded()) {
      transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime), Space.World);
      return;
    }
    ClampPosition();

    Vector2 dir = _moveAction.ReadValue<Vector2>();
    if (!(Math.Abs(dir.x) > 0.99) && !(Math.Abs(dir.y) > 0.99)) return;

    _bMoving      = true;
    _rotRemainder = 90.0f;

    // Decide axis + direction
    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) {
      _rotAxis = Vector3.forward;
      _rotDir  = dir.x > 0 ? -1f : 1f;
    }
    else {
      _rotAxis = Vector3.right;
      _rotDir  = dir.y > 0 ? 1f : -1f;
    }
    _rotPoint = GetRotPoint(dir);
  }
}