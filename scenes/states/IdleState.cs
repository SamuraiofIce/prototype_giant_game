using Godot;

public partial class IdleState : BaseState
{

    public override void OnStateBegin()
    {
        GD.Print("Idling...");
        Character._animationPlayer.Play(AnimationName);
    }
}