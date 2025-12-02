using UnityEngine;

/// <summary>
/// Detects when the player reaches the goal and triggers the level completion sequence.
/// Attach this script to goal tiles in your level.
/// </summary>
public class GoalTrigger : MonoBehaviour
{
  [Header("Settings")]
  [SerializeField] private int nextLevelNumber = 2;
  [SerializeField] private bool returnToMainMenu = false;

  [Header("Audio (Optional)")]
  [SerializeField] private AudioClip victorySound;

  private bool _goalReached = false;

  private void OnTriggerEnter(Collider other) {
    // Check if the player has reached the goal
    if (_goalReached) return;

    if (other.CompareTag("Player")) {
      OnGoalReached();
    }
  }

  private void OnGoalReached() {
    _goalReached = true;

    // Play victory sound if available
    if (victorySound != null) {
      AudioSource.PlayClipAtPoint(victorySound, transform.position);
    }

    // Notify the orchestrator
    if (returnToMainMenu) {
      // Return to main menu
      if (GameManager.Instance != null) {
        GameManager.Instance.LoadMainMenu();
      }
    }
    else {
      // Load next level through orchestrator
      if (LevelOrchestrator.Instance != null) {
        LevelOrchestrator.Instance.OnLevelComplete(nextLevelNumber);
      }
      else {
        Debug.LogWarning("GoalTrigger: LevelOrchestrator not found in scene!");
        // Fallback: load level directly
        if (GameManager.Instance != null) {
          GameManager.Instance.LoadLevel(nextLevelNumber);
        }
      }
    }
  }
}
