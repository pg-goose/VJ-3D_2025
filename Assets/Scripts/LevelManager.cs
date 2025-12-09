using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
  public static LevelManager Instance { get; private set; }

  [Header("Referencias")]
  [SerializeField]
  private GameObject playerPrefab;

  [SerializeField]
  private Transform playerSpawnPoint;

  private MapCreation _mapCreation;
  private GameObject _playerInstance;
  private MoveCuboid _playerController;

  // Control de niveles
  private int _currentMapIndex = 0;
  
  private void LoadLevelSafe(int index) {
    if (!_mapCreation || _mapCreation.levelMaps == null) return;
    if (index >= 0 && index < _mapCreation.levelMaps.Length) {
      _currentMapIndex = index;
      LoadLevel(index);
      return;
    }
    Debug.LogWarning(
      $"[LevelManager] Has pulsado la tecla {index + 1}, pero solo tienes {_mapCreation.levelMaps.Length} mapas en la lista.");
  }
  
  private void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    _mapCreation = FindFirstObjectByType<MapCreation>();
  }

  private void Start() {
    // Al empezar la escena Level0, cargamos el primer mapa 
    LoadLevel(_currentMapIndex);
  }

  private void Update() {
    if (Keyboard.current.digit1Key.wasPressedThisFrame) {
      LoadLevelSafe(0);
      return;
    }
    if (Keyboard.current.digit2Key.wasPressedThisFrame) {
      LoadLevelSafe(1);
      return;
    }
  }

  private void LoadLevel(int index) {
    Debug.Log($"[LevelManager] Cargando mapa índice {index}...");

    if (_mapCreation) {
      _mapCreation.UnloadMap();
      _mapCreation.CreateMap(index);
      Physics.SyncTransforms();
    }
    SetupAndResetPlayer();
  }

  public void OnPlayerFailed() {
    Debug.Log("Jugador cayó. Reiniciando...");
    Invoke(nameof(RestartCurrentMap), 1.5f);
  }

  private void RestartCurrentMap() {
    LoadLevel(_currentMapIndex);
  }

  public void OnLevelComplete() {
    Debug.Log("¡Nivel Completado!");
    if (_mapCreation && _currentMapIndex + 1 < _mapCreation.levelMaps.Length) {
      _currentMapIndex++;
      Invoke(nameof(LoadNextMapDelay), 1.0f);
      return;
    }
    Debug.Log("Juego terminado. Cargando créditos...");
    if (GameManager.Instance) {
      GameManager.Instance.LoadCredits();
    }
  }

  private void LoadNextMapDelay() {
    LoadLevel(_currentMapIndex);
  }

  private void SetupAndResetPlayer() {
    var spawnPos = new Vector3(0, 10, 0);
    if (playerSpawnPoint == null) {
      var obj                   = GameObject.Find("SpawnPoint");
      if (obj) playerSpawnPoint = obj.transform;
    }

    if (playerSpawnPoint) spawnPos = playerSpawnPoint.position;

    if (_playerInstance == null) {
      _playerInstance = GameObject.FindGameObjectWithTag("Player");
      if (!_playerInstance && playerPrefab)
        _playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    if (_playerInstance) {
      _playerInstance.transform.position = spawnPos;
      _playerInstance.transform.rotation = Quaternion.identity;
      _playerController                  = _playerInstance.GetComponent<MoveCuboid>();
      if (_playerController) _playerController.ResetState();
    }
  }

  public void NotifyGoalReached() => OnLevelComplete();
  public void NotifyPlayerFell()  => OnPlayerFailed();
}