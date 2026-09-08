using Godot;

/// <summary>
/// GrappleState handles the state when grappling to a target.
/// </summary>
public partial class GrappleState : BaseState
{
    [Export] public float GrappleDuration = 0f; // 0 = indefinite
    private bool _isGrappled = false;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isGrappled = true;
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Release grapple
        if (Input.IsActionJustPressed("release"))
        {
            _isGrappled = false;
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
            // Can transition to movement state while grappled
            Machine.TransitionTo(new MoveState(), "Input");
        }

        // Check for attack input
        if (Input.IsActionJustPressed("attack"))
        {
            _isGrappled = false;
            AnimationName = "Idle";
        }
    }

    public override bool IsDone()
    {
        return GrappleDuration > 0 && true; // Simplified - check actual grapple timer if needed
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
