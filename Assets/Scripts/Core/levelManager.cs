using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint; 

    private MapCreation _mapCreation;
    private GameObject _playerInstance;
    private MoveCuboid _playerController;

    // Control de niveles
    private int _currentMapIndex = 0; 

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        
        _mapCreation = FindFirstObjectByType<MapCreation>();
    }

    private void Start() {
        // Al empezar la escena Level0, cargamos el primer mapa 
        LoadLevel(_currentMapIndex);
    }

    private void LoadLevel(int index) {
        Debug.Log($"[LevelManager] Cargando mapa índice {index}...");

        
        if (_mapCreation != null) {
            foreach (Transform child in _mapCreation.transform) Destroy(child.gameObject);
            
            
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
        
        
        if (_mapCreation != null && _currentMapIndex + 1 < _mapCreation.levelMaps.Length) {
            _currentMapIndex++; 
            Invoke(nameof(LoadNextMapDelay), 1.0f);
        } else {
            
            Debug.Log("Juego terminado. Cargando créditos...");
            if (GameManager.Instance != null) {
                GameManager.Instance.LoadCredits();
            }
        }
    }

    private void LoadNextMapDelay() {
        LoadLevel(_currentMapIndex);
    }

    private void SetupAndResetPlayer() {
        Vector3 spawnPos = new Vector3(0, 10, 0);
        if (playerSpawnPoint == null) {
            var obj = GameObject.Find("SpawnPoint");
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
            _playerController = _playerInstance.GetComponent<MoveCuboid>();
            if (_playerController) _playerController.ResetState();
        }
    }

    public void NotifyGoalReached() => OnLevelComplete();
    public void NotifyPlayerFell() => OnPlayerFailed();
}