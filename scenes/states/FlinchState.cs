using Godot;

/// <summary>
/// FlinchState handles the flinch/recovery state after taking damage.
/// </summary>
public partial class FlinchState : BaseState
{
    [Export] public float FlinchDuration = 0.3f;
    private bool _isFlinching = false;
    private float _flinchTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isFlinching = true;
        _flinchTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isFlinching)
        {
            _flinchTimer += delta;
            
            // Check for flinch completion
            if (_flinchTimer >= FlinchDuration)
            {
                _isFlinching = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Allow getting up from flinch
        if (Input.IsActionJustPressed("jump"))
        {
            _isFlinching = false;
            AnimationName = "Idle";
        }
    }

    public override bool IsDone()
    {
        return !_isFlinching;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }

    public override bool CanInterrupt(string interruptType)
    {
        // Flinch can be interrupted by jump (get up)
        return interruptType == "Input" || interruptType == "TakeHit";
    }
}
