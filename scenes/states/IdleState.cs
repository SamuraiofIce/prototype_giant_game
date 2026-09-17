using Godot;

public partial class IdleState : BaseState
{
    protected override void HandleInput(float delta)
    {
        if (Input.IsActionJustPressed("dash") && Machine.TransitionTo("Dash"))
            return;
            
        base.HandleInput(delta);
    }
}