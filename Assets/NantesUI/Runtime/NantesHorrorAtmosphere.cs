using UnityEngine;
using UnityEngine.UI;

namespace Nantes.UI
{
    public sealed class NantesHorrorAtmosphere : MonoBehaviour
    {
        public NantesMenu menu;
        public RawImage veil;
        Material film;
        float clock;

        void Awake() { film = new Material(veil.material); veil.material = film; }
        void Update()
        {
            if (!menu.ReducedMotion) clock += Time.unscaledDeltaTime;
            film.SetFloat("_SignalTime", clock);
        }
        void OnDestroy() { if (film != null) Destroy(film); }
    }
}
