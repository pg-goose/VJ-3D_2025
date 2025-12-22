using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
  [Header("Audio Settings")]
  [SerializeField] private AudioClip musicClip;
  [SerializeField, Range(0f, 1f)] private float targetVolume = 0.5f;
  
  [Header("Fade Settings")]
  [SerializeField] private float fadeInDuration = 2f;

  private AudioSource _audioSource;

  private void Awake() {
    _audioSource = GetComponent<AudioSource>();
    ConfigureAudioSource();
  }

  private void Start() {
    StartCoroutine(PlayWithFadeIn());
  }

  private void ConfigureAudioSource() {
    _audioSource.clip = musicClip;
    _audioSource.loop = true;
    _audioSource.playOnAwake = false;
    _audioSource.volume = 0f;
  }

  private IEnumerator PlayWithFadeIn() {
    _audioSource.Play();
    
    float elapsed = 0f;
    while (elapsed < fadeInDuration) {
      elapsed += Time.deltaTime;
      float normalizedTime = elapsed / fadeInDuration;
      _audioSource.volume = Mathf.Lerp(0f, targetVolume, normalizedTime);
      yield return null;
    }
    
    _audioSource.volume = targetVolume;
  }
}
