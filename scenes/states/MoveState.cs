using Godot;

/// <summary>
/// MoveState handles walking movement for the player character.
/// </summary>
public partial class MoveState : BaseState
{
    [Export] public float MoveSpeed = 5.0f;
    private bool _isMoving = false;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Start with idle animation
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
            _isMoving = true;
            
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
            _isMoving = false;
        }
    }

    public override bool IsDone()
    {
        return !_isMoving;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
