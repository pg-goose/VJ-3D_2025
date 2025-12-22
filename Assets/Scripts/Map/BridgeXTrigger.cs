using UnityEngine;

public class BridgeXTrigger : MonoBehaviour, ITileHandler
{
  [Header("Audio")]
  public AudioClip pressed;
  
  private AudioSource _audioSource;
  private bool _pressed;
  
  private void Awake() {
    _audioSource = GetComponent<AudioSource>();
  }
  
  private void OnTriggerExit(Collider other) {
    _pressed = false;
  }

  public void OnPlayerStanding(PlayerCore player) {
    if (_pressed) return; 
    GameEvents.EmitPressedTileX();
    _audioSource.PlayOneShot(pressed);
    _pressed = true;
  }
  public void OnPlayerOver(PlayerCore player) { }
}
