using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// StateMachine is a finite state machine (FSM) node that manages character states.
/// It handles state transitions, lifecycle management, and interrupt priority system.
/// </summary>
public partial class BaseStateMachine : Node
{
     #region Exports
    /// <summary>
    /// Number of jumps remaining (for double jump mechanics).
    /// </summary>
    [Export] private int MaxJumps = 1;
    [Export] private int InputSlot = 0;

    [ExportGroup("Gravity")]
    /// <summary>
    /// The gravity force applied to the character.
    /// </summary>
    [Export] private float Gravity = 50.0f;
    [Export] public float MaxGravity = 200.0f;
    [Export] public float TerminalVelocity = 50.0f;
    [Export] public float GravityExponent = 4.0f;

    [ExportGroup("Speed and Jumps")]
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float FallingMoveSpeed = 9.0f;
    [Export] public float JumpVelocity = 20.0f;
    [Export] public float TurnSpeed = 12.0f;
    [Export] public float WallJumpUpVelocity = 20.0f;
    [Export] public float WallJumpHorizontalVelocity = 30.0f;
    [Export] public float WallJumpHorizontalDuration = 0.35f;

    [ExportGroup("References")]
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private Skeleton3D _meshSkeleton;
    [Export] private MeshInstance3D _mesh;
    [Export] private MeshInstance3D _shadowMesh;
    [Export] private BaseCharacter _character;
    #endregion

    #region Properties
    /// <summary>
    /// The currently active state in the machine.
    /// </summary>
    private BaseState CurrentState;


    /// <summary>
    /// Collection of all states registered with this machine.
    /// </summary>
    public List<BaseState> _states { get; private set;} = new();

    /// <summary>
    /// Whether the state machine is currently processing a transition. If it is, good for it. We have no place to judge. 
    /// </summary>
    private bool _isTransitioning = false;

    #endregion

    #region Node Method Overrides
    public override void _Ready()
    {
        //Register all child nodes as states
        foreach (Node child in GetChildren()){
            if (child is BaseState state)
            {
                _states.Add(state);
                state.Machine = this;
                state.Character = _character;
            }
        }
        TransitionTo("Idle");
    }

    public override void _Process(double delta)
    {
        CurrentState?._Update((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState?._UpdatePhysics((float)delta);
    }

    #endregion

    #region State Mangement
    /// <summary>
    /// Transitions to a new state based on interrupt priority.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    /// <returns>True if the transition was successful.</returns>
    public bool TransitionTo(string newStateName, string interruptType = "Input")
    {
        // Check if current state can be interrupted
        if (CurrentState != null && !CurrentState.CanInterrupt(interruptType))
        {
            return false;
        }
        BaseState newState = this._states.First(s => s.StateName == newStateName);

        // Remove current state if exists
        if (CurrentState != null)
        {
            CurrentState.OnStateExit();
        }
        CurrentState = newState;
        CurrentState.OnStateBegin();

        return true;
    }
    #endregion

    /// <summary>
    /// Handles state change events.
    /// </summary>
    /// <param name="state">The state that changed.</param>
    /// <param name="entering">True if entering, false if exiting.</param>
    protected virtual void OnStateChanged(BaseState state, bool entering)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Gets all registered states.
    /// </summary>
    public List<BaseState> GetStates() => _states;

    /// <summary>
    /// Gets the current active state.
    /// </summary>
    public BaseState GetCurrentState() => CurrentState;
}
