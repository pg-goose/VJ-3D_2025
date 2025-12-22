using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  private const string MainMenuScene = "MainMenu";
  private const string GameScene = "Level0"; //nomes una escena unica level0
  private const string CreditsScene = "Credits";

  public static GameManager Instance { get; private set; }

  private void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  private void Start() {
      if (SceneManager.GetActiveScene().name != MainMenuScene)
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
}