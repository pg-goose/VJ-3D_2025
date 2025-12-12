using UnityEngine;

public class BridgeXTrigger : MonoBehaviour, ITileHandler
{
  private bool _pressed;
  private void OnTriggerExit(Collider other) {
    _pressed = false;
  }

  public void OnPlayerStanding(PlayerCore player) {
    if (_pressed) return; 
    GameEvents.EmitPressedTileX();
    _pressed = true;
  }
  public void OnPlayerOver(PlayerCore player) { }
}
