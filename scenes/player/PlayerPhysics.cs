using Godot;

/// <summary>
/// This script controls the physics for the player specifically. 
/// </summary>
/// <remarks>
/// In the future, it may be best to genralize this script and split out the inputs.
/// It also does not currently integrate with Godot's built-in physics engine
/// for things such as gravity. This gives us very finite control, at some potential technical debt.
/// </remarks>
public partial class PlayerPhysics : Node3D
{
    /// <summary>
    /// Enum for holding jump types/states.
    /// </summary>
    /// <remarks>
    /// The 'Bonus' type is currently used for both air jumps and wall jumps.  
    /// </remarks>
    private enum JumpType
    {
        None,
        Grounded,
        Bonus
    }
    private JumpType _jumpType = JumpType.None;


    [ExportGroup("References")]
    [Export] private Camera3D _camera;
    [Export] private PackedScene WallJumpEffect;
    [Export] private Skeleton3D _meshSkeleton;
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private CharacterBody3D parent;

    [ExportGroup("Gravity")]
    [Export] public float Gravity = 50.0f;
    [Export] public float MaxGravity = 200.0f;
    [Export] public float TerminalVelocity = 50.0f;
    [Export] public float GravityExponent = 4.0f;

    [ExportGroup("Speed and Jumps")]
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float FallingMoveSpeed = 9.0f;
    [Export] public float JumpVelocity = 20.0f;
    [Export] public float TurnSpeed = 12.0f;
    [Export] public float WallJumpUpVelocity = 20.0f;
    [Export] public float WallJumpHorizontalVelocity = 30.0f;
    [Export] public float WallJumpHorizontalDuration = 0.35f;


    private Vector3 _wallNormal = Vector3.Zero;
    private Vector3 _wallJumpVelocity = Vector3.Zero;
    private bool _isTouchingWall = false;
    // Wall-jump momentum
    private float _wallJumpTimer = 0.0f;
    private bool _hasAirJump = true;

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement((float) delta);
        parent.MoveAndSlide();
        // Update wall state AFTER movement so we know
        // what we actually collided with this frame.
        UpdateWallState();
        UpdateAnimation();
    }

    /// <summary>
    /// Handles movement for the current frame. Calls a number of helper functions.
    /// </summary>
    /// <param name="delta">(float) This frame's delta, per _process().</param>
    private void HandleMovement(float delta)
    {
        HandleHorizontalMovement(delta);
        HandleGravity(delta);
        HandleJump();
    }

    /// <summary>
    /// Handles horizontal movement, including the momentum from wall jumps.
    /// </summary>
    /// <param name="delta">(float) This frame's delta, per _process().</param>
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
            parent.Velocity.X,
            0.0f,
            parent.Velocity.Z
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

            parent.Velocity = new Vector3(
                wallVelocity.X,
                parent.Velocity.Y,
                wallVelocity.Z
            );

            return;
        }

        // --------------------------------
        // NORMAL MOVEMENT
        // --------------------------------

        float maxHorizontalSpeed =
            parent.Velocity.Y < 0.0f
                ? FallingMoveSpeed
                : MoveSpeed;

        Vector3 horizontalVelocity = Vector3.Zero;

        if (direction.LengthSquared() > 0.0f)
        {
            horizontalVelocity =
                direction * maxHorizontalSpeed;
        }

        parent.Velocity = new Vector3(
            horizontalVelocity.X,
            parent.Velocity.Y,
            horizontalVelocity.Z
        );
    }

    /// <summary>
    /// Handles gravity for the current frame. 
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
    /// <remarks>
    /// We likely want to replace/update this later on for more finite control over the gravity
    /// to get a real nice, smooth, "feely" gravity. Like, crisp and clean, fine tuned.
    /// </remarks>
    private void HandleGravity(float delta)
    {
        if (!parent.IsOnFloor())
        {
            // Only care about downward velocity.
            float fallSpeed = Mathf.Max(-parent.Velocity.Y, 0.0f);

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
                parent.Velocity.Y - currentGravity * delta;

            // Never exceed terminal velocity.
            newVelocityY = Mathf.Max(
                newVelocityY,
                -TerminalVelocity
            );

            parent.Velocity = new Vector3(
                parent.Velocity.X,
                newVelocityY,
                parent.Velocity.Z
            );
        }
        else if (parent.Velocity.Y < 0.0f)
        {
            parent.Velocity = new Vector3(
                parent.Velocity.X,
                0.0f,
                parent.Velocity.Z
            );
        }
    }

    /// <summary>
    /// Handles jumping. If the jump button isn't pressed, this does nothing on the current frame.
    /// </summary>
    private void HandleJump()
    {
        if (!Input.IsActionJustPressed("jump"))
            return;

        // --------------------------------
        // GROUND JUMP
        // --------------------------------

        if (parent.IsOnFloor())
        {
            parent.Velocity = new Vector3(
                parent.Velocity.X,
                JumpVelocity,
                parent.Velocity.Z
            );

            // Refresh the air jump.
            _hasAirJump = true;

            _jumpType = JumpType.Grounded;

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

            parent.Velocity = new Vector3(
                _wallJumpVelocity.X,
                WallJumpUpVelocity,
                _wallJumpVelocity.Z
            );

            // Wall jumping gives the player
            // their air jump back.
            _hasAirJump = true;

            _jumpType = JumpType.Bonus;

            SpawnWallJumpEffect(_wallNormal);

            return;
        }

        // --------------------------------
        // DOUBLE / AIR JUMP
        // --------------------------------

        if (_hasAirJump)
        {
            parent.Velocity = new Vector3(
                parent.Velocity.X,
                JumpVelocity,
                parent.Velocity.Z
            );

            _hasAirJump = false;

            _jumpType = JumpType.Bonus;

            SpawnWallJumpEffect(Vector3.Down);
        }
    }

    /// <summary>
    /// Updates the current animation.
    /// </summary>
    /// <remarks>
    /// This is another area to really fine-tune. Right now it's super basic.
    /// No smoothing, no super dynamic animations, just really rough righ tnow.
    /// </remarks>
    private void UpdateAnimation()
    {
        // --------------------------------
        // ON GROUND
        // --------------------------------

        if (parent.IsOnFloor())
        {
            Vector3 horizontalVelocity = new Vector3(
                parent.Velocity.X,
                0.0f,
                parent.Velocity.Z
            );

            if (horizontalVelocity.LengthSquared() > 0.01f)
            {
                PlayAnimation("NinjaAnims/Run");
            }
            else
            {
                PlayAnimation("NinjaAnims/Idle");
            }

            _jumpType = JumpType.None;

            return;
        }

        // --------------------------------
        // IN AIR
        // --------------------------------

        if (_jumpType == JumpType.Bonus)
        {
            PlayAnimation("NinjaAnims/Backflip");
        }
        else
        {
            PlayAnimation("NinjaAnims/Jump");
        }
    }
    /// <summary>
    /// Plays an animation with a given name.
    /// </summary>
    /// <param name="animationName">(string) A name matching an animation in the animation player's library.</param>
    private void PlayAnimation(string animationName)
    {
        if (_animationPlayer.CurrentAnimation != animationName)
        {
            _animationPlayer.Play(animationName);
        }
    }


    /// <summary>
    /// Updtes the wall state. Starts by setting the player as not touching a wall,
    /// then checks to see if they are.
    /// </summary>
    private void UpdateWallState()
    {
        _isTouchingWall = false;
        _wallNormal = Vector3.Zero;

        for (int i = 0; i < parent.GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision =
                parent.GetSlideCollision(i);

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

    /// <summary>
    /// Spawns the little effect that plays when a player wall jumps. 
    /// </summary>
    /// <param name="normal">(Vector3) The normal of the wall that they're jumping off of.</param>
    private void SpawnWallJumpEffect(Vector3 normal)
    {
        if (WallJumpEffect == null)
            return;

        var effect = WallJumpEffect.Instantiate<Impact>();

        GetTree().CurrentScene.AddChild(effect);

        effect.Initialize(
            parent.GlobalPosition,
            normal,
            Vector3.Down
        );

        effect = WallJumpEffect.Instantiate<Impact>();

        GetTree().CurrentScene.AddChild(effect);

        effect.Initialize(
            parent.GlobalPosition,
            normal,
            Vector3.Left
        );
    }
}