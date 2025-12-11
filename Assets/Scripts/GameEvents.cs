using System;
using UnityEngine;

public static class GameEvents {
  public static event Action PlayerDied;
  public static event Action GoalReached;
  public static event Action<Transform> CameraTargetChanged;
  public static event Action CubeFell;

  public static void EmitPlayerDied() {
    PlayerDied?.Invoke();
  }
  
  public static void EmitGoalReached() {
    GoalReached?.Invoke();
  }
  
  public static void EmitCameraTargetChanged(Transform newTarget) {
    CameraTargetChanged?.Invoke(newTarget);
  }
  
  public static void EmitCubeFell() {
    CubeFell?.Invoke();
  }
}