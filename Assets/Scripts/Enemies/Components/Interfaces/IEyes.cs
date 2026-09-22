using UnityEngine;

public interface IEyes
{
    bool activeVision { get; set; }
    Transform playerGhost { get; set; }

    public void OnSee();
}
