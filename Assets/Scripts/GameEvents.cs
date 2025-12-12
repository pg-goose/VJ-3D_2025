using System;
using UnityEngine;

public static class GameEvents
{
  public static event Action            CubeFell;
  public static event Action            GoalReached;
  public static event Action            LevelReady;
  public static event Action            PlayerDied;
  public static event Action            PressedTileO;
  public static event Action            PressedTileX;
  public static event Action<Transform> CameraTargetChanged;

  public static void EmitCubeFell() {
    CubeFell?.Invoke();
  }

  public static void EmitGoalReached() {
    GoalReached?.Invoke();
  }

  public static void EmitLevelReady() {
    LevelReady?.Invoke();
  }
  
  public static void EmitPlayerDied() {
    PlayerDied?.Invoke();
  }

  public static void EmitPressedTileO() {
    PressedTileO?.Invoke();
  }

  public static void EmitPressedTileX() {
    PressedTileX?.Invoke();
  }

  public static void EmitChangeCameraTarget(Transform newTarget) {
    Debug.Log("Emited CameraTargetChanged:  " + newTarget.position);
    CameraTargetChanged?.Invoke(newTarget);
  }
}