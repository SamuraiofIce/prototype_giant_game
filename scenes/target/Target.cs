using Godot;

/// <summary>
/// Target point/marker for various use cases.
/// </summary>
public partial class Target : Node3D
{
    [ExportGroup("Targeting")]
    [Export] public Marker3D TargetPoint;

    [Export] public Marker3D DashPoint;
    public bool IsTargeted { get; private set; }

    public override void _Ready()
    {
        AddToGroup("targetable");
    }
    public virtual void SetTargeted(bool targeted)
    {
        if (IsTargeted == targeted)
            return;

        IsTargeted = targeted;
    }
}