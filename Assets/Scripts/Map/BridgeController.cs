using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;


public class BridgeController : MonoBehaviour
{
  public enum BridgeType { Null, X, O }
  
  
  private GameObject _other;
  private BridgeType _type = BridgeType.Null;
  private bool _extended = true;
  private bool _isAnimating;
  
  // Computed once in Start
  private float _rotationSpeed = 300f;
  private int _rotationDir;
  private Vector3 _rotationAxis;
  private Vector3 _rotationPivot;

  public void SetOther(GameObject other) {
    _other = other;
  }
  
  public void SetType(BridgeType type) {
    _type = type;
  }

  public void Initialize() {
    UnsubscribeFromButtonEvents();
    SubscribeToButtonEvents();
  }
  
  private bool IsExtended() {
    return transform.rotation.x == 0f;
  }

  private void EnsureRetracted() {
    if (IsExtended()) OnPressedButton();
  }

  private void OnLevelReady() {
    Invoke(nameof(EnsureRetracted), .5f);
  }
  
  private void OnEnable() {
    GameEvents.LevelReady += ComputeRotationAxisAndPivot;
    GameEvents.LevelReady += OnLevelReady;
  }

  private void SubscribeToButtonEvents() {
    switch (_type) {
    case BridgeType.O:
      GameEvents.PressedTileO += OnPressedButton;
    break;
    case BridgeType.X:
      GameEvents.PressedTileX += OnPressedButton;
    break;
    case BridgeType.Null:
    break;
    default:
      throw new ArgumentOutOfRangeException();
    }
  }

  private void UnsubscribeFromButtonEvents() {
    // Unsubscribe from both to be safe (in case type changed)
    GameEvents.PressedTileO -= OnPressedButton;
    GameEvents.PressedTileX -= OnPressedButton;
  }

  private void OnDisable() {
    CancelInvoke();
    GameEvents.LevelReady -= ComputeRotationAxisAndPivot;
    GameEvents.LevelReady -= OnLevelReady;
    UnsubscribeFromButtonEvents();
  }

  private void Update() {
    // Debug.DrawRay(_rotationPivot, _rotationAxis, Color.yellow);
  }

  private void OnPressedButton() {
    if (this == null) return;
    if (_isAnimating) return;
    
    _rotationDir *= -1;
    if (_extended) {
      StartCoroutine(AnimateRotation(-180f));    // Retract: fold under
    } else {
      StartCoroutine(AnimateRotation(180f)); // Extend: unfold
    }
    
    _extended = !_extended;
  }

  private IEnumerator AnimateRotation(float totalAngle) {
    _isAnimating = true;
    float rotated = 0f;
    float targetAngle = Mathf.Abs(totalAngle);
    
    while (rotated < targetAngle) {
      float step = _rotationSpeed * Time.deltaTime;
      if (rotated + step > targetAngle) {
        step = targetAngle - rotated;
      }
      
      transform.RotateAround(_rotationPivot, _rotationAxis, step * _rotationDir);
      rotated += step;
      yield return null;
    }
    
    _isAnimating = false;
  }

  private void Start() {
    Assert.IsNotNull(_other, $"{nameof(BridgeController)} on {name} requires _other to be set.");
    Assert.AreNotEqual(_type, BridgeType.Null, $"{nameof(BridgeController)} on {name} requires type to be set.");
  }

  private void ComputeRotationAxisAndPivot() {
    if (_other == null) return;
    
    Vector3 myPos = transform.position;
    Vector3 otherPos = _other.transform.position;
    Vector3 toOther = otherPos - myPos;
    
    // manually put the extents since the original collider is scaled to avoid bugs
    var extents = new Vector3(.5f, .1f, .5f);
    
    // The rotation axis is perpendicular to the direction to other
    // The pivot is the bottom edge closest to the other half
    if (Mathf.Abs(toOther.x) > Mathf.Abs(toOther.z)) {
      // Other is along X axis
      _rotationAxis = Vector3.forward; // Rotate around Z
      int   sign  = toOther.x < 0 ? 1 : -1;
      _rotationDir = -sign;
      float edgeX = myPos.x + sign * extents.x;
      _rotationPivot = new Vector3(edgeX, myPos.y - extents.y, myPos.z);
    } else {
      // Other is along Z axis  
      _rotationAxis = Vector3.right; // Rotate around X
      int   sign  = toOther.z < 0 ? 1 : -1;
      _rotationDir = sign;
      float edgeZ = myPos.z + sign * extents.z;
      _rotationPivot = new Vector3(myPos.x, myPos.y - extents.y, edgeZ);
    }
  }
}
