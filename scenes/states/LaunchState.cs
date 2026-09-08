using Godot;

/// <summary>
/// LaunchState handles the state when launching an item/projectile.
/// </summary>
public partial class LaunchState : BaseState
{
    [Export] public float LaunchDuration = 0.3f;
    private bool _isLaunching = false;
    private float _launchTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isLaunching = true;
        _launchTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isLaunching)
        {
            _launchTimer += delta;
            
            // Check for launch completion
            if (_launchTimer >= LaunchDuration)
            {
                _isLaunching = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel launch on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isLaunching = false;
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
        return !_isLaunching;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
