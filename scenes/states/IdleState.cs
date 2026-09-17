using Godot;

public partial class IdleState : BaseState
{

    public override void OnStateBegin()
    {
        Character._animationPlayer.Play(AnimationName);
    }
}