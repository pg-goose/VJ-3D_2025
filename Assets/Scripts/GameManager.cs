using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    
    private const string MainMenuScene = "MainMenu";
    private const string GameScene = "Level0"; 
    private const string CreditsScene = "Credits";

    
    public int TotalMoves { get; private set; } = 0; 

    public static GameManager Instance { get; private set; }

    
    private void OnEnable() {
        GameEvents.PlayerMoved += OnPlayerMoved; 
        SceneManager.sceneLoaded += OnSceneLoaded; 
    }

    private void OnDisable() {
        GameEvents.PlayerMoved -= OnPlayerMoved; 
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnPlayerMoved() {
        TotalMoves++;
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        if (SceneManager.GetActiveScene().name != MainMenuScene && 
            !SceneManager.GetActiveScene().name.StartsWith("Level")) 
            LoadMainMenu();
    }
    
    public void StartGame() {
        SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene(MainMenuScene, LoadSceneMode.Single);
    }

    public void LoadCredits() {
        SceneManager.LoadScene(CreditsScene, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name.StartsWith("Level")) {
            Debug.Log($"[GameManager] Nivel cargado: {scene.name}");
        }
    }
}