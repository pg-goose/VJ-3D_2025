using System;
using UnityEngine;

/// <summary>
/// Handles the separation of the cuboid player into two individual cubes
/// when stepping on a TileSeparator.
/// </summary>
[RequireComponent(typeof(MoveCuboid))]
public class PlayerSeparator : MonoBehaviour
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

  private void OnTriggerStay(Collider other) {
    Vector3 colliderPos = other.transform.position;
    Vector3 playerPosA  = centerA.position;
    Vector3 playerPosB   = centerB.position;
      
    bool aOnTop = MathUtils.Approximately(colliderPos.x, playerPosA.x) && MathUtils.Approximately(colliderPos.z, playerPosA.z);
    bool bOnTop = MathUtils.Approximately(colliderPos.x, playerPosB.x) && MathUtils.Approximately(colliderPos.z, playerPosB.z);
    bool onTop = aOnTop || bOnTop;
    if (!onTop) return; // early exit
    if (aOnTop && bOnTop) { // cuboid is standing
      aOnTop = playerPosA.y < playerPosB.y;
      bOnTop = playerPosB.y < playerPosA.y; // unused
    }
    if (other.CompareTag("Separator") && !_isSeparated) {    
        SeparatePlayer(aOnTop);
    }
  }
  
  private void SeparatePlayer(bool aOnButton) {
    if (cubePrefab == null) {
      Debug.LogError("PlayerSeparator: Cube prefab not assigned!");
      return;
    }
    _isSeparated = true;
    _moveCuboid.enabled = false;

    CreateSeparatedCubes(aOnButton);
    HideCuboid();
  }

  private void CreateSeparatedCubes(bool aOnButton) {
    // Cube A at centerA position (current position of the active half)
    _cubeA = Instantiate(cubePrefab, centerA.position, Quaternion.identity);
    _cubeA.transform.parent = transform.parent;
    _moveCubeA = _cubeA.GetComponent<MoveCube>();

    // Cube B at original spawn position
    Vector3 spawnPos = _originalSpawnPosition;
    spawnPos.y = 0.5f; // Ensure proper height for a cube
    _cubeB = Instantiate(cubePrefab, spawnPos, Quaternion.identity);
    _cubeB.transform.parent = transform.parent;
    _moveCubeB = _cubeB.GetComponent<MoveCube>();

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
    _moveCubeA.SetActive(aOnButton);
    _moveCubeB.SetActive(!aOnButton);
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
    float distance = Vector3.Distance(posA, posB);

    if (Mathf.Abs(distance - 1.0f) < 0.1f) {
      RestoreCuboid(posA, posB);
    }
  }

  private void RestoreCuboid(Vector3 posA, Vector3 posB) {
    // 1. Compute where the cuboid should be (world space)
    Vector3 center = (posA + posB) * 0.5f;

    // 2. Compute direction of the long axis in world space
    Vector3 dir = posB - posA;
    dir.y = 0f;                       // should already be flat, but just in case

    if (dir.sqrMagnitude > 0.0001f) { // avoid zero-length just in case
      // We want the cuboid's local Y (up) to align with the vector between the cubes.
      var rot = Quaternion.FromToRotation(Vector3.up, dir.normalized);
      transform.SetPositionAndRotation(center, rot);
    } else {
      // Fallback: cubes somehow in the same spot -> just stand upright in the middle
      transform.position = center;
      transform.rotation = Quaternion.identity;
    }
    // 3. Re-enable visuals & collider, clean up cubes
    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    if (meshRenderer != null) {
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
    if (_cubeA) Destroy(_cubeA);
    if (_cubeB) Destroy(_cubeB);
    _isSeparated        = false;
    _moveCuboid.enabled = true;
  }


  public bool IsSeparated() {
    return _isSeparated;
  }
}
