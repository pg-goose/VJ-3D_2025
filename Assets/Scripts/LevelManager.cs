using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{
  public static LevelManager Instance { get; private set; }

  [Header("Referencias")]
  [SerializeField] private GameObject player;
  [SerializeField] private GameObject spawn;
  [SerializeField] private MapCreation mapCreation;
  [SerializeField] private GameObject mainCamera;

  private GameObject _cameraInitialTarget;
  
  // Control de niveles
  private int _currentMapIndex = 0;

  private void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance                                = this;
    mapCreation                             = FindFirstObjectByType<MapCreation>();
    _cameraInitialTarget                    = new GameObject();
    _cameraInitialTarget.transform.position = new Vector3(-3f, 0f, -6f);
  }

  private void Start() {
    LoadLevel(_currentMapIndex);
  }

  private static readonly Key[] LevelKeys = {
    Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
    Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
  };

  private void Update() {
    for (int i = 0; i < LevelKeys.Length; i++) {
      if (Keyboard.current[LevelKeys[i]].wasPressedThisFrame) {
        LoadLevelSafe(i);
        break;
      }
    }
  }

  private void OnEnable() {
    GameEvents.PlayerDied  += OnPlayerDied;
    GameEvents.GoalReached += OnGoalReached;
    GameEvents.CubeFell    += OnCubeFell;
  }

  private void OnDisable() {
    GameEvents.PlayerDied  -= OnPlayerDied;
    GameEvents.GoalReached -= OnGoalReached;
    GameEvents.CubeFell    -= OnCubeFell;
  }

  private void OnPlayerDied() {
    Invoke(nameof(RestartCurrentMap), 1.0f);
  }

  private void OnCubeFell() {
    Invoke(nameof(RestartCurrentMap), 1.0f);
  }
  
  private void LoadLevelSafe(int index) {
    Debug.Assert(mapCreation != null && mapCreation.levelMaps != null);

    if (index >= 0 && index < mapCreation.levelMaps.Length) {
      _currentMapIndex = index;
      LoadLevel(index);
      return;
    }
  }

  private void LoadLevel(int index) {
    StartCoroutine(LoadLevelSequence(index));
  }

  private IEnumerator LoadLevelSequence(int index) {
    // Hide player during map transition
    PlayerCore playerCore = player.GetComponent<PlayerCore>();
    if (playerCore) playerCore.SetVisible(false);
    
    mapCreation.UnloadMap();
    
    MapCreation.MapData mapData = mapCreation.CreateMap(index);
    spawn.transform.position = mapData.SpawnPosition;
    Physics.SyncTransforms();
    GameEvents.EmitChangeCameraTarget(_cameraInitialTarget.transform);
    
    yield return StartCoroutine(AnimateTiles(mapData.TileAnimators));
    
    SetupAndResetPlayer();
    GameEvents.EmitLevelReady();
  }

  private IEnumerator AnimateTiles(List<TileAnimator> animators) {
    if (animators == null || animators.Count == 0) yield break;
    
    int completedCount = 0;
    int totalCount = animators.Count;
    
    foreach (TileAnimator animator in animators) {
      if (animator) {
        animator.Play(() => completedCount++);
      } else {
        completedCount++;
      }
    }
    
    // Wait until all animations complete
    while (completedCount < totalCount) {
      yield return null;
    }
  }

  private void RestartCurrentMap() {
    LoadLevel(_currentMapIndex);
  }

  public void OnGoalReached() {
    if (mapCreation != null && _currentMapIndex + 1 < mapCreation.levelMaps.Length) {
      _currentMapIndex++;
      Invoke(nameof(LoadNextMapDelay), 1.0f);
      return;
    }
    GameManager.Instance.LoadCredits();
  }

  private void LoadNextMapDelay() {
    LoadLevel(_currentMapIndex);
  }
  
  private void SetupAndResetPlayer() {
    Debug.Assert(player && spawn && mainCamera);
    Vector3 spawnPos = spawn.transform.position;
    player.transform.position = spawnPos;
    player.transform.rotation = Quaternion.identity;
    
    // Camera target will be changed when player lands (see MoveCuboid.cs)

    PlayerCore playerCore = player.GetComponent<PlayerCore>();
    Debug.Assert(playerCore);
    playerCore.ResetState();

    MoveCuboid moveCuboid = player.GetComponent<MoveCuboid>();
    Debug.Assert(moveCuboid);
    moveCuboid.ResetState();
    moveCuboid.enabled = true;
  }
}