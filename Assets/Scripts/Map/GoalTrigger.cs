using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour, ITileHandler {
  private bool _completed;

  public void OnPlayerStanding(PlayerCore player) {
    if (_completed) return;
    _completed = true;

    // Optional: lock movement, play animation, etc.
    MoveCuboid move = player.GetComponent<MoveCuboid>();
    if (move) {
      move.enabled = false;
      move.FallStraight = true;
    }
    GameEvents.EmitGoalReached();
  }

  public void OnPlayerOver(PlayerCore player) { }
}