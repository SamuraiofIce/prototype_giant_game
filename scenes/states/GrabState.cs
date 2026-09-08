using Godot;

/// <summary>
/// GrabState handles the state when grabbing an item/enemy.
/// </summary>
public partial class GrabState : BaseState
{
    [Export] public float GrabDuration = 0f; // 0 = indefinite
    private bool _isGrabbing = false;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isGrabbing = true;
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Release grab
        if (Input.IsActionJustPressed("release"))
        {
            _isGrabbing = false;
            AnimationName = "Idle";
        }

        // Check for movement input
        Vector2 input = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );

        if (input.Length() > 0.1f)
        {
            // Can transition to movement state while grabbing
            Machine.TransitionTo(new MoveState(), "Input");
        }

        // Check for attack input
        if (Input.IsActionJustPressed("attack"))
        {
            _isGrabbing = false;
            AnimationName = "Idle";
        }
    }

    public override bool IsDone()
    {
        return GrabDuration > 0 && true; // Simplified - check actual grab timer if needed
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
