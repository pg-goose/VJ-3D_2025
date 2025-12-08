using UnityEngine;

public class FragileTrigger : OnStandingTrigger  {
  private Collider _collider;
  private bool _broken = false;

  private void Awake() {
    _collider = GetComponent<Collider>();
  }
  
  public override void OnTriggerStay(Collider other) {
    if (_broken) return;
    if  (!StandingOnCollider(other)) return;
    if (!other.CompareTag("Player")) return;
    _broken           = true;
    _collider.enabled = false;
  }
}