using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
  private const string MainMenu = "MainMenu";
  private const string Credits = "Credits";
  private const string Level = "Level";

  public static GameManager Instance     { get; private set; }
  public        int         CurrentLevel { get; private set; } = 0;

  private int _nbLevels = 9;

  // Awake is called when an enabled script instance is being loaded.
  private void Awake() {
    if (Instance != null && Instance != this) {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  public void Start() {
    LoadMainMenu();
  }

  public void LoadMainMenu() {
    SceneManager.LoadScene(MainMenu, LoadSceneMode.Single);
  }

  private string LevelName(int level) {
    return Level + "${level}";
  }
  
  public void LoadLevel(int level) {
    CurrentLevel = level;
    SceneManager.LoadScene(LevelName(level), LoadSceneMode.Single);
  }

  public void LoadCredits() {
    SceneManager.LoadScene(Credits, LoadSceneMode.Single);
  }
}