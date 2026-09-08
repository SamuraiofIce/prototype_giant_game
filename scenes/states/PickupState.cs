using Godot;

/// <summary>
/// PickupState handles the state when picking up an item.
/// </summary>
public partial class PickupState : BaseState
{
    [Export] public float PickupDuration = 0.5f;
    private bool _isPickingUp = false;
    private float _pickupTimer = 0f;

    public override void OnStateBegin()
    {
        base.OnStateBegin();
        AnimationName = "Idle"; // Will be set by animation player
        _isPickingUp = true;
        _pickupTimer = 0f;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        if (_isPickingUp)
        {
            _pickupTimer += delta;
            
            // Check for pickup completion
            if (_pickupTimer >= PickupDuration)
            {
                _isPickingUp = false;
                AnimationName = "Idle";
            }
        }
    }

    public override void HandleInput(float delta)
    {
        base.HandleInput(delta);
        
        // Cancel pickup on new input
        if (Input.IsActionJustPressed("move_left") || 
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_forward") || 
            Input.IsActionJustPressed("move_backward"))
        {
            _isPickingUp = false;
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
        return !_isPickingUp;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        AnimationName = "Idle";
    }
}
