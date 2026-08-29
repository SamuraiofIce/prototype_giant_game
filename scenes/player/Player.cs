using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public float MoveSpeed = 12.0f;
[Export] public float FallingMoveSpeed = 9.0f;


    [Export] public float JumpVelocity = 20.0f;

    // Gravity settings
    [Export] public float Gravity = 50.0f;
[Export] public float MaxGravity = 200.0f;
[Export] public float TerminalVelocity = 50.0f;
[Export] public float GravityExponent = 4.0f;


    [Export] public float WallJumpUpVelocity = 20.0f;
    [Export] public float WallJumpHorizontalVelocity = 30.0f;
    [Export] public float WallJumpHorizontalDuration = 0.35f;

    [Export] private Camera3D _camera;
    [Export] private PackedScene WallJumpEffect;
    [Export] private Skeleton3D _meshSkeleton;

[Export] public float TurnSpeed = 12.0f;

    private Vector3 _wallNormal = Vector3.Zero;
    private bool _isTouchingWall = false;

    // Wall-jump momentum
    private Vector3 _wallJumpVelocity = Vector3.Zero;
    private float _wallJumpTimer = 0.0f;

    // Allows one additional jump while airborne.
    private bool _hasAirJump = true;

    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        HandleMovement(dt);
        MoveAndSlide();

        // Update wall state AFTER movement so we know
        // what we actually collided with this frame.
        UpdateWallState();
    }

    private void HandleMovement(float delta)
    {
        HandleHorizontalMovement(delta);
        HandleGravity(delta);
        HandleJump();
    }

    private void HandleHorizontalMovement(float delta)
{
    Vector2 input = Input.GetVector(
        "move_left",
        "move_right",
        "move_forward",
        "move_backward"
    );

    // Camera-relative movement.
    Vector3 cameraForward = -_camera.GlobalTransform.Basis.Z;
    Vector3 cameraRight = _camera.GlobalTransform.Basis.X;

    cameraForward.Y = 0.0f;
    cameraRight.Y = 0.0f;

    cameraForward = cameraForward.Normalized();
    cameraRight = cameraRight.Normalized();

    Vector3 direction =
        cameraRight * input.X +
        cameraForward * -input.Y;

    if (direction.LengthSquared() > 1.0f)
        direction = direction.Normalized();
Vector3 facingDirection = new Vector3(
    Velocity.X,
    0.0f,
    Velocity.Z
);

if (facingDirection.LengthSquared() > 0.001f)
{
    facingDirection = facingDirection.Normalized();

    Basis targetBasis = Basis.LookingAt(
        facingDirection,
        Vector3.Up
    );

    _meshSkeleton.GlobalBasis = _meshSkeleton.GlobalBasis.Slerp(
        targetBasis,
        1.0f - Mathf.Exp(-TurnSpeed * delta)
    );
    /*_shadowMesh.GlobalBasis = _shadowMesh.GlobalBasis.Slerp(
        targetBasis,
        1.0f - Mathf.Exp(-TurnSpeed * delta)
    );*/
}


    // --------------------------------
    // WALL JUMP MOMENTUM
    // --------------------------------

    if (_wallJumpTimer > 0.0f)
    {
        _wallJumpTimer -= delta;

        float strength = Mathf.Clamp(
            _wallJumpTimer / WallJumpHorizontalDuration,
            0.0f,
            1.0f
        );

        Vector3 wallVelocity =
            _wallJumpVelocity * strength;

        Velocity = new Vector3(
            wallVelocity.X,
            Velocity.Y,
            wallVelocity.Z
        );

        return;
    }

    // --------------------------------
    // NORMAL MOVEMENT
    // --------------------------------

    float maxHorizontalSpeed =
        Velocity.Y < 0.0f
            ? FallingMoveSpeed
            : MoveSpeed;

    Vector3 horizontalVelocity = Vector3.Zero;

    if (direction.LengthSquared() > 0.0f)
    {
        horizontalVelocity =
            direction * maxHorizontalSpeed;
    }

    Velocity = new Vector3(
        horizontalVelocity.X,
        Velocity.Y,
        horizontalVelocity.Z
    );
}

    private void HandleGravity(float delta)
{
    if (!IsOnFloor())
    {
        // Only care about downward velocity.
        float fallSpeed = Mathf.Max(-Velocity.Y, 0.0f);

        // Convert fall speed into a 0-1 range.
        float fallProgress = Mathf.Clamp(
            fallSpeed / TerminalVelocity,
            0.0f,
            1.0f
        );

        // Exponential curve.
        //
        // Exponent of 1 = linear
        // Exponent of 2 = gradual ramp-up
        // Exponent of 3+ = stays gentle longer, then ramps sharply
        float gravityProgress = Mathf.Pow(
            fallProgress,
            GravityExponent
        );

        // Increase gravity as the player falls faster.
        float currentGravity = Mathf.Lerp(
            Gravity,
            MaxGravity,
            gravityProgress
        );

        float newVelocityY =
            Velocity.Y - currentGravity * delta;

        // Never exceed terminal velocity.
        newVelocityY = Mathf.Max(
            newVelocityY,
            -TerminalVelocity
        );

        Velocity = new Vector3(
            Velocity.X,
            newVelocityY,
            Velocity.Z
        );
    }
    else if (Velocity.Y < 0.0f)
    {
        Velocity = new Vector3(
            Velocity.X,
            0.0f,
            Velocity.Z
        );
    }
}

    private void HandleJump()
    {
        if (!Input.IsActionJustPressed("jump"))
            return;

        // --------------------------------
        // GROUND JUMP
        // --------------------------------

        if (IsOnFloor())
        {
            Velocity = new Vector3(
                Velocity.X,
                JumpVelocity,
                Velocity.Z
            );

            // Refresh the air jump.
            _hasAirJump = true;

            return;
        }

        // --------------------------------
        // WALL JUMP
        // --------------------------------

        if (_isTouchingWall)
        {
            _wallJumpVelocity =
                _wallNormal * WallJumpHorizontalVelocity;

            _wallJumpTimer =
                WallJumpHorizontalDuration;

            Velocity = new Vector3(
                _wallJumpVelocity.X,
                WallJumpUpVelocity,
                _wallJumpVelocity.Z
            );

            // Wall jumping gives the player
            // their air jump back.
            _hasAirJump = true;

            SpawnWallJumpEffect(_wallNormal);

            return;
        }

        // --------------------------------
        // DOUBLE / AIR JUMP
        // --------------------------------

        if (_hasAirJump)
        {
            Velocity = new Vector3(
                Velocity.X,
                JumpVelocity,
                Velocity.Z
            );

            _hasAirJump = false;

            SpawnWallJumpEffect(Vector3.Down);
        }
    }

    private void UpdateWallState()
    {
        _isTouchingWall = false;
        _wallNormal = Vector3.Zero;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision =
                GetSlideCollision(i);

            Vector3 normal = collision.GetNormal();

            // Ignore floors and ceilings.
            if (Mathf.Abs(normal.Y) < 0.7f)
            {
                _isTouchingWall = true;
                _wallNormal = normal;
                break;
            }
        }
    }

    private void SpawnWallJumpEffect(Vector3 normal)
    {
        if (WallJumpEffect == null)
            return;

        var effect = WallJumpEffect.Instantiate<Impact>();

        GetTree().CurrentScene.AddChild(effect);

        effect.Initialize(
            GlobalPosition,
            normal,
            Vector3.Down
        );

        effect = WallJumpEffect.Instantiate<Impact>();

        GetTree().CurrentScene.AddChild(effect);

        effect.Initialize(
            GlobalPosition,
            normal,
            Vector3.Left
        );
    }
}