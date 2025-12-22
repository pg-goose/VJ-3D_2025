using System;
using UnityEngine;

public class BridgeOTrigger : MonoBehaviour, ITileHandler
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

  public void OnPlayerStanding(PlayerCore player) { }
  public void OnPlayerOver(PlayerCore player) {
    if (_pressed) return; 
    GameEvents.EmitPressedTileO();
    _audioSource.PlayOneShot(pressed);
    _pressed = true;
  }
}
