using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates the level lifecycle: map creation, tile animations, player spawning,
/// and level transitions.
/// </summary>
public class LevelOrchestrator : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private MapCreation mapCreation;
  [SerializeField] private Spawn playerSpawn;

  [Header("Animation Settings")]
  [SerializeField] private float delayBeforePlayerSpawn = 0.5f;
  [SerializeField] private float delayBeforeTransition = 1.0f;

  private List<TileAnimator> _tileAnimators = new List<TileAnimator>();
  private GameObject _currentPlayer;
  private bool _isTransitioning;

  public static LevelOrchestrator Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  private void Start() {
    StartCoroutine(LevelIntroSequence());
  }

  #region Level Intro

  /// <summary>
  /// Executes the level introduction sequence:
  /// 1. Create map tiles
  /// 2. Animate tiles in
  /// 3. Spawn player
  /// </summary>
  private IEnumerator LevelIntroSequence() {
    _isTransitioning = true;

    // Step 1: Create the map tiles
    yield return StartCoroutine(CreateMap());

    // Step 2: Animate tiles in
    yield return StartCoroutine(AnimateTilesIn());

    // Step 3: Wait a bit before spawning player
    yield return new WaitForSeconds(delayBeforePlayerSpawn);

    // Step 4: Spawn the player
    SpawnPlayer();

    _isTransitioning = false;
  }

  private IEnumerator CreateMap() {
    if (mapCreation != null) {
      // MapCreation creates tiles in its Start(), so we need to ensure it runs
      // If it hasn't run yet, wait a frame
      yield return null;

      // Collect all TileAnimator components from created tiles
      CollectTileAnimators();
    }
    else {
      Debug.LogWarning("LevelOrchestrator: MapCreation reference not set!");
    }
  }

  private void CollectTileAnimators() {
    _tileAnimators.Clear();
    
    // Find all TileAnimator components that are children of the MapCreation object
    if (mapCreation != null) {
      TileAnimator[] animators = mapCreation.GetComponentsInChildren<TileAnimator>();
      _tileAnimators.AddRange(animators);
    }
  }

  private IEnumerator AnimateTilesIn() {
    if (_tileAnimators.Count == 0) {
      yield break;
    }

    // Start all tile animations
    foreach (TileAnimator animator in _tileAnimators) {
      if (animator != null) {
        animator.StartAnimation();
      }
    }

    // Wait for all animations to finish
    yield return StartCoroutine(WaitForTileAnimations());
  }

  private IEnumerator WaitForTileAnimations() {
    bool allFinished = false;
    
    while (!allFinished) {
      allFinished = true;
      
      foreach (TileAnimator animator in _tileAnimators) {
        if (animator != null && !animator.AnimationFinished) {
          allFinished = false;
          break;
        }
      }
      
      yield return null;
    }
  }

  private void SpawnPlayer() {
    if (playerSpawn != null) {
      // The Spawn script will instantiate the player
      // We could capture the reference if needed
      playerSpawn.enabled = true;
    }
    else {
      Debug.LogWarning("LevelOrchestrator: PlayerSpawn reference not set!");
    }
  }

  #endregion

  #region Level Outro

  /// <summary>
  /// Call this when the player wins the level to trigger the outro sequence.
  /// </summary>
  public void OnLevelComplete(int nextLevel) {
    if (_isTransitioning) return;
    
    StartCoroutine(LevelOutroSequence(nextLevel));
  }

  /// <summary>
  /// Executes the level outro sequence:
  /// 1. Despawn player
  /// 2. Animate tiles out
  /// 3. Unload current level
  /// 4. Load next level
  /// </summary>
  private IEnumerator LevelOutroSequence(int nextLevel) {
    _isTransitioning = true;

    // Step 1: Despawn player
    DespawnPlayer();

    // Step 2: Wait a bit
    yield return new WaitForSeconds(delayBeforeTransition);

    // Step 3: Animate tiles down
    yield return StartCoroutine(AnimateTilesOut());

    // Step 4: Load next level
    LoadNextLevel(nextLevel);

    _isTransitioning = false;
  }

  private void DespawnPlayer() {
    // Find the player in the scene
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    
    if (player != null) {
      Destroy(player);
    }
  }

  private IEnumerator AnimateTilesOut() {
    if (_tileAnimators.Count == 0) {
      yield break;
    }

    // Start all tile reverse animations
    // Note: TileAnimator would need a method for reverse animation
    // For now, we assume StartAnimation() can handle both directions
    foreach (TileAnimator animator in _tileAnimators) {
      if (animator != null) {
        animator.StartAnimation();
      }
    }

    // Wait for all animations to finish
    yield return StartCoroutine(WaitForTileAnimations());
  }

  private void LoadNextLevel(int nextLevel) {
    if (GameManager.Instance != null) {
      GameManager.Instance.LoadLevel(nextLevel);
    }
    else {
      Debug.LogError("LevelOrchestrator: GameManager instance not found!");
    }
  }

  #endregion

  #region Public API

  /// <summary>
  /// Returns true if a level transition is currently in progress.
  /// </summary>
  public bool IsTransitioning() {
    return _isTransitioning;
  }

  #endregion
}
