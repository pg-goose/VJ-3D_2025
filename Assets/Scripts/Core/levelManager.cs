using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint; 

    [Header("Camera")]
    [SerializeField] private CameraFollow mainCamera;

    private MapCreation _mapCreation;
    private Vector3 _currentLevelSpawnPos;
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
    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) LoadLevelSafe(0);
        
        if (Keyboard.current.digit2Key.wasPressedThisFrame) LoadLevelSafe(1);
    }

    private void LoadLevelSafe(int index)
    {
        
        if (_mapCreation != null && _mapCreation.levelMaps != null)
        {
            if (index >= 0 && index < _mapCreation.levelMaps.Length)
            {
                
                _currentMapIndex = index; 
                LoadLevel(index);         
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Has pulsado la tecla {index + 1}, pero solo tienes {_mapCreation.levelMaps.Length} mapas en la lista.");
            }
        }
    }

    private void LoadLevel(int index) {
        Debug.Log($"[LevelManager] Cargando mapa índice {index}...");

        if (_mapCreation != null) {
            foreach (Transform child in _mapCreation.transform) Destroy(child.gameObject);
            
            _currentLevelSpawnPos = _mapCreation.CreateMap(index);
            
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
        Vector3 finalSpawnPos = _currentLevelSpawnPos != Vector3.zero ? _currentLevelSpawnPos : new Vector3(0, 10, 0);

        
        if (_playerInstance == null) {
            _playerInstance = GameObject.FindGameObjectWithTag("Player");
            if (!_playerInstance && playerPrefab) 
                _playerInstance = Instantiate(playerPrefab, finalSpawnPos, Quaternion.identity);
        }

        if (_playerInstance) {
            
            _playerInstance.transform.position = finalSpawnPos;
            _playerInstance.transform.rotation = Quaternion.identity;
            
            
           
            if (mainCamera != null) mainCamera.SetTarget(_playerInstance.transform, true);

            
            _playerController = _playerInstance.GetComponent<MoveCuboid>();
            if (_playerController) {
                _playerController.ResetState();
                
                
                _playerController.enabled = false;
                StartCoroutine(DropPlayerSequence());
            }
        }
    }
    private IEnumerator DropPlayerSequence()
    {
        
        yield return new WaitForSeconds(0.8f);
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
    }

    public void NotifyGoalReached() => OnLevelComplete();
    public void NotifyPlayerFell() => OnPlayerFailed();
}