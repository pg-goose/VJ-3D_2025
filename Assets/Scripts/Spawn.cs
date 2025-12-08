using UnityEngine;

public class Spawn : MonoBehaviour
{
  public GameObject player; // player prefab

  private void OnEnable() {
    SpawnPlayer();
  }

  private void SpawnPlayer() {
    if (player != null) {
      GameObject obj = Instantiate(player, transform.position, transform.rotation);
      obj.transform.parent = transform;
    }
  }
}