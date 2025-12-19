using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesText;

    private void Start() {
        
        UpdateUI();
    }

    private void OnEnable() {
        
        GameEvents.PlayerMoved += UpdateUI;
    }

    private void OnDisable() {
        
        GameEvents.PlayerMoved -= UpdateUI;
    }

    private void UpdateUI() {
       
        if (GameManager.Instance == null) {
            Debug.LogError("ERROR No se encuentra GameManager");
            return;
        }

        movesText.text = $"MOVES: {GameManager.Instance.TotalMoves}";
        
        
    }
}