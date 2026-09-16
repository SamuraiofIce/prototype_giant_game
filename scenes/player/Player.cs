using Godot;

/// <summary>
/// Player is the main player character class that combines physics, camera control, and state management.
/// This unifies the functionality from PlayerPhysics.cs and PlayerCamRig.cs while integrating with the state machine system.
/// </summary>
public partial class Player : BaseCharacter
{

    #region State Machine Integration

    /// <summary>
    /// Whether the player is currently on the ground.
    /// </summary>
    //public bool IsGrounded => IsOnFloor();

    #endregion

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
       
    }
}