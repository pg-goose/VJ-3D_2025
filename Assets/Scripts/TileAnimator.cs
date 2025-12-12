using UnityEngine;
using System;
using System.Collections;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float maxDelay = 0.2f;
    [SerializeField] private float dropHeight = 20f;
    
    private Vector3 _targetPosition;
    private float _delay;
    private Action _onComplete;

    /// <summary>
    /// Returns the total time this animation will take (delay + duration)
    /// </summary>
    public float TotalDuration => _delay + duration;

    /// <summary>
    /// Prepares the tile for animation (moves it to start position)
    /// </summary>
    public void Prepare(Vector3 finalPos) {
        _targetPosition = finalPos;
        _delay = UnityEngine.Random.Range(0f, maxDelay);
        transform.localPosition = finalPos + (Vector3.down * dropHeight);
    }

    /// <summary>
    /// Starts the animation. Optionally calls onComplete when done.
    /// </summary>
    public void Play(Action onComplete = null) {
        _onComplete = onComplete;
        StartCoroutine(AnimateCoroutine());
    }

    private IEnumerator AnimateCoroutine() {
        yield return new WaitForSeconds(_delay);

        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < duration) {
            transform.localPosition = Vector3.Lerp(startPos, _targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _targetPosition;
        _onComplete?.Invoke();
        Destroy(this);
    }
}