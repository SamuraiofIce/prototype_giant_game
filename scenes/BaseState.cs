using Godot;

/// <summary>
/// BaseState is an abstract base class for all game states in the state machine.
/// It defines the lifecycle pattern that all states must follow.
/// </summary>
public abstract partial class BaseState : Node
{
    /// <summary>
    /// Reference to the parent character using this state.
    /// </summary>
    [Export] public BaseCharacter Character { get; set; }

    /// <summary>
    /// Reference to the state machine managing this state.
    /// </summary>
    public BaseStateMachine Machine { get; set; }

    /// <summary>
    /// Priority level for interrupting other states.
    /// Higher values can interrupt lower priority states.
    /// Force (0) > Input (1) > TakeHit (2)
    /// </summary>
    [Export] public int InterruptPriority = 1;

    /// <summary>
    /// Name of the animation to play when entering this state.
    /// </summary>
    [Export] public string AnimationName = "NinjaAnims/Idle";

    /// <summary>
    /// Whether this state should be interrupted by lower priority states.
    /// </summary>
    [Export] public bool CanBeInterrupted = true;
    [Export] public string StateName = "Base";
    protected Vector2 HorizontalMovementVector = Vector2.Zero;

    public override void _Ready()
    {
        Machine = GetParent<BaseStateMachine>();
    }

    /// <summary>
    /// Called when the state is entered.
    /// </summary>
    public virtual void OnStateBegin()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Process method called every frame (non-physics).
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    public virtual void _Update(float delta)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Physics process method called every physics frame.
    /// </summary>
    /// <param name="delta">Time since last physics frame.</param>
    public virtual void _UpdatePhysics(float delta)
    {
        // Override in derived classes if needed
        HandleInput((float) delta);
        HandleAnimation((float) delta);
        HandleMovement((float)delta);
        Character.MoveAndSlide();

        // Update wall state AFTER movement so we know what we actually collided with this frame.
        //UpdateWallState();
        //UpdateAnimation();
    }

    /// <summary>
    /// Checks if the current state should end and transition to another state.
    /// </summary>
    /// <returns>True if the state should end.</returns>
    public virtual bool IsDone()
    {
        return false; // Default: continue in this state
    }

    /// <summary>
    /// Checks if this state can be interrupted by a given interruption type.
    /// </summary>
    /// <param name="interruptType">The type of interruption attempt.</param>
    /// <returns>True if the state can be interrupted.</returns>
    public virtual bool CanInterrupt(string interruptType)
    {
        return true; // Default: allow interruption
    }

    /// <summary>
    /// Called when the state is about to exit.
    /// </summary>
    public virtual void OnStateExit()
    {
        // Override in derived classes if cleanup is needed
    }

    /// <summary>
    /// Handles input specific to this state.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    protected virtual void HandleInput(float delta)
    {
        // Override in derived classes for state-specific input handling
        Vector2 leftJoyInput = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );
        HorizontalMovementVector += leftJoyInput;

        if (Input.IsActionJustPressed("jump"))
        {
            Machine.TransitionTo("Jump");
        }
    }

    /// <summary>
    /// Handles animation playback for this state.
    /// </summary>
    protected virtual void HandleAnimation(float delta)
    {

    }

    /// <summary>
    /// Checks for state completion conditions.
    /// </summary>
    /// <returns>True if the state should end.</returns>
    protected virtual bool CheckStateCompletion()
    {
        return IsDone();
    }

    /// <summary>
    /// Handles movement for the current frame. Calls a number of helper functions.
    /// </summary>
    protected void HandleMovement(float delta)
    {
        HandleHorizontalMovement(delta);
        HandleGravity(delta);
    }

    /// <summary>
    /// Handles horizontal movement, including the momentum from wall jumps.
    /// </summary>
    protected  void HandleHorizontalMovement(float delta)
    {
        // Camera-relative movement.
        Vector3 cameraForward = -Character._camera.GlobalTransform.Basis.Z;
        Vector3 cameraRight = Character._camera.GlobalTransform.Basis.X;

        cameraForward.Y = 0.0f;
        cameraRight.Y = 0.0f;

        cameraForward = cameraForward.Normalized();
        cameraRight = cameraRight.Normalized();

        Vector3 direction =
            cameraRight * HorizontalMovementVector.X +
            cameraForward * -HorizontalMovementVector.Y;

        if (direction.LengthSquared() > 1.0f)
            direction = direction.Normalized();

        Vector3 facingDirection = new Vector3(
            Character.Velocity.X,
            0.0f,
            Character.Velocity.Z
        );

        if (facingDirection.LengthSquared() > 0.001f)
        {
            facingDirection = facingDirection.Normalized();
        }

        // --------------------------------
        // NORMAL, GROUNDED MOVEMENT
        // --------------------------------

        float maxHorizontalSpeed =
            Character.Velocity.Y < 0.0f
                ? Character.FallingMoveSpeed
                : Character.MoveSpeed;

        Vector3 horizontalVelocity = Vector3.Zero;

        if (direction.LengthSquared() > 0.0f)
        {
            horizontalVelocity =
                direction * maxHorizontalSpeed;
        }

        Character.Velocity = new Vector3(
            horizontalVelocity.X,
            Character.Velocity.Y,
            horizontalVelocity.Z
        );

        HorizontalMovementVector = Vector2.Zero;
    }

    /// <summary>
    /// Handles gravity for the current frame. 
    /// </summary>
    private void HandleGravity(float delta)
    {
        if (!Character.IsOnFloor())
        {
            // Only care about downward velocity.
            float fallSpeed = Mathf.Max(-Character.Velocity.Y, 0.0f);

            // Convert fall speed into a 0-1 range.
            float fallProgress = Mathf.Clamp(
                fallSpeed / Character.TerminalVelocity,
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
                Character.GravityExponent
            );

            // Increase gravity as the player falls faster.
            float currentGravity = Mathf.Lerp(
                Character.Gravity,
                Character.MaxGravity,
                gravityProgress
            );


            float newVelocityY =
                Character.Velocity.Y - currentGravity * delta;

            // Never exceed terminal velocity.
            newVelocityY = Mathf.Max(
                newVelocityY,
                -Character.TerminalVelocity
            );

            Character.Velocity = new Vector3(
                Character.Velocity.X,
                newVelocityY,
                Character.Velocity.Z
            );
        }
        else if (Character.Velocity.Y < 0.0f)
        {
            Character.Velocity = new Vector3(
                Character.Velocity.X,
                0.0f,
                Character.Velocity.Z
            );
        }
    }
}
