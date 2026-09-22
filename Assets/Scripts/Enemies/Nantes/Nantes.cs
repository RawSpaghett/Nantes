using UnityEngine;
using UnityEngine.AI;

public class Nantes: Enemy, IEars, IEyes, ITouch
{
    #region Components
    public Eyes eyes;
    public Ears ears;
    public Touch touch;
    #endregion

    #region States

    public EStateMachine<Nantes> stateMachine {get; set;}
    private PursueState pursueState;
    public InvestigateState investigateState;
    private IdleState idleState;
    #endregion

    public Transform playerGhost {get;set;}
    public bool activeVision {get;set;}
    protected override float CurrentSpeed => stateMachine.currentState.speed; //cast back to base class23

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
        investigateState = new InvestigateState(this,stateMachine);
        idleState = new IdleState(this,stateMachine);
        stateMachine.Intialize(idleState);
    }
    protected override void FixedUpdate()
    {
       stateMachine.currentState.FrameUpdate();
    }
    
    //Interfaces
    public void OnNoiseHeard(Vector3 target)
    {
        base.PathFinder(target);
        if(stateMachine.currentState != investigateState)
            stateMachine.ChangeState(investigateState);
    }

    public void OnSee()
    {
        if(stateMachine.currentState != pursueState)
            stateMachine.ChangeState(pursueState);
    }

    public void OnTouch()
    {
        if(stateMachine.currentState != pursueState)
            stateMachine.ChangeState(pursueState);
    }
    
}
