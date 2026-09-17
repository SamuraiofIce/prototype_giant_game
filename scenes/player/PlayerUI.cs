using Godot;

public partial class PlayerUI : CanvasGroup
{
    [Export] private TextureRect _targetReticle;
    [Export] private Camera3D _camera;

	[Export] public float TargetSearchInterval = 0.1f;
	[Export] public float MaxTargetDistance = 30.0f;
    private Target _currentTarget;
	private float _targetSearchTimer = 0.0f;
    public override void _Ready()
    {
        _targetReticle.Visible = false;
        UpdateTargetReticle();
    }
    public override void _Process(double delta)
    {
        HandleTargeting((float) delta);
        UpdateTargetReticle();
    }


	private void HandleTargeting(float delta)
	{
		_targetSearchTimer -= delta;

		if (_targetSearchTimer > 0.0f)
			return;

		_targetSearchTimer = TargetSearchInterval;

		Target bestTarget = FindBestTarget();

		if (bestTarget == _currentTarget)
			return;

		_currentTarget?.SetTargeted(false);

		_currentTarget = bestTarget;

		_currentTarget?.SetTargeted(true);
	}
	private Target FindBestTarget()
	{
		Target bestTarget = null;
		float bestScore = float.MaxValue;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

		// The center of the LEFT half of the screen.
		float leftHalfCenterX = viewportSize.X * 0.25f;
		float screenCenterY = viewportSize.Y * 0.5f;

		foreach (Node node in GetTree().GetNodesInGroup("targets"))
		{
			if (node is not Target target)
				continue;

			if (!IsInstanceValid(target))
				continue;


			// Don't target things that are too far away.
			float distance = GetParent<Camera3D>().GlobalPosition.DistanceTo(
				target.GlobalPosition
			);

			if (distance > MaxTargetDistance)
				continue;
			
			// Convert the target's 3D position to screen coordinates.
			Vector2 screenPosition =
				_camera.UnprojectPosition(target.GlobalPosition);

			// Ignore targets behind the camera.
			if (_camera.IsPositionBehind(target.GlobalPosition))
				continue;

			// Only consider the LEFT half of the screen.
			// The character will grapple with their left, so they shouldn't be able to grapple to the right.
			// Can be adjusted later.
			if (screenPosition.X > viewportSize.X * 0.5f)
				continue;

			// Calculate how far the target is from the center
			// of the left half of the screen.
			float screenDistance = new Vector2(
				screenPosition.X - leftHalfCenterX,
				screenPosition.Y - screenCenterY
			).Length();

			// Normalize screen distance so resolution doesn't
			// drastically affect the score.
			float normalizedScreenDistance =
				screenDistance / viewportSize.X;

			// Combine screen position and world distance.
			float score =
				normalizedScreenDistance * 10.0f +
				distance * 0.1f;

			if (score < bestScore)
			{
				bestScore = score;
				bestTarget = target;
			}
		}

		return bestTarget;
	}
    private void UpdateTargetReticle()
	{
		if (_currentTarget == null ||
			!IsInstanceValid(_currentTarget))
		{
			_targetReticle.Visible = false;
			return;
		}

		Vector3 targetPosition = _currentTarget.GetTargetPosition();

		if (_camera.IsPositionBehind(targetPosition))
		{
			_targetReticle.Visible = false;
			return;
		}

		Vector2 screenPosition =
			_camera.UnprojectPosition(targetPosition);

		_targetReticle.Position =
			screenPosition - (_targetReticle.Size / 2.0f);

		_targetReticle.Visible = true;
		GD.Print("Showing target");
	}
}
