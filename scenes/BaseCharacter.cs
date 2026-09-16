using Godot;

/// <summary>
/// BaseCharacter is a parent class for game characters that provides common properties and methods.
/// It handles position, velocity, gravity, and input processing for character-based entities.
/// </summary>
public partial class BaseCharacter : CharacterBody3D
{
    public enum JumpType
    {
        None,
        Grounded,
        Bonus
    }
    public JumpType _jumpType = JumpType.None;


    [ExportGroup("References")]
    [Export] public Camera3D _camera;
    [Export] protected PackedScene WallJumpEffect;
    [Export] protected Skeleton3D _meshSkeleton;
    [Export] public AnimationPlayer _animationPlayer;

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

    #region Properties

    public Vector3 _wallNormal = Vector3.Zero;
    public Vector3 _wallJumpVelocity = Vector3.Zero;
    public bool _isTouchingWall = false;
    // Wall-jump momentum
    public float _wallJumpTimer = 0.0f;
    public bool _hasAirJump = true;
    

    #endregion
}