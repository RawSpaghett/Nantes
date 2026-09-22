using UnityEngine;
using UnityEngine.AI;

public class BNantes: Enemy, ITouch
{
    public Touch touch;

    public EStateMachine<BNantes> stateMachine {get; set;}

    #region States
    public PursueState pursueState;
    public RetreatState retreatState;
    #endregion
    protected override float CurrentSpeed => stateMachine.currentState.speed; //cast back to base class

    protected override void Awake()
    {
        base.Awake();
        //Components
        touch = GetComponentInChildren<Touch>();
        //States
        stateMachine = new EStateMachine<BNantes>();
        //pursueState = new PursueState(this,stateMachine); Needs to be decoupled from nantes
        retreatState = new RetreatState(this,stateMachine);
    }
    protected override void FixedUpdate()
    {
       stateMachine.currentState.FrameUpdate();
    }

    //Interfaces
    public void OnTouch()
    {}
    
}
