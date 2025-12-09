using UnityEngine;

public class OnStandingTrigger : MonoBehaviour
{
  private GameObject _centerA;
  private GameObject _centerB;

  private void Start() {
    _centerA = GameObject.Find("CenterA");
    _centerB = GameObject.Find("CenterB");
  }

  protected bool OnCollider(Collider other) {
    Vector3 colliderPos = other.transform.position;
    Vector3 playerPosA  = _centerA.transform.position;
    Vector3 playerPosB  = _centerB.transform.position;
    return MathUtils.Approximately(colliderPos.x, playerPosA.x) &&
           MathUtils.Approximately(colliderPos.z, playerPosA.z) ||
           MathUtils.Approximately(colliderPos.x, playerPosB.x) &&
           MathUtils.Approximately(colliderPos.z, playerPosB.z);
  }

  protected bool Standing() {
    Vector3 playerPosA = _centerA.transform.position;
    Vector3 playerPosB = _centerB.transform.position;
    return MathUtils.Approximately(playerPosA.x, playerPosB.x) &&
           MathUtils.Approximately(playerPosA.z, playerPosB.z);
  }

  protected bool StandingOnCollider(Collider other) {
    return Standing() && OnCollider(other);
  }

  public virtual void OnTriggerStay(Collider other) {
    throw new System.NotImplementedException();
  }
}
