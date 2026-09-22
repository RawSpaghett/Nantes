using UnityEngine;
using UnityEngine.AI;

public class BNantes: Enemy, ITouch
{
    public Touch touch;

    public EStateMachine<BNantes> stateMachine {get; set;}

    #region States
    public BPursueState bPursueState;
    public RetreatState retreatState;
    #endregion
    protected override float CurrentSpeed => stateMachine.currentState.speed; //cast back to base class

    protected Transform retreatPoint;

    protected override void Awake()
    {
        base.Awake();
        //Components
        touch = GetComponentInChildren<Touch>();
        //States
        stateMachine = new EStateMachine<BNantes>();
        bPursueState = new BPursueState(this,stateMachine); 
        retreatState = new RetreatState(this,stateMachine);
        stateMachine.Intialize(bPursueState);
        retreatPoint = GameObject.FindWithTag("retreatPoint").transform;
    }
    protected override void FixedUpdate()
    {
       stateMachine.currentState.FrameUpdate();
    }

    //Interfaces
    public void OnTouch()
    {
        //player lose state
    }

    public void OnRetreat()
    {
        base.PathFinder(retreatPoint.position);
        stateMachine.ChangeState(retreatState);
    }
    
}
