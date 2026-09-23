using UnityEngine;

namespace NantesGame.Tablet
{
    public sealed class TabletHardwareKey : MonoBehaviour
    {
        public TabletController tablet;
        public int action;
        Vector3 rest;
        float travel;
        void Awake(){rest=transform.localPosition;}
        public void Press(){travel=1;}
        void Update(){travel=Mathf.MoveTowards(travel,0,Time.unscaledDeltaTime*6);transform.localPosition=rest+Vector3.forward*travel*.022f;}
    }
}
