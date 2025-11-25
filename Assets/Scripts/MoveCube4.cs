using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCuboid : MonoBehaviour
{
  private float _timeSinceLastMove = 0f;
  private Collider _collider;
  private Rigidbody _rigidbody;
  private bool _bFallingWithPhysics;

  private InputAction _moveAction;

  private bool _bMoving;
  private bool _bFalling;

  public float rotSpeed;
  public float fallSpeed;

  private Vector3 _rotPoint, _rotAxis;
  private float _rotRemainder;
  private float _rotDir;

  private LayerMask _layerMask;

  public AudioClip[] sounds;
  public AudioClip fallSound;

  public Transform centerA, centerB;
  
  private bool IsPartiallyOffEdge() {
    if (_bMoving) return false; // CRÍTICO: No verificar durante el movimiento
    if (_bFallingWithPhysics) return false; // AÑADIDO: No verificar si ya está cayendo

    bool centerAGrounded = CheckCenterGrounded(centerA.position);
    bool centerBGrounded = CheckCenterGrounded(centerB.position);
  
    if (centerAGrounded != centerBGrounded) {
      if (IsStanding()) {
        Vector3 lowerCenter = centerA.position.y < centerB.position.y ? centerA.position : centerB.position;
        return !CheckCenterGrounded(lowerCenter);
      }
      return true;
    }

    return false;
  }
  
  private bool CheckCenterGrounded(Vector3 position) {
    RaycastHit hit;
    float rayDistance = IsStanding() ? 1.5f : 0.6f;
  
    Debug.DrawLine(position, position + Vector3.down * rayDistance, Color.yellow);
  
    return Physics.Raycast(
      position,
      Vector3.down,
      out hit,
      rayDistance,
      _layerMask
    );
  }

  private bool IsGrounded() {
    if (_bFallingWithPhysics) return false; // AÑADIDO: No verificar si está cayendo con física
    
    RaycastHit hit;
    float rayDistance = _collider.bounds.extents.y;
    Debug.DrawLine(transform.position, transform.position + Vector3.down * rayDistance, Color.red);

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
    Vector3 center  = _collider.bounds.center;
    Vector3 extents = _collider.bounds.extents;

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
    _rigidbody = GetComponent<Rigidbody>(); // CORREGIDO: Faltaba esta línea
    
    // Configurar rigidbody
    _rigidbody.isKinematic = true;
    _rigidbody.useGravity = false;
    _rigidbody.interpolation = RigidbodyInterpolation.Interpolate; // Suavizar movimiento
    
    // Sin constraints por ahora, para que caiga libremente
  }

  private void Start() {
    _moveAction = InputSystem.actions.FindAction("Move");
    _layerMask  = LayerMask.GetMask("Ground");
  }

  private void StartPhysicsFall() {
    if (_bFallingWithPhysics) return;
  
    _bFallingWithPhysics = true;
    _bMoving = false;
  
    // Activar física
    _rigidbody.isKinematic = false;
    _rigidbody.useGravity = true;
  
    // Aplicar torque para que rote al caer
    Vector3 fallDirection = GetFallDirection();
    _rigidbody.AddTorque(fallDirection * 3f, ForceMode.Impulse);
  
    Debug.Log("Cube falling with physics!");
    
    // Opcional: Reiniciar después de 3 segundos
    // Invoke(nameof(ResetCube), 3f);
  }

  private Vector3 GetFallDirection() {
    bool centerAGrounded = CheckCenterGrounded(centerA.position);
  
    if (!centerAGrounded) {
      Vector3 dirToA = (centerA.position - centerB.position).normalized;
      return Vector3.Cross(dirToA, Vector3.up);
    } else {
      Vector3 dirToB = (centerB.position - centerA.position).normalized;
      return Vector3.Cross(dirToB, Vector3.up);
    }
  }
  
  private void Update() {
    if (_bFallingWithPhysics) return;
    
    Debug.DrawLine(centerA.position, centerB.position);
    Debug.DrawLine(transform.position, _rotPoint, Color.blue);
    
    if (_bMoving) {
      _timeSinceLastMove = 0f;
      
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
    _timeSinceLastMove += Time.deltaTime;

    if (_timeSinceLastMove > 0.1f && IsPartiallyOffEdge()) {
      StartPhysicsFall(); 
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