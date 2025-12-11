using UnityEngine;
using UnityEngine.Assertions;

public class TileStandingDispatcher : MonoBehaviour {
  private PlayerCore _player;
  
  private void Awake() {
    _player = GetComponent<PlayerCore>();
    Assert.IsNotNull(_player, $"{nameof(ITileStandingHandler)} requires {nameof(PlayerCore)} on the same GameObject.");
  }

  private bool OnCollider(Collider other) {
    Vector3 pos = transform.position;
    Vector3 cpos = other.transform.position;
    return MathUtils.Approximately(pos.x, cpos.x) &&
           MathUtils.Approximately(pos.z, cpos.z);
  }

  private bool StandingOnCollider(Collider other) {
    return _player.IsStanding() && OnCollider(other);
  }

  public void OnTriggerStay(Collider other) {
    if (!StandingOnCollider(other))
      return;
    
    ITileStandingHandler[] handlers = other.GetComponents<ITileStandingHandler>();
    if (handlers == null || handlers.Length == 0)
      return;
    
    foreach (ITileStandingHandler t in handlers)
      t.OnPlayerStanding(_player);
  }
}
