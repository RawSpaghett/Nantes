using UnityEngine;
using UnityEngine.AI;

public class Nantes: Enemy, IEars, IEyes, ITouch
{
    public Eyes eyes;
    public Ears ears;
    public Touch touch;

    public EStateMachine<Nantes> stateMachine {get; set;}
    #region States
    private PursueState pursueState;
    private InvestigateState investigateState;
    private IdleState idleState;
    #endregion

    protected override float CurrentSpeed => stateMachine.currentState.speed; //cast back to base class

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
        stateMachine.ChangeState(investigateState);
    }

    public void OnSee(Vector3 target)
    {
        base.PathFinder(target);
        stateMachine.ChangeState(pursueState);
    }

    public void OnTouch(Vector3 target)
    {
        base.PathFinder(target);
        stateMachine.ChangeState(pursueState);
    }
    
}
