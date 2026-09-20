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
    

    protected override void Move()
    {
        
    }

    protected override void PathFinder()
    {
        //Grabs closest path to a target
        if (NavMesh.CalculatePath(transform.position, base.target, NavMesh.AllAreas, path)) // stores resulting path
        {
            if(path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                base.cornerCount = path.GetCornersNonAlloc(base.cornerArray);
                currentCornerIndex = 1; //not including self
            }
        }
        else
        {
            Debug.Log("<Color=red>No Complete OR Partial path found</Color>");
        }
    }

    //Interfaces

    public void OnNoiseHeard()
    {}

    public void OnSee()
    {}

    public void OnTouch()
    {}
    
}
