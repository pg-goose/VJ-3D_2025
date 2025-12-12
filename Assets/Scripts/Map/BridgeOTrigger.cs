using System;
using UnityEngine;

public class BridgeOTrigger : MonoBehaviour, ITileHandler
{
  private bool _pressed;
  private void OnTriggerExit(Collider other) {
    _pressed = false;
  }

  public void OnPlayerStanding(PlayerCore player) { }
  public void OnPlayerOver(PlayerCore player) {
    if (_pressed) return; 
    GameEvents.EmitPressedTileO();
    _pressed = true;
  }
}
