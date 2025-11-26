using UnityEngine;
using System.Collections;


public class TileAnimator : MonoBehaviour
{
  // Encara nose com pero hem de fer les coses en ordre
  // 1. crear el mapa
  // 2. animar els tiles
  // 3. posar al jugador
  // aixi que suposo que aixó será necessari:
  
  // flag per saber que la animació s'acaba
  public bool AnimationFinished { get; private set; }

  // funcio que es crida desde altres scripts (els que organitzen)
  public void StartAnimation() {
    // ho fem en paral·lel per no parar el joc fins que acabi la animacio
    StartCoroutine(PlayAnimation());
  }

  // funcio que fa la animacio
  private IEnumerator PlayAnimation() {
    AnimationFinished = false;
    // codi que cambia la posicio, rotacio, etc... aka fa la animacio
    AnimationFinished = true;
    return null;
  }
}

