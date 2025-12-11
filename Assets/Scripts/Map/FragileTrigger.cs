using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FragileTrigger : MonoBehaviour, ITileStandingHandler  {
  private bool _broken;

  public void OnPlayerStanding(PlayerCore player) {
    if (_broken) return;
    _broken = true;
    player.GetComponent<Collider>().enabled = false;
    player.GetComponent<MoveCuboid>().FallStraight = true;
  }
}