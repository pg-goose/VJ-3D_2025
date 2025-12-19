using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel; 

    private bool isPaused = false;

    void Update()
    {
        // Si pulsas ESCAPE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f; 
    }

    public void ResumeGame()
    {
        
        TogglePause();
    }

    public void GoToMainMenu()
    {
        
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }
}