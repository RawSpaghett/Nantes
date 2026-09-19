using UnityEngine;

public class Nantes: Enemy
{
    public Eyes eyes;
    public Ears ears;
    public Touch touch;

    protected override void Awake()
    {
        base.Awake();
        eyes = GetComponentInChildren<Eyes>();
        ears = GetComponentInChildren<Ears>();
        touch = GetComponentInChildren<Touch>();
    }
    
}
