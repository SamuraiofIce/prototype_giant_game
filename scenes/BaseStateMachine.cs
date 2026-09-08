using Godot;

/// <summary>
/// StateMachine is a finite state machine (FSM) node that manages character states.
/// It handles state transitions, lifecycle management, and interrupt priority system.
/// </summary>
public partial class StateMachine : Node3D
{
    /// <summary>
    /// Reference to the parent CharacterBody3D node.
    /// </summary>
    [Export] private CharacterBody3D _characterBody;

    /// <summary>
    /// The currently active state in the machine.
    /// </summary>
    protected BaseState? CurrentState { get; private set; }

    /// <summary>
    /// Collection of all states registered with this machine.
    /// </summary>
    private List<BaseState> _states = new();

    /// <summary>
    /// Whether the state machine is currently processing a transition.
    /// </summary>
    private bool _isTransitioning = false;

    public override void _Ready()
    {
        // Find parent CharacterBody3D
        if (GetParent<CharacterBody3D>() != null)
        {
            _characterBody = GetParent<CharacterBody3D>();
            
            // Connect to physics process for state updates
            _characterBody.BodyMoved += OnCharacterBodyMoved;
            _characterBody.BodyEnteredGround += OnCharacterEnteredGround;
            _characterBody.BodyLeftGround += OnCharacterLeftGround;
        }

        // Register all child nodes as states
        foreach (Node3D child in GetTree().Root.GetNodeOrNull("Player")?.GetChildren() ?? [])
        {
            if (child is BaseState state)
            {
                _states.Add(state);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (CurrentState != null && !CurrentState.IsDisposed())
        {
            CurrentState._Process((float)delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentState != null && !CurrentState.IsDisposed())
        {
            CurrentState._PhysicsProcess((float)delta);
        }
    }

    /// <summary>
    /// Adds a state to the machine.
    /// </summary>
    /// <param name="state">The state to add.</param>
    public void AddState(BaseState state)
    {
        if (!_states.Contains(state))
        {
            _states.Add(state);
            
            // Connect state lifecycle events
            OnStateChanged?.Invoke(state, true); // Entering state
        }
    }

    /// <summary>
    /// Removes a state from the machine.
    /// </summary>
    /// <param name="state">The state to remove.</param>
    public void RemoveState(BaseState state)
    {
        if (_states.Remove(state))
        {
            OnStateChanged?.Invoke(state, false); // Exiting state
        }
    }

    /// <summary>
    /// Transitions to a new state based on interrupt priority.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    /// <returns>True if the transition was successful.</returns>
    public bool TransitionTo(BaseState newState, string interruptType = "Input")
    {
        if (CurrentState != null && CurrentState.IsDisposed())
        {
            CurrentState.Dispose();
            CurrentState = null;
        }

        // Check if current state can be interrupted
        if (CurrentState != null && !CurrentState.CanInterrupt(interruptType))
        {
            return false;
        }

        // Remove current state if exists
        if (CurrentState != null)
        {
            CurrentState.OnStateExit();
            _states.Remove(CurrentState);
            CurrentState.Dispose();
            CurrentState = null;
        }

        // Add new state
        AddState(newState);

        // Update character reference
        if (newState.Character == null && _characterBody != null)
        {
            newState.Character = _characterBody as BaseCharacter;
        }

        return true;
    }

    /// <summary>
    /// Handles state change events.
    /// </summary>
    /// <param name="state">The state that changed.</param>
    /// <param name="entering">True if entering, false if exiting.</param>
    protected virtual void OnStateChanged(BaseState state, bool entering)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Called when the character's body moves.
    /// </summary>
    private void OnCharacterBodyMoved()
    {
        // Can be overridden by states for movement-based logic
    }

    /// <summary>
    /// Called when the character enters the ground.
    /// </summary>
    private void OnCharacterEnteredGround()
    {
        // Can trigger state transitions based on ground contact
    }

    /// <summary>
    /// Called when the character leaves the ground.
    /// </summary>
    private void OnCharacterLeftGround()
    {
        // Can trigger state transitions based on becoming airborne
    }

    /// <summary>
    /// Event raised when a state changes.
    /// </summary>
    public event Action<BaseState, bool>? OnStateChanged;

    /// <summary>
    /// Gets all registered states.
    /// </summary>
    public List<BaseState> GetStates() => _states;

    /// <summary>
    /// Gets the current active state.
    /// </summary>
    public BaseState? GetCurrentState() => CurrentState;

    /// <summary>
    /// Disposes of all states in the machine.
    /// </summary>
    public void DisposeAllStates()
    {
        foreach (var state in _states)
        {
            state.Dispose();
        }
        _states.Clear();
        CurrentState = null;
    }

    protected override void _ExitTree()
    {
        DisposeAllStates();
    }
}
