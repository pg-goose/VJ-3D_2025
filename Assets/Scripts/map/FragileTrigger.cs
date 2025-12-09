using UnityEngine;

public class FragileTrigger : MonoBehaviour  {
  private Collider _collider;
  private bool _broken = false;

  private void Awake() {
    _collider = GetComponent<Collider>();
  }
  
  public void OnTriggerStay(Collider other) {}
}