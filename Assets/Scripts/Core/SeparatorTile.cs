using UnityEngine;

/// <summary>
/// Component for separator tiles that triggers player separation.
/// Attach this to your TileSeparator prefab along with a trigger collider.
/// </summary>
public class SeparatorTile : MonoBehaviour
{
  [Header("Settings")]
  [SerializeField] private bool separateOnEnter = true;

  [Header("Audio (Optional)")]
  [SerializeField] private AudioClip activationSound;

  private bool _hasTriggered = false;

  private void OnTriggerEnter(Collider other) {
    if (_hasTriggered && separateOnEnter) return;

    // Check if the player (cuboid) entered
    PlayerSeparator separator = other.GetComponent<PlayerSeparator>();
    if (separator != null && !separator.IsSeparated()) {
      TriggerSeparation();
    }
  }

  private void OnTriggerStay(Collider other) {
    if (_hasTriggered) return;

    // Alternative: trigger on stay (when player is on top)
    PlayerSeparator separator = other.GetComponent<PlayerSeparator>();
    if (separator != null && !separator.IsSeparated()) {
      TriggerSeparation();
    }
  }

  private void TriggerSeparation() {
    _hasTriggered = true;

    if (activationSound != null) {
      AudioSource.PlayClipAtPoint(activationSound, transform.position);
    }

    // Visual feedback could be added here (e.g., change material color)
  }

  private void OnValidate() {
    // Ensure this GameObject has the "Separator" tag
    if (!gameObject.CompareTag("Separator") && !gameObject.name.Contains("Separator")) {
      Debug.LogWarning($"SeparatorTile on {gameObject.name} should have tag 'Separator' or contain 'Separator' in its name.");
    }
  }
}
