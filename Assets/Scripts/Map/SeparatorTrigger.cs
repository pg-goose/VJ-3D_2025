using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeparatorTrigger : MonoBehaviour, ITileHandler {
  public void OnPlayerStanding(PlayerCore player) {
    player.SeparateCuboid();
  }

  public void OnPlayerOver(PlayerCore player) {
  }
}