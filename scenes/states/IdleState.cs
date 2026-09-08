using Godot;

/// <summary>
/// IdleState handles the idle state for the player character.
/// This state is entered when the player is not performing any other action.
/// </summary>
public partial class IdleState : BaseState
{
    /// <summary>
    /// Time spent in idle state before auto-transitioning.
    /// </summary>
    [Export] private float _idleTimeout = 0f;

    /// <summary>
    /// Timer for idle timeout.
    /// </summary>
    private float _idleTimer = 0f;

    /// <summary>
    /// Whether the player is currently idle.
    /// </summary>
    public bool IsIdle => CurrentState == this;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        
        // Reset idle timer
        _idleTimer = 0f;
        
        // Play idle animation
        if (Character != null && Character._animationPlayer != null)
        {
            Character._animationPlayer.Animate("NinjaAnims", "Idle");
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        // Update idle timer
        if (_idleTimeout > 0)
        {
            _idleTimer += delta;
            
            // Auto-transition after timeout
            if (_idleTimer >= _idleTimeout)
            {
                CheckStateCompletion();
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Check for movement input
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );

        if (input.Length() > 0.1f)
        {
            // Transition to MoveState or RunState based on input magnitude
            Machine.TransitionTo(new MoveState(), "Input");
        }

        // Check for jump input
        if (Input.IsActionJustPressed("jump"))
        {
            // Jump will be handled by PlayerPhysics, but we can reset idle timer
            _idleTimer = 0f;
        }

        // Check for attack input
        if (Input.IsActionJustPressed("attack"))
        {
            // Transition to AttackState
            Machine.TransitionTo(new AttackState(), "Input");
        }

        // Check for dash input
        if (Input.IsActionJustPressed("dash") && Character.CurrentJumps > 0)
        {
            // Transition to DashState
            Machine.TransitionTo(new DashState(), "Input");
        }

        // Check for dodge input
        if (Input.IsActionJustPressed("dodge"))
        {
            // Transition to DodgeState
            Machine.TransitionTo(new DodgeState(), "Input");
        }

        // Check for grapple input
        if (Input.IsActionJustPressed("grapple"))
        {
            // Transition to GrappleState
            Machine.TransitionTo(new GrappleState(), "Input");
        }

        // Check for grab input
        if (Input.IsActionJustPressed("grab"))
        {
            // Transition to GrabState
            Machine.TransitionTo(new GrabState(), "Input");
        }
    }

    public override bool IsDone()
    {
        // Check if player is moving
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );

        if (input.Length() > 0.1f)
        {
            return true; // Transition to movement state
        }

        return false;
    }

    public override bool CanInterrupt(string interruptType)
    {
        // Idle state can be interrupted by any input
        return true;
    }
}
