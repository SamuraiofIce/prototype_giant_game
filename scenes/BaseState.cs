using Godot;

/// <summary>
/// BaseState is an abstract base class for all game states in the state machine.
/// It defines the lifecycle pattern that all states must follow.
/// </summary>
public abstract partial class BaseState : Node3D
{
    /// <summary>
    /// Reference to the parent character using this state.
    /// </summary>
    [Export] protected BaseCharacter Character { get; set; }

    /// <summary>
    /// Reference to the state machine managing this state.
    /// </summary>
    [Export] protected StateMachine Machine { get; set; }

    /// <summary>
    /// Priority level for interrupting other states.
    /// Higher values can interrupt lower priority states.
    /// Force (0) > Input (1) > TakeHit (2)
    /// </summary>
    [Export] public int InterruptPriority = 1;

    /// <summary>
    /// Name of the animation to play when entering this state.
    /// </summary>
    [Export] public string AnimationName = "Idle";

    /// <summary>
    /// Whether this state should be interrupted by lower priority states.
    /// </summary>
    [Export] public bool CanBeInterrupted = true;

    public override void _Ready()
    {
        if (Character != null)
        {
            Character.RequestStateChange(this);
        }
    }

    /// <summary>
    /// Called when the state is entered.
    /// </summary>
    public virtual void OnStateBegin()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Process method called every frame (non-physics).
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    public virtual void _Process(float delta)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Physics process method called every physics frame.
    /// </summary>
    /// <param name="delta">Time since last physics frame.</param>
    public virtual void _PhysicsProcess(float delta)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Checks if the current state should end and transition to another state.
    /// </summary>
    /// <returns>True if the state should end.</returns>
    public virtual bool IsDone()
    {
        return false; // Default: continue in this state
    }

    /// <summary>
    /// Checks if this state can be interrupted by a given interruption type.
    /// </summary>
    /// <param name="interruptType">The type of interruption attempt.</param>
    /// <returns>True if the state can be interrupted.</returns>
    public virtual bool CanInterrupt(string interruptType)
    {
        return true; // Default: allow interruption
    }

    /// <summary>
    /// Called when the state is about to exit.
    /// </summary>
    public virtual void OnStateExit()
    {
        // Override in derived classes if cleanup is needed
    }

    /// <summary>
    /// Handles input specific to this state.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    protected virtual void HandleInput(float delta)
    {
        // Override in derived classes for state-specific input handling
    }

    /// <summary>
    /// Handles animation playback for this state.
    /// </summary>
    protected virtual void HandleAnimation()
    {
        if (Character != null && Character._animationPlayer != null)
        {
            // Animation handling can be overridden by specific states
        }
    }

    /// <summary>
    /// Checks for state completion conditions.
    /// </summary>
    /// <returns>True if the state should end.</returns>
    protected virtual bool CheckStateCompletion()
    {
        return IsDone();
    }
}
