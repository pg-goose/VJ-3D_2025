using Unity.Mathematics.Geometry;
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
    // Store the original spawn position when the player is created
    _originalSpawnPosition = transform.position;
  }

  private void OnTriggerStay(Collider other) {
    Vector3 colliderPos = other.transform.position;
    Vector3 playerPos   = transform.position;
      
    bool onTop = Mathf.Approximately(colliderPos.x, playerPos.x);
    onTop |= Mathf.Approximately(colliderPos.z, playerPos.z);

    // Check if we hit a separator tile and we're not already separated
    if (_isSeparated) return;

    if (other.CompareTag("Separator")) {

    
      if (onTop) {
        SeparatePlayer();
      }
    }
  }

  private void SeparatePlayer() {
    if (cubePrefab == null) {
      Debug.LogError("PlayerSeparator: Cube prefab not assigned!");
      return;
    }
    _isSeparated = true;

    // Disable the cuboid movement
    _moveCuboid.enabled = false;

    // Create two cubes at the positions of centerA and centerB
    CreateSeparatedCubes();

    // Hide or destroy the original cuboid
    HideCuboid();
  }

  private void CreateSeparatedCubes() {
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

    if (_moveCubeA != null && _moveCubeB != null) {
      // Link the cubes together so they know about each other
      _moveCubeA.SetOtherCube(_moveCubeB);
      _moveCubeB.SetOtherCube(_moveCubeA);

      // Copy rotation speed from the cuboid
      _moveCubeA.rotSpeed = _moveCuboid.rotSpeed;
      _moveCubeB.rotSpeed = _moveCuboid.rotSpeed;

      // Set one as active (A), one as inactive (B)
      _moveCubeA.SetActive(true);
      _moveCubeB.SetActive(false);
    }
  }

  private void HideCuboid() {
    // Disable rendering of the cuboid
    MeshRenderer renderer = GetComponent<MeshRenderer>();
    if (renderer != null) {
      renderer.enabled = false;
    }

    // Disable all child renderers
    MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();
    foreach (var childRenderer in childRenderers) {
      childRenderer.enabled = false;
    }

    // Disable the collider
    Collider collider = GetComponent<Collider>();
    if (collider != null) {
      collider.enabled = false;
    }
  }

  public void MergeCubes() {
    if (!_isSeparated) return;

    // Check if cubes are adjacent and can merge
    if (_cubeA != null && _cubeB != null) {
      Vector3 posA = _cubeA.transform.position;
      Vector3 posB = _cubeB.transform.position;
      float distance = Vector3.Distance(posA, posB);

      // If cubes are exactly 1 unit apart (adjacent)
      if (Mathf.Abs(distance - 1.0f) < 0.1f) {
        // Restore the cuboid
        RestoreCuboid(posA, posB);
      }
    }
  }

  private void RestoreCuboid(Vector3 posA, Vector3 posB) {
    // Calculate the center position between the two cubes
    Vector3 centerPosition = (posA + posB) / 2.0f;
    centerPosition.y = 0.5f; // Snap to proper height (lying down)

    // Move the cuboid to the center position
    transform.position = centerPosition;

    // Re-enable the cuboid
    _moveCuboid.enabled = true;

    // Re-enable rendering
    MeshRenderer renderer = GetComponent<MeshRenderer>();
    if (renderer != null) {
      renderer.enabled = true;
    }

    MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>(true);
    foreach (var childRenderer in childRenderers) {
      childRenderer.enabled = true;
    }

    // Re-enable collider
    Collider collider = GetComponent<Collider>();
    if (collider != null) {
      collider.enabled = true;
    }

    // Destroy the separated cubes
    if (_cubeA != null) Destroy(_cubeA);
    if (_cubeB != null) Destroy(_cubeB);

    _isSeparated = false;
  }

  public bool IsSeparated() {
    return _isSeparated;
  }
}
