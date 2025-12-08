using UnityEngine;
using System.Collections;

public class TileAnimator : MonoBehaviour
{
    private Vector3 _targetPosition;
    private float _duration = 0.6f; 
    private float _delay = 0f;

    public void Animate(Vector3 finalPos)
    {
        _targetPosition = finalPos;
        
        transform.position = finalPos + (Vector3.down * 20f); 
        
        _delay = Random.Range(0f, 0.2f);

        StartCoroutine(MoveUp());
    }

    private IEnumerator MoveUp()
    {
        
        yield return new WaitForSeconds(_delay);

        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < _duration)
        {
            
            transform.position = Vector3.Lerp(startPos, _targetPosition, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        
        transform.position = _targetPosition;
        
        Destroy(this);
    }
}