using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour, ITileHandler {
  private bool _completed;

  public void OnPlayerStanding(PlayerCore player) {
    if (_completed) return;
    _completed = true;

    // Start win animation - the cuboid will slide down into the goal
    MoveCuboid move = player.GetComponent<MoveCuboid>();
    if (move) {
      move.StartWinAnimation();
    }
  }

  public void OnPlayerOver(PlayerCore player) { }
}