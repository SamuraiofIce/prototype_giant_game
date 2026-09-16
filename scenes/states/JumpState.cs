using Godot;

public partial class JumpState : BaseState
{
    private float jumpStartup = 0.2f;
    private float legTimer;
    private bool jumpStarted;

    public override void OnStateBegin()
    {
        legTimer = 0;
        jumpStarted = false;
        Character._animationPlayer.Play("NinjaAnims/Jump");
        
    }
    public override void _Process(double delta)
    {
        
    }

    public override void _UpdatePhysics(float delta)
    {
        legTimer += delta;

        if (jumpStarted && Character.IsOnFloor())
        {
            Machine.TransitionTo("Idle");
        }
        if(legTimer >= 0.2f && !jumpStarted)
        {
            Character.Velocity = new Vector3(
                Character.Velocity.X,
                Character.JumpVelocity,
                Character.Velocity.Z
            );
            jumpStarted = true;
        }
        base.HandleMovement(delta);
        Character.MoveAndSlide();
    }
}