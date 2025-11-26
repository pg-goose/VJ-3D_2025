using UnityEngine;

public class Spawn : MonoBehaviour
{
  public GameObject player; // player prefab

  private void Start() {
    GameObject obj = Instantiate(player, transform.position, transform.rotation);
    obj.transform.parent = transform;
  }
}