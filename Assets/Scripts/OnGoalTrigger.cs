using UnityEngine;

[RequireComponent(typeof(MoveCuboid))]
public class OnGoalTrigger : OnStandingTrigger
{
  [SerializeField] private LevelOrchestrator levelOrchestrator;
  private bool _goalReached = false;
  private MoveCuboid _moveCuboid;

  private void Awake() {
    _moveCuboid = GetComponent<MoveCuboid>();
  }

  public override void OnTriggerStay(Collider other) {
    if (_goalReached) return;
    if (!StandingOnCollider(other)) return;
    if (!other.CompareTag("Goal")) return;
    
    _goalReached        = true;
    _moveCuboid.enabled = false;
    if (LevelOrchestrator.Instance) {
      // LevelOrchestrator.Instance.OnLevelComplete();
      Debug.Log("Goal reached");
      return;
    }
    Debug.LogWarning("OnGoalTrigger: LevelOrchestrator not found in scene!");
  }
}
