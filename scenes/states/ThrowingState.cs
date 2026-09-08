using Godot;

/// <summary>
/// ThrowingState handles the state when throwing an item/projectile.
/// </summary>
public partial class ThrowingState : BaseState
{
    [Export] public float ThrowDuration = 0.3f;
    private bool _isThrowing = false;
    private float _throwTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isThrowing = true;
        _throwTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isThrowing)
        {
            _throwTimer += delta;
            
            // Check for throw completion
            if (_throwTimer >= ThrowDuration)
            {
                _isThrowing = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel throw on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isThrowing = false;
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
        return !_isThrowing;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
