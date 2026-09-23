using System;
using UnityEngine;

namespace NantesGame.Tablet
{
    public enum TabletState { Off, Booting, Home, Scanner, ShuttingDown }
    public enum ContactKind { Unknown, Food, Movement }

    [Serializable]
    public struct ScannerContact
    {
        public Vector2 position;
        public ContactKind kind;
        [Range(0,1)] public float confidence;
        public ScannerContact(Vector2 position,ContactKind kind,float confidence=1)
        {this.position=position;this.kind=kind;this.confidence=confidence;}
    }
}
