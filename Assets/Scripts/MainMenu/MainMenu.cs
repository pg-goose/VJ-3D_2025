using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
  [SerializeField] private UIDocument mainMenuDocument;

  private const string StartGameButtonName = "start-game-button";
  private const string CreditsButtonName = "credits-button";
  
  private Button _startGameButton;
  private Button _creditsButton;
  
  private void Awake() {
    VisualElement root = mainMenuDocument.rootVisualElement;

    _startGameButton = root.Q<Button>(StartGameButtonName);
    _creditsButton = root.Q<Button>(CreditsButtonName);

    _startGameButton.clicked += StartGame;
    _creditsButton.clicked   += Credits;
  }

  private void StartGame() {
    GameManager.Instance.LoadLevel(1);
  }

  private void Credits() {
    GameManager.Instance.LoadCredits();
  }
  
}
