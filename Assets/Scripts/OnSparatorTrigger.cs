using UnityEngine;

/// <summary>
/// Handles the separation of the cuboid player into two individual cubes
/// when stepping on a TileSeparator.
/// </summary>
[RequireComponent(typeof(MoveCuboid))]
public class OnSparatorTrigger : OnStandingTrigger
{
  [Header("Cube Prefab")]
  [SerializeField] private GameObject cubePrefab;

  [Header("References")]
  [SerializeField] private Transform centerA;
  [SerializeField] private Transform centerB;

  [Header("Audio")]
  [SerializeField] private AudioClip separationSound;

  private MoveCuboid _moveCuboid;
  private bool _isSeparated = false;
  private GameObject _cubeA;
  private GameObject _cubeB;
  private MoveCube _moveCubeA;
  private MoveCube _moveCubeB;
  private Vector3 _originalSpawnPosition;

  private void Awake() {
    _moveCuboid = GetComponent<MoveCuboid>();
    _originalSpawnPosition = transform.position;
  }
  
  public bool IsSeparated() {
    return _isSeparated;
  }
  
  public override void OnTriggerStay(Collider other) {
    if (_isSeparated) return;
    if (!StandingOnCollider(other)) return;
    if (!other.CompareTag("Separator")) return;
    
    if (!cubePrefab) {
      Debug.LogError("OnSparatorTrigger: Cube prefab not assigned!");
      return;
    }
    _isSeparated        = true;
    _moveCuboid.enabled = false;
    CreateSeparatedCubes();
    HideCuboid();
  }
  
  private void CreateSeparatedCubes() {
    Vector3 spawnPos = _originalSpawnPosition;
    spawnPos.y = .5f;
    _cubeA = Instantiate(
      cubePrefab,
      spawnPos,
      Quaternion.identity
    );
    _cubeA.transform.parent = transform.parent;
    _cubeA.name             = "CubeA";
    _moveCubeA              = _cubeA.GetComponent<MoveCube>();

    Vector3 posB = centerB.transform.position;
    posB.y = .5f;
    _cubeB = Instantiate(
      cubePrefab,
      posB,
      Quaternion.identity
    );
    _cubeB.transform.parent = transform.parent;
    _cubeB.name             = "CubeB";
    _moveCubeB              = _cubeB.GetComponent<MoveCube>();

    if (!_moveCubeA || !_moveCubeB) return;
    
    // Link the cubes together so they know about each other
    _moveCubeA.SetOtherCube(_moveCubeB);
    _moveCubeB.SetOtherCube(_moveCubeA);
    // Give cubes reference to this separator for merging
    _moveCubeA.SetPlayerSeparator(this);
    _moveCubeB.SetPlayerSeparator(this);
    // Copy rotation speed from the cuboid
    _moveCubeA.rotSpeed = _moveCuboid.rotSpeed;
    _moveCubeB.rotSpeed = _moveCuboid.rotSpeed;
    // Set one as active (A), one as inactive (B)
    _moveCubeA.SetActive(false);
    _moveCubeB.SetActive(true);
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

  public void MergeCubes() {
    Vector3 posA = _cubeA.transform.position;
    Vector3 posB = _cubeB.transform.position;
    if (MathUtils.Approximately(Vector3.Distance(posA, posB), 1.0f, 0.001f)) {
      RestoreCuboid(posA, posB);
    }
  }
  
  private void RestoreCuboid(Vector3 posA, Vector3 posB) {
    // where the cuboid should be
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
    if (_cubeA) Destroy(_cubeA);
    if (_cubeB) Destroy(_cubeB);
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
    _isSeparated        = false;
    _moveCuboid.enabled = true;
    _moveCuboid.SetSpawning(true);
  }
}
