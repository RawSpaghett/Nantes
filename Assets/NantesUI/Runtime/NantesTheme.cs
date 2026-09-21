using UnityEngine;

namespace Nantes.UI
{
    [CreateAssetMenu(menuName="Nantes/UI color theme")]
    public sealed class NantesTheme : ScriptableObject
    {
        public Color bone = new Color(.83f,.80f,.73f);
        public Color muted = new Color(.58f,.56f,.52f);
        public Color disabled = new Color(.31f,.30f,.28f);
        public Color blood = new Color(.70f,.075f,.045f);
        public Color deepRed = new Color(.24f,.035f,.026f);
        public Color charcoal = new Color(.018f,.021f,.023f);
    }
}
