using UnityEngine;

public class OnFragileTrigger : OnStandingTrigger
{
  private MoveCuboid _moveCuboid;
  private bool _triggered = false;
  private Collider _collider;

  private void Awake() {
    _moveCuboid = GetComponent<MoveCuboid>();
    _collider   = GetComponent<Collider>();
  }

  public override void OnTriggerStay(Collider other) {
    if (_triggered) return;
    if (!StandingOnCollider(other)) return;
    _moveCuboid.FallStraight = true;
    _collider.enabled        = false;
    _triggered               = true;
  }
}