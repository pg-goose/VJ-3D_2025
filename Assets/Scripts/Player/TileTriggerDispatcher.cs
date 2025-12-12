using UnityEngine;
using UnityEngine.Assertions;

public class TileTriggerDispatcher : MonoBehaviour {
  private PlayerCore _player;
  
  private void Awake() {
    _player = GetComponent<PlayerCore>();
    Assert.IsNotNull(_player, $"{nameof(ITileHandler)} requires {nameof(PlayerCore)} on the same GameObject.");
  }

  private bool OnCollider(Collider other) {
    Vector3 posC = other.transform.position;
    bool OnPosC(Vector3 center) => MathUtils.Approximately(center.x, posC.x) &&
                                   MathUtils.Approximately(center.z, posC.z);
      
    Vector3 posA = _player.CenterA.transform.position;
    if (OnPosC(posA)) return true;
    
    Vector3 posB = _player.CenterB.transform.position;
    if (OnPosC(posB)) return true;
    return false;
  }

  public void OnTriggerStay(Collider other) {
    if (!OnCollider(other)) return;
    
    ITileHandler[] handlers = other.GetComponents<ITileHandler>();
    if (handlers.Length == 0) return;

    bool standing = _player.IsStanding();
    foreach (ITileHandler t in handlers) {
        t.OnPlayerOver(_player);
        if (standing)
          t.OnPlayerStanding(_player);
    }
  }
}
