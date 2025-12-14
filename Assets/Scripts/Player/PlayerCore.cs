using System;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCore : MonoBehaviour {
  [SerializeField]
  public GameObject cubePrefab;
  public GameObject spawn;
  
  [SerializeField]
  private float fallYThreshold = -5f;
  
  public Transform CenterA { get; private set; }
  public Transform CenterB { get; private set; }
  public Rigidbody RigidBody { get; private set; }
  public MoveCuboid MoveCuboid { get; private set; }
  public GameObject CubeA { get; private set; }
  public GameObject CubeB { get; private set; }
  private MoveCube _moveCubeA;
  private MoveCube _moveCubeB;
  
  
  private bool _hasFallen;
  private bool _separated;
  private Vector3 _originalPosition;

  private void OnEnable() {
    GameEvents.PlayerDied += RestoreCuboid;
  }

  private void OnDisable() {
    GameEvents.PlayerDied -= RestoreCuboid;
  }

  private void Awake() {
    CenterA = transform.Find("CenterA");
    CenterB = transform.Find("CenterB");
    MoveCuboid = GetComponent<MoveCuboid>();
    RigidBody =  GetComponent<Rigidbody>();
    _originalPosition = transform.position;
    SetPhysicsEnabled(false);
    
    Assert.IsNotNull(CenterA, $"{nameof(PlayerCore)} on {name} requires a child named 'CenterA'.");
    Assert.IsNotNull(CenterB, $"{nameof(PlayerCore)} on {name} requires a child named 'CenterB'.");
    Assert.IsNotNull(MoveCuboid, $"{nameof(PlayerCore)} on {name} requires 'MoveCuboid' component.\"");
  }
  
  private void Update() {
    if (transform.position.y < fallYThreshold) {
      if (_hasFallen) return;
      _hasFallen = true;
      GameEvents.EmitPlayerDied();
    }

    if (CubeA && CubeB) {
      Vector3 posA = CubeA.transform.position;
      Vector3 posB = CubeB.transform.position;
      if (!MathUtils.Approximately(Vector3.Distance(posA, posB), 1.0f, 0.1f)) return;
      if (!_moveCubeA.IsIdle() || !_moveCubeB.IsIdle()) return;
      RestoreCuboid();
    }
  }
  
  public bool IsStanding() {
    return MathUtils.Approximately(CenterA.position.x, CenterB.position.x) &&
           MathUtils.Approximately(CenterA.position.z, CenterB.position.z);
  }

  public bool IsPhysicsEnabled() {
    return ! RigidBody.freezeRotation && RigidBody.useGravity;
  }

  public void SetPhysicsEnabled(bool value) {
    RigidBody.freezeRotation = !value;
    RigidBody.useGravity     = value;
  }

  public void SeparateCuboid() {
    if (_separated)  return;
    _separated         = true;
    MoveCuboid.enabled = false;
    HideCuboid();
    CreateSeparatedCubes();
    SetPhysicsEnabled(false);
    transform.position = spawn.transform.position;
  }
  
  private void CreateSeparatedCubes() {
    Vector3 spawnPos = spawn.transform.position;
    spawnPos.y = .5f;
    CubeA = Instantiate(
      cubePrefab,
      spawnPos,
      Quaternion.identity
    );
    CubeA.transform.parent = transform.parent;
    CubeA.name             = "CubeA";
    _moveCubeA              = CubeA.GetComponent<MoveCube>();

    Vector3 posB = CenterB.transform.position;
    posB.y = .5f;
    CubeB = Instantiate(
      cubePrefab,
      posB,
      Quaternion.identity
    );
    CubeB.transform.parent = transform.parent;
    CubeB.name             = "CubeB";
    _moveCubeB              = CubeB.GetComponent<MoveCube>();

    if (!_moveCubeA || !_moveCubeB) return;
    
    // Set player core reference so cubes can trigger merge
    _moveCubeA.SetPlayerCore(this);
    _moveCubeB.SetPlayerCore(this);
    // Link the cubes together so they know about each other
    _moveCubeA.SetOtherCube(_moveCubeB);
    _moveCubeB.SetOtherCube(_moveCubeA);
    // Copy rotation speed from the cuboid
    _moveCubeA.rotSpeed = MoveCuboid.rotationSpeed;
    _moveCubeB.rotSpeed = MoveCuboid.rotationSpeed;
    // Set one as active (A), one as inactive (B)
    _moveCubeA.SetActive(false);
    _moveCubeB.SetActive(true);
    
    // Emit camera target change to the active cube
    GameEvents.EmitChangeCameraTarget(CubeB.transform);
  }

  private void HideCuboid() {
    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    if (meshRenderer) {
      meshRenderer.enabled = false;
    }
    MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
    foreach (MeshRenderer childRenderer in childRenderers) {
      childRenderer.enabled = false;
    }
    Collider colliderComp = GetComponent<Collider>();
    if (colliderComp) {
      colliderComp.enabled = false;
    }
  }
  
  private void RestoreCuboid() {
    // where the cuboid should be
    if (!_separated) return;
    Vector3 posA = CubeA.transform.position;
    Vector3 posB = CubeB.transform.position;
    Vector3 center = (posA + posB) * 0.5f;

    Vector3 dir = posB - posA;
    dir.y = 0f;

    if (dir.sqrMagnitude > 0.0001f) { // avoid zero-length just in case
      var rot = Quaternion.FromToRotation(Vector3.up, dir.normalized);
      transform.SetPositionAndRotation(center, rot);
    } else {
      transform.position = center;
      transform.rotation = Quaternion.identity;
    }
    // re-enable visuals & collider, clean up cubes
    if (CubeA) Destroy(CubeA);
    if (CubeB) Destroy(CubeB);
    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    if (meshRenderer) {
      meshRenderer.enabled = true;
    }
    MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>(true);
    foreach (MeshRenderer childRenderer in childRenderers) {
      childRenderer.enabled = true;
    }
    Collider colliderComp = GetComponent<Collider>();
    if (colliderComp) {
      colliderComp.enabled = true;
    }
    _separated         = false;
    MoveCuboid.enabled = true;
    MoveCuboid.SetSpawning(true);
    
    // Emit camera target change back to the main player
    GameEvents.EmitChangeCameraTarget(transform);
  }
  
  public void ResetState() {
    _hasFallen = false;
    _separated = false;
    
    // Clean up any separated cubes
    if (CubeA) Destroy(CubeA);
    if (CubeB) Destroy(CubeB);
    CubeA = null;
    CubeB = null;
    _moveCubeA = null;
    _moveCubeB = null;
    
    // Re-enable visuals and collider
    SetVisible(true);
    Collider colliderComp = GetComponent<Collider>();
    if (colliderComp) colliderComp.enabled = true;
  }
  
  public void SetVisible(bool visible) {
    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    if (meshRenderer) meshRenderer.enabled = visible;
    MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>(true);
    foreach (MeshRenderer r in childRenderers) r.enabled = visible;
  }
}
