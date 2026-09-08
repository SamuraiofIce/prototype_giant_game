using Godot;

/// <summary>
/// HoldingState handles the state when holding an item.
/// </summary>
public partial class HoldingState : BaseState
{
    [Export] public float HoldDuration = 0f; // 0 = indefinite
    private bool _isHolding = false;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isHolding = true;
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Release held item
        if (Input.IsActionJustPressed("release"))
        {
            _isHolding = false;
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
            // Can transition to movement state while holding
            Machine.TransitionTo(new MoveState(), "Input");
        }

        // Check for attack input
        if (Input.IsActionJustPressed("attack"))
        {
            // Use held item
            _isHolding = false;
            AnimationName = "Idle";
        }
    }

    public override bool IsDone()
    {
        return HoldDuration > 0 && true; // Simplified - check actual hold timer if needed
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
