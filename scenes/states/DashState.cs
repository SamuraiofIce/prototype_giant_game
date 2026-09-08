using Godot;

/// <summary>
/// DashState handles dash movement for the player character.
/// </summary>
public partial class DashState : BaseState
{
    [Export] public float DashSpeed = 20.0f;
    [Export] public float DashDuration = 0.3f;
    private bool _isDashing = false;
    private float _dashTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Run"; // Use run animation for dash
        _isDashing = true;
        _dashTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isDashing)
        {
            _dashTimer += delta;
            
            // Check for dash completion
            if (_dashTimer >= DashDuration)
            {
                _isDashing = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel dash on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isDashing = false;
            AnimationName = "Idle";
        }

        // Check for jump input
        if (Input.IsActionJustPressed("jump"))
        {
            Character?.PerformJump();
        }

        // Check for dodge input
        if (Input.IsActionJustPressed("dodge"))
        {
            Machine.TransitionTo(new DodgeState(), "Input");
        }
    }

    public override bool IsDone()
    {
        return !_isDashing;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
