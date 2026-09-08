using Godot;

/// <summary>
/// BaseCharacter is a parent class for game characters that provides common properties and methods.
/// It handles position, velocity, gravity, and input processing for character-based entities.
/// </summary>
public partial class BaseCharacter : Node3D
{
    /// <summary>
    /// The current position of the character in world space.
    /// </summary>
    public Vector3 Position => GlobalPosition;

    /// <summary>
    /// The current velocity of the character.
    /// </summary>
    public Vector3 Velocity { get; set; } = Vector3.Zero;

    /// <summary>
    /// The gravity force applied to the character.
    /// </summary>
    [Export] public float Gravity = 9.81f;

    /// <summary>
    /// Whether the character is currently on the ground.
    /// </summary>
    public bool IsGrounded { get; set; }

    /// <summary>
    /// The current state of the character (if using a StateMachine).
    /// </summary>
    public BaseState? CurrentState { get; private set; }

    /// <summary>
    /// Reference to the parent CharacterBody3D node.
    /// </summary>
    [Export] private CharacterBody3D _characterBody;

    /// <summary>
    /// Input vector for movement (left/right, forward/backward).
    /// </summary>
    public Vector2 MovementInput => new(
        Input.IsActionPressed("move_right") ? 1.0f : 
        Input.IsActionPressed("move_left") ? -1.0f : 0.0f,
        Input.IsActionPressed("move_forward") ? 1.0f : 
        Input.IsActionPressed("move_backward") ? -1.0f : 0.0f
    );

    /// <summary>
    /// Whether the character is currently jumping.
    /// </summary>
    public bool IsJumping { get; set; }

    /// <summary>
    /// Number of jumps remaining (for double jump mechanics).
    /// </summary>
    [Export] public int MaxJumps = 1;
    [Export] public int CurrentJumps = 1;

    /// <summary>
    /// Whether the character is touching a wall.
    /// </summary>
    public bool IsTouchingWall { get; set; }

    public override void _Ready()
    {
        if (_characterBody != null)
        {
            // Connect to CharacterBody3D signals
            _characterBody.PositionChanged += OnCharacterPositionChanged;
        }
    }

    public override void _Process(double delta)
    {
        UpdateInputState();
        HandleJumping(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        Velocity = Vector3.Zero; // Reset velocity for physics engine to handle
    }

    /// <summary>
    /// Updates the input state based on current player actions.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    private void UpdateInputState()
    {
        // Input handling can be overridden by specific states
    }

    /// <summary>
    /// Handles jumping mechanics including double jump and wall jump.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    private void HandleJumping(float delta)
    {
        if (IsGrounded && Input.IsActionJustPressed("jump"))
        {
            PerformJump();
        }

        if (!IsGrounded && CurrentJumps > 0 && Input.IsActionJustPressed("jump"))
        {
            PerformDoubleJump();
        }
    }

    /// <summary>
    /// Applies gravity to the character.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    private void ApplyGravity(float delta)
    {
        if (_characterBody != null)
        {
            _characterBody.Velocity.Y -= Gravity * delta;
        }
    }

    /// <summary>
    /// Performs a standard jump when on the ground.
    /// </summary>
    private void PerformJump()
    {
        if (_characterBody != null)
        {
            _characterBody.Velocity.Y = 10.0f; // Jump velocity
            IsGrounded = false;
            CurrentJumps--;
        }
    }

    /// <summary>
    /// Performs a double jump when in the air.
    /// </summary>
    private void PerformDoubleJump()
    {
        if (_characterBody != null)
        {
            _characterBody.Velocity.Y = 10.0f; // Jump velocity
            CurrentJumps--;
        }
    }

    /// <summary>
    /// Called when the character's position changes.
    /// </summary>
    private void OnCharacterPositionChanged()
    {
        // Can be overridden by states for position-based logic
    }

    /// <summary>
    /// Checks if a state can interrupt the current state.
    /// </summary>
    /// <param name="interruptType">The type of interruption attempt.</param>
    /// <returns>True if the state can be interrupted.</returns>
    public virtual bool CanInterrupt(string interruptType)
    {
        return true; // Default: allow interruption
    }

    /// <summary>
    /// Called when a new state is entered.
    /// </summary>
    /// <param name="state">The new state to enter.</param>
    public virtual void OnStateEnter(BaseState state)
    {
        CurrentState = state;
        state.OnStateBegin();
    }

    /// <summary>
    /// Called when the current state is exited.
    /// </summary>
    /// <param name="state">The state being exited.</param>
    public virtual void OnStateExit(BaseState state)
    {
        CurrentState = null;
    }

    /// <summary>
    /// Handles wall collision detection and response.
    /// </summary>
    private void HandleWallCollision()
    {
        IsTouchingWall = false; // Override in specific states if needed
    }

    public void RequestStateChange(BaseState state)
    {
        CurrentState = state;
    }
}
