using UnityEngine;

namespace Nantes.UI
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class NantesMenuMusic : MonoBehaviour
    {
        [Range(0, 1)] public float level = .48f;
        [Min(.1f)] public float fadeInSeconds = 3;
        AudioSource source;
        float elapsed;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.volume = 0;
        }

        void Start() { if (source.clip != null && !source.isPlaying) source.Play(); }

        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = level * Mathf.SmoothStep(0, 1, elapsed / fadeInSeconds);
        }
    }
}
