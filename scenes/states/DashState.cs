using Godot;

public partial class DashState : BaseState
{
    [Export] public float DashSpeed = 300.0f;
    [Export] public float DashStopDistance = 1.5f;
    [Export] public float DashStartupDuration = 0.15f;
    [Export] public float MaxDashDuration = 2.0f;
    [Export] public float DashAccelerationTime = 0.15f;
    [Export] private PlayerUI UI;
    private bool _isDashStarting = false;

    private float _dashTimer = 0.0f;
    private float _dashDuration = 0.0f;

    private Vector3 _dashStartPosition;
    private Vector3 _dashTargetPosition;
    public override void OnStateBegin()
    {
        _dashStartPosition = Character.GlobalPosition;
        _dashTargetPosition = UI._currentTarget.GlobalPosition;
        Character._animationPlayer.Play(AnimationName);
        StartDash();
    }

    public override void _UpdatePhysics(float delta)
    {
        UpdateDash(delta);
    }

    private void StartDash()
    {
        Vector3 toTarget =
            _dashTargetPosition - _dashStartPosition;

        float distance = toTarget.Length();

        if (distance < 0.01f)
        {
            Machine.TransitionTo("Idle");
            return;
        }


        // Calculate how long the dash should take.

        _dashDuration = Mathf.Min(
            distance / (DashSpeed * 0.5f),
            MaxDashDuration
        );

        _dashTimer = 0.0f;

        //Vector3 direction = toTarget.Normalized();
        /*
            // Face the target.
            float targetRotation = Mathf.Atan2(
                direction.X,
                direction.Z
            );

            Rotation = new Vector3(
                Rotation.X,
                targetRotation,
                Rotation.Z
            );*/
    }

    private void UpdateDash(float delta)
    {
        _dashTimer += delta;

        float t = Mathf.Clamp(
            _dashTimer / _dashDuration,
            0.0f,
            1.0f
        );

        // Ease into the dash.
        float speedMultiplier = Mathf.SmoothStep(
            0.0f,
            1.0f,
            Mathf.Clamp(
                _dashTimer / DashAccelerationTime,
                0.0f,
                1.0f
            )
        );

        Vector3 toTarget =
            _dashTargetPosition - Character.GlobalPosition;

        float distanceRemaining = toTarget.Length();

        if (distanceRemaining < 0.05f)
        {
            Character.GlobalPosition = _dashTargetPosition;

            Character.Velocity = Vector3.Zero;

            Machine.TransitionTo("Idle");

            return;
        }

        Vector3 direction =
            toTarget.Normalized();

        float speed =
            DashSpeed * speedMultiplier;

        // Never move farther than the target in one frame.
        float movementDistance =
            Mathf.Min(
                speed * delta,
                distanceRemaining
            );

        Character.Velocity =
            direction *
            (movementDistance / delta);

        Character.MoveAndSlide();

        if (_dashTimer >= _dashDuration)
        {
            Character.Velocity = Vector3.Zero;
            Machine.TransitionTo("Idle");
        }
    }

}