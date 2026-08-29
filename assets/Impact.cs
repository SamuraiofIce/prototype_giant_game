using Godot;
using System;

public partial class Impact : Sprite3D
{
    [Export] public float Lifecycle = 0.5f;

    // Final size relative to the starting size.
    [Export] public float FinalScale = 2.0f;

    // Maximum opacity (80% = 0.8).
    [Export] public float MaxOpacity = 0.8f;

    // Total rotation in radians over the lifecycle.
    [Export] public float SpinSpeed = 15.0f;

    private float _elapsed = 0.0f;
    private Vector3 _initialScale;

    private Vector3 _spinAxis = Vector3.Forward;

    // Tracks how much we've rotated so far.
    private float _previousSpin = 0.0f;

    public override void _Ready()
    {
        GD.Print(Position);
        GD.Print("Spawned correctly!");

        _initialScale = Scale;

        // Start completely transparent.
        Modulate = new Color(0, 0, 0, 0);
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;

        float progress = Mathf.Clamp(_elapsed / Lifecycle, 0.0f, 1.0f);

        // Ease-out curve.
        float easedProgress = 1.0f - Mathf.Pow(1.0f - progress, 3.0f);

        // Grow from initial scale to final scale.
        float scale = Mathf.Lerp(1.0f, FinalScale, easedProgress);
        Scale = _initialScale * scale;

        // Fade in during the first ~70% of the lifetime.
        float opacity;

        if (progress < 0.7f)
        {
            float fadeInProgress = progress / 0.7f;
            opacity = Mathf.Lerp(0.0f, MaxOpacity, fadeInProgress);
        }
        else
        {
            // Fade back out during the final ~30%.
            float fadeOutProgress = (progress - 0.7f) / 0.3f;
            opacity = Mathf.Lerp(MaxOpacity, 0.0f, fadeOutProgress);
        }

        Modulate = new Color(0, 0, 0, opacity);

        // Ease-out the spin.
        float currentSpin = easedProgress * SpinSpeed;
        float spinDelta = currentSpin - _previousSpin;

        Rotate(_spinAxis, spinDelta);

        _previousSpin = currentSpin;

        // Remove after the lifecycle ends.
        if (_elapsed >= Lifecycle)
        {
            QueueFree();
        }
    }

    public void Initialize(Vector3 spawnPosition, Vector3 normal, Vector3 spinAxis)
    {
        GlobalPosition = spawnPosition;

        LookAt(
            GlobalPosition + normal,
            spinAxis
        );

        _spinAxis = spinAxis.Normalized();
    }
}