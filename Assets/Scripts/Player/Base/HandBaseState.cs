using UnityEngine;

public abstract class HandBaseState
{
    public abstract void EnterState(PlayerHandManager handManager);

    public abstract void UpdateState(PlayerHandManager handManager);

    public abstract void ExitState(PlayerHandManager handManager);

    public abstract void CheckSwitchState(PlayerHandManager handManager);

}
