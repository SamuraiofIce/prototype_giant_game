using System;
using Godot;

public partial class PlayerCamRig : Node3D
{
    public enum CamMode
    {
        FirstPerson = 0,
        CloseUp = 1,
        Lakitu = 2
    }

    [Export] private Camera3D camera;
    [Export] private MeshInstance3D player;
    [Export] private float PlayerFadeSpeed = 5f;

    private float _playerTargetAlpha = 1f;
    private float _playerAlpha = 1f;
    private CamMode _previousCamMode;

    [Export] private float FirstPersonDistance = 0f;
    [Export] private float CloseUpDistance = 1.2f;
    [Export] private float LakituDistance = 4.3f;

    [Export] private float CameraTransitionSpeed = 15f;
    [Export] private float CameraRotationPerSec = 1.5f;
    [Export] private float CameraPitchTransitionSpeed = 100f;

    [Export] private Vector2 FirstPersonPitchLimits = new(-90f, 90f);
    [Export] private Vector2 CloseUpPitchLimits = new(-90f, 50f);
    [Export] private Vector2 LakituPitchLimits = new(-90f, 17f);
    [Export] private float PlayerFadeOutDistance = 0.5f;
    [Export] private float PlayerFadeInDistance = 0.9f;
    [Export] private float CameraPitchCorrectionSpeed = 90f;
    [Export] private float CloseUpHorizontalOffset = 0.75f;
    private float _targetCameraX;


    private double _rigYaw;
    private double _rigPitch;

    private CamMode _camMode = CamMode.Lakitu;

    private float _targetCameraDistance;
    private Vector2 _currentPitchLimits;
    private Vector2 _targetPitchLimits;

    public override void _Ready()
    {
        _targetCameraDistance = GetCameraDistance();
        _targetCameraX = GetCameraHorizontalOffset();

        _currentPitchLimits = GetPitchLimits();
        _targetPitchLimits = _currentPitchLimits;

        Position = new Vector3(0, 2.345f, 0);

        _rigPitch = Rotation.X;
        _rigYaw = Rotation.Y;

        camera.Position = new Vector3(
            _targetCameraX,
            0,
            _targetCameraDistance
        );

        camera.Rotation = Vector3.Zero;

        SetupPlayerMaterial();
    }

    private void SetupPlayerMaterial()
    {
        if (player == null)
            return;

        Material material = player.GetActiveMaterial(0);

        if (material is StandardMaterial3D standardMaterial)
        {
            standardMaterial = (StandardMaterial3D)standardMaterial.Duplicate();
            standardMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            player.SetSurfaceOverrideMaterial(0, standardMaterial);
        }
    }

    public override void _Process(double delta)
    {
        HandleCameraModeInput();
        HandleCameraTransition(delta);
        HandlePitchLimitTransition(delta);
        HandlePlayerFade(delta);
        HandleRotationInput(delta);
    }

    private void HandleCameraModeInput()
    {
        if (Input.IsActionJustPressed("cam_in"))
        {
            SetCameraMode(_camMode - 1);
        }

        if (Input.IsActionJustPressed("cam_out"))
        {
            SetCameraMode(_camMode + 1);
        }
    }

    private void SetCameraMode(CamMode mode)
    {
        mode = (CamMode)Math.Clamp(
            (int)mode,
            (int)CamMode.FirstPerson,
            (int)CamMode.Lakitu
        );

        if (_camMode == mode)
            return;

        _previousCamMode = _camMode;
        _camMode = mode;

        _targetCameraDistance = GetCameraDistance();
        _targetPitchLimits = GetPitchLimits();
        _targetCameraX = GetCameraHorizontalOffset();
    }

    private float GetCameraHorizontalOffset()
    {
        return _camMode switch
        {
            CamMode.CloseUp => CloseUpHorizontalOffset,
            _ => 0f
        };
    }
    private void HandlePlayerFade(double delta)
    {
        float targetAlpha;

        if (_previousCamMode == CamMode.FirstPerson &&
            _camMode != CamMode.FirstPerson)
        {
            // Leaving first person: wait until the camera has moved
            // far enough away before bringing the player back.
            targetAlpha = Mathf.Clamp(
                (camera.Position.Z - PlayerFadeInDistance) /
                (GetCameraDistance() - PlayerFadeInDistance),
                0f,
                1f
            );
        }
        else if (_camMode == CamMode.FirstPerson)
        {
            // Entering first person: fade out as the camera gets close.
            targetAlpha = Mathf.Clamp(
                camera.Position.Z / PlayerFadeOutDistance,
                0f,
                1f
            );
        }
        else
        {
            targetAlpha = 1f;
        }

        _playerAlpha = Mathf.MoveToward(
            _playerAlpha,
            targetAlpha,
            PlayerFadeSpeed * (float)delta
        );

        SetPlayerAlpha(_playerAlpha);
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (player == null)
            return;

        if (player.GetActiveMaterial(0) is StandardMaterial3D material)
        {
            Color color = material.AlbedoColor;
            color.A = alpha;
            material.AlbedoColor = color;
        }
    }

    private void HandleCameraTransition(double delta)
    {
        float deltaFloat = (float)delta;

        float newX = Mathf.MoveToward(
            camera.Position.X,
            _targetCameraX,
            CameraTransitionSpeed * deltaFloat
        );

        float newZ = Mathf.MoveToward(
            camera.Position.Z,
            _targetCameraDistance,
            CameraTransitionSpeed * deltaFloat
        );

        camera.Position = new Vector3(
            newX,
            camera.Position.Y,
            newZ
        );
    }

    private void HandlePitchLimitTransition(double delta)
    {
        _currentPitchLimits.X = Mathf.MoveToward(
            _currentPitchLimits.X,
            _targetPitchLimits.X,
            CameraPitchTransitionSpeed * (float)delta
        );

        _currentPitchLimits.Y = Mathf.MoveToward(
            _currentPitchLimits.Y,
            _targetPitchLimits.Y,
            CameraPitchTransitionSpeed * (float)delta
        );
    }

    private float GetCameraDistance()
    {
        return _camMode switch
        {
            CamMode.FirstPerson => FirstPersonDistance,
            CamMode.CloseUp => CloseUpDistance,
            CamMode.Lakitu => LakituDistance,
            _ => LakituDistance
        };
    }

    private Vector2 GetPitchLimits()
    {
        return _camMode switch
        {
            CamMode.FirstPerson => FirstPersonPitchLimits,
            CamMode.CloseUp => CloseUpPitchLimits,
            CamMode.Lakitu => LakituPitchLimits,
            _ => LakituPitchLimits
        };
    }

    private void HandleRotationInput(double delta)
    {
        double horizontalMovement =
            Input.GetAxis("cam_left", "cam_right")
            * delta
            * CameraRotationPerSec
            * 360;

        double verticalMovement =
            Input.GetAxis("cam_up", "cam_down")
            * delta
            * CameraRotationPerSec
            * 360;

        _rigYaw -= horizontalMovement;
        _rigPitch -= verticalMovement;

        float deltaFloat = (float)delta;

        // Smoothly correct the pitch if it is outside the current limits.
        if (_rigPitch < _currentPitchLimits.X)
        {
            _rigPitch = Mathf.MoveToward(
                (float)_rigPitch,
                _currentPitchLimits.X,
                CameraPitchCorrectionSpeed * deltaFloat
            );
        }
        else if (_rigPitch > _currentPitchLimits.Y)
        {
            _rigPitch = Mathf.MoveToward(
                (float)_rigPitch,
                _currentPitchLimits.Y,
                CameraPitchCorrectionSpeed * deltaFloat
            );
        }

        // Never allow the pitch to actually exceed the current limits.
        _rigPitch = Math.Clamp(
            _rigPitch,
            _currentPitchLimits.X,
            _currentPitchLimits.Y
        );

        Rotation = new Vector3(
            Mathf.DegToRad((float)_rigPitch),
            Mathf.DegToRad((float)_rigYaw),
            0
        );
    }
}