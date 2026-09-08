using Godot;

/// <summary>
/// DodgeState handles dodge/roll movement for the player character.
/// </summary>
public partial class DodgeState : BaseState
{
    [Export] public float DodgeDuration = 0.4f;
    private bool _isDodging = false;
    private float _dodgeTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Run"; // Use run animation for dodge
        _isDodging = true;
        _dodgeTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isDodging)
        {
            _dodgeTimer += delta;
            
            // Check for dodge completion
            if (_dodgeTimer >= DodgeDuration)
            {
                _isDodging = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel dodge on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isDodging = false;
            AnimationName = "Idle";
        }

        // Check for jump input
        if (Input.IsActionJustPressed("jump"))
        {
            Character?.PerformJump();
        }
    }

    public override bool IsDone()
    {
        return !_isDodging;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
