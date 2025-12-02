using UnityEngine;

/// <summary>
/// Helper component to visually distinguish separated cubes in the scene.
/// Attach to cube prefabs to add visual feedback for active/inactive states.
/// </summary>
public class CubeVisualFeedback : MonoBehaviour
{
  [Header("Visual Settings")]
  [SerializeField] private Color activeColor = Color.white;
  [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  [SerializeField] private float emissionIntensity = 0.5f;

  [Header("References")]
  [SerializeField] private MeshRenderer cubeRenderer;

  private MoveCube _moveCube;
  private Material _material;
  private Color _originalColor;

  private void Awake() {
    _moveCube = GetComponent<MoveCube>();

    if (cubeRenderer == null) {
      cubeRenderer = GetComponent<MeshRenderer>();
    }

    if (cubeRenderer != null) {
      // Create a unique material instance to avoid affecting the prefab
      _material = cubeRenderer.material;
      _originalColor = _material.color;
    }
  }

  private void Update() {
    if (_moveCube != null && _material != null) {
      UpdateVisuals();
    }
  }

  private void UpdateVisuals() {
    // Change color/brightness based on active state
    Color targetColor = _moveCube.IsActive() ? activeColor : inactiveColor;
    _material.color = Color.Lerp(_material.color, targetColor, Time.deltaTime * 10f);

    // Optional: Add emission for active cube
    if (_material.HasProperty("_EmissionColor")) {
      Color emissionColor = _moveCube.IsActive() 
        ? activeColor * emissionIntensity 
        : Color.black;
      _material.SetColor("_EmissionColor", emissionColor);
    }
  }

  private void OnDestroy() {
    // Clean up material instance
    if (_material != null) {
      Destroy(_material);
    }
  }
}
