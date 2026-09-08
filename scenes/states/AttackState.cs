using Godot;

/// <summary>
/// AttackState handles melee attack for the player character.
/// </summary>
public partial class AttackState : BaseState
{
    [Export] public float AttackDuration = 0.5f;
    private bool _isAttacking = false;
    private float _attackTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isAttacking = true;
        _attackTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isAttacking)
        {
            _attackTimer += delta;
            
            // Check for attack completion
            if (_attackTimer >= AttackDuration)
            {
                _isAttacking = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel attack on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isAttacking = false;
            AnimationName = "Idle";
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

    public override bool IsDone()
    {
        return !_isAttacking;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
