using Godot;

/// <summary>
/// RunState handles running movement for the player character.
/// </summary>
public partial class RunState : BaseState
{
    [Export] public float RunSpeed = 10.0f;
    private bool _isRunning = false;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Run";
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );

        if (input.Length() > 0.1f)
        {
            _isRunning = true;
            
            // Check for attack input
            if (Input.IsActionJustPressed("attack"))
            {
                Machine.TransitionTo(new AttackState(), "Input");
            }

            // Check for jump input
            if (Input.IsActionJustPressed("jump"))
            {
                Character?.PerformJump();
            }

            // Check for dash input
            if (Input.IsActionJustPressed("dash") && Character.CurrentJumps > 0)
            {
                Machine.TransitionTo(new DashState(), "Input");
            }

            // Check for dodge input
            if (Input.IsActionJustPressed("dodge"))
            {
                Machine.TransitionTo(new DodgeState(), "Input");
            }
        }
        else
        {
            _isRunning = false;
        }
    }

    public override bool IsDone()
    {
        return !_isRunning;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
