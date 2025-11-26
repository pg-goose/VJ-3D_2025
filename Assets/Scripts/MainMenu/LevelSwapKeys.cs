using UnityEngine;

public class LevelSwapKeys : MonoBehaviour
{
  private readonly KeyCode[] _keys = {
    KeyCode.Alpha0, KeyCode.Keypad0,
    KeyCode.Alpha1, KeyCode.Keypad1,
    KeyCode.Alpha2, KeyCode.Keypad2,
    KeyCode.Alpha3, KeyCode.Keypad3,
    KeyCode.Alpha4, KeyCode.Keypad4,
    KeyCode.Alpha5, KeyCode.Keypad5,
    KeyCode.Alpha6, KeyCode.Keypad6,
    KeyCode.Alpha7, KeyCode.Keypad7,
    KeyCode.Alpha8, KeyCode.Keypad8,
    KeyCode.Alpha9, KeyCode.Keypad9
  };

  private void Update() {
    int? pressedNumber = null;
    foreach (KeyCode key in _keys)
      if (Input.GetKeyDown(key))
        pressedNumber = (int)key;

    if (pressedNumber.HasValue) {
      GameManager.Instance.LoadLevel(pressedNumber.Value);
      return;
    }

    if (Input.GetKeyDown(KeyCode.Escape)) GameManager.Instance.LoadMainMenu();
  }
}