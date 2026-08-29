/*using Godot;
using System;
public partial class PlayerExample : CharacterBody3D
{
	[Export] public float CameraRotationSpeed = 250.0f;
	[Export] public float CameraZoomSpeed = 15.0f;

	[Export] public float MinZoom = 3.0f;
	[Export] public float MaxZoom = 10.0f;

	[Export] public float ZoomedInPitch = 25.0f;
	[Export] public float ZoomedOutPitch = -25.0f;
	[Export] public float MoveSpeed = 12.0f;
	[Export] public float RotationSpeed = 10.0f;

	[Export] public float TargetSearchInterval = 0.1f;
	[Export] public float MaxTargetDistance = 30.0f;
	[Export] public float JumpVelocity = 10.0f;
	[Export] public float Gravity = 20.0f;
	[Export] public float DashSpeed = 300.0f;
	[Export] public float DashStopDistance = 1.5f;


	[Export] public float WallJumpUpVelocity = 15.0f;
	[Export] public float WallJumpHorizontalVelocity = 15.0f;
	[Export] public float WallCheckDistance = 0.8f;

	[Export] public float WallJumpHorizontalDuration = 0.35f;

	[Export] public float DashStartupDuration = 0.15f;
	[Export] public float MaxDashDuration = 2.0f;
	[Export] public float DashAccelerationTime = 0.15f;

	private Vector3 _wallNormal = Vector3.Zero;
	private bool _isTouchingWall = false;

	private Vector3 _wallJumpVelocity = Vector3.Zero;
	private float _wallJumpTimer = 0.0f;

	private bool _isDashing = false;
	private bool _hasAirJump = true;
	private bool _isWallJumping = false;
	private bool _isDashStarting = false;

	private float _dashTimer = 0.0f;
	private float _dashDuration = 0.0f;

	private Vector3 _dashStartPosition;
	private Vector3 _dashTargetPosition;
	private float _targetSearchTimer = 0.0f;

	private Node3D _cameraRig;
	private Camera3D _camera;

	private float _yaw = 0.0f;
	private float _pitch = -20.0f;
	private float _zoom = 5.0f;
	private TextureRect _targetReticle;

	public override void _Ready()
	{
		_cameraRig = GetNode<Node3D>("CamRig");
		_camera = GetNode<Camera3D>("CamRig/Camera3D");

		//_cameraRig.Position = Vector3.Zero;
		_cameraRig.GlobalPosition = GlobalPosition;

		_camera.Position = new Vector3(0, 0, _zoom);

		_targetReticle =
	GetNode<TextureRect>("TargetingUI/TargetReticle");

		_targetReticle.Visible = false;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;



		_cameraRig.Position = Vector3.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;



		if (!_isDashing)
		{
			HandleMovement(dt);
		}
	}


	private void HandleMovement(float delta)
	{
		Vector2 input = Input.GetVector(
			"move_left",
			"move_right",
			"move_forward",
			"move_backward"
		);

		Vector3 cameraForward =
			-_camera.GlobalTransform.Basis.Z;

		Vector3 cameraRight =
			_camera.GlobalTransform.Basis.X;

		cameraForward.Y = 0.0f;
		cameraRight.Y = 0.0f;

		cameraForward = cameraForward.Normalized();
		cameraRight = cameraRight.Normalized();

		Vector3 direction =
			cameraRight * input.X +
			cameraForward * -input.Y;

		if (direction.LengthSquared() > 1.0f)
			direction = direction.Normalized();

		// --------------------------------
		// WALL JUMP MOMENTUM
		// --------------------------------

		if (_wallJumpTimer > 0.0f)
		{
			_wallJumpTimer -= delta;

			float t = Mathf.Clamp(
				_wallJumpTimer / WallJumpHorizontalDuration,
				0.0f,
				1.0f
			);

			// Smoothly reduce the wall-jump momentum.
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
		}
		else
		{
			// Normal movement.
			Velocity = new Vector3(
				direction.X * MoveSpeed,
				Velocity.Y,
				direction.Z * MoveSpeed
			);
		}

		// Gravity.
		if (!IsOnFloor())
		{
			Velocity = new Vector3(
				Velocity.X,
				Velocity.Y - Gravity * delta,
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

		// Jump input.
		HandleJump();

		// Rotate toward movement direction.

		MoveAndSlide();

		UpdateWallState();
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
	private void HandleJump()
	{
		if (!Input.IsActionJustPressed("jump"))
			return;

		// Ground jump.
		if (IsOnFloor())
		{
			Velocity = new Vector3(
				Velocity.X,
				JumpVelocity,
				Velocity.Z
			);

			_hasAirJump = true;
			_isWallJumping = false;

			return;
		}

		// Wall jump.
		if (_isTouchingWall)
		{
			_wallJumpVelocity =
				_wallNormal * WallJumpHorizontalVelocity;

			_wallJumpTimer = WallJumpHorizontalDuration;

			Velocity = new Vector3(
				_wallJumpVelocity.X,
				WallJumpUpVelocity,
				_wallJumpVelocity.Z
			);

			// Wall jumping restores the air jump.
			_hasAirJump = true;

			_isWallJumping = true;

			return;
		}

		// Air jump.
		if (_hasAirJump)
		{
			Velocity = new Vector3(
				Velocity.X,
				JumpVelocity,
				Velocity.Z
			);

			_hasAirJump = false;
			_isWallJumping = false;
		}
	}

	


	
	private bool IsTouchingWall()
	{
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision =
				GetSlideCollision(i);

			Vector3 normal = collision.GetNormal();

			// Ignore floors and ceilings.
			if (Mathf.Abs(normal.Y) < 0.7f)
			{
				_wallNormal = normal;
				return true;
			}
		}

		_wallNormal = Vector3.Zero;
		return false;
	}
}*/