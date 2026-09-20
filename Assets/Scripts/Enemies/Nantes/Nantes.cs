using UnityEngine;
using UnityEngine.AI;

public class Nantes: Enemy, IEars, IEyes, ITouch
{
    public Eyes eyes;
    public Ears ears;
    public Touch touch;

    public EStateMachine<Nantes> stateMachine {get; set;}

    #region States
    public PursueState pursueState;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        //Components
        eyes = GetComponentInChildren<Eyes>();
        ears = GetComponentInChildren<Ears>();
        touch = GetComponentInChildren<Touch>();
        //States
        stateMachine = new EStateMachine<Nantes>();
        pursueState = new PursueState(this,stateMachine);
    }
    protected override void FixedUpdate()
    {
       stateMachine.currentState.FrameUpdate();
    }
    
    //Interfaces
    public void OnNoiseHeard()
    {}

    public void OnSee()
    {}

    public void OnTouch()
    {}
    
}
