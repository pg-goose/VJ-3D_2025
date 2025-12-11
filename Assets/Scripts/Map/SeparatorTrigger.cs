using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeparatorTrigger : MonoBehaviour, ITileStandingHandler {
  public void OnPlayerStanding(PlayerCore player) {
    player.SeparateCuboid();
  }
}