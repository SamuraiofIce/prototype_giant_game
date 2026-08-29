using Godot;

public partial class PlayerCamRig : Node3D
{
    public enum CamMode
    {
        FirstPerson,
        CloseUp,
        Lakitu
    }

    // -------------------------------------------------------------------------
    // References
    // -------------------------------------------------------------------------

    [Export] private Camera3D _camera;
    [Export] private MeshInstance3D _player;
    [Export] private MeshInstance3D _playerShadowMesh;
    // -------------------------------------------------------------------------
    // Camera Distances
    // -------------------------------------------------------------------------

    [Export] private float _firstPersonDistance = 0f;
    [Export] private float _closeUpDistance = 1.2f;
    [Export] private float _lakituDistance = 4.3f;

    // -------------------------------------------------------------------------
    // Camera Movement
    // -------------------------------------------------------------------------

    [Export] private float _cameraTransitionSpeed = 15f;
    [Export] private float _cameraRotationPerSecond = 1.5f;
    [Export] private float _cameraPitchTransitionSpeed = 100f;
    [Export] private float _cameraPitchCorrectionSpeed = 90f;

    [Export] private float _closeUpHorizontalOffset = 0.75f;

    // -------------------------------------------------------------------------
    // Camera Pitch Limits
    // -------------------------------------------------------------------------

    [Export] private Vector2 _firstPersonPitchLimits = new(-89f, 89f);
    [Export] private Vector2 _closeUpPitchLimits = new(-89f, 50f);
    [Export] private Vector2 _lakituPitchLimits = new(-89f, 25f);

    // -------------------------------------------------------------------------
    // Player Fade
    // -------------------------------------------------------------------------

    [Export] private float _playerFadeSpeed = 5f;
    [Export] private float _playerFadeOutDistance = 0.5f;
    [Export] private float _playerFadeInDistance = 0.9f;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private CamMode _cameraMode = CamMode.Lakitu;
    private CamMode _previousCameraMode;

    private float _targetCameraDistance;
    private float _targetCameraHorizontalOffset;

    private Vector2 _currentPitchLimits;
    private Vector2 _targetPitchLimits;

    private float _playerAlpha = 1f;

    private double _rigYaw;
    private double _rigPitch;

    // -------------------------------------------------------------------------
    // Godot Lifecycle
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        InitializeRig();
        InitializeCamera();
        SetupPlayerMaterial();
    }

    public override void _Process(double delta)
    {
        HandleCameraModeInput();
        HandleRotationInput(delta);

        UpdateCameraTransition(delta);
        UpdatePitchLimitTransition(delta);
        UpdatePlayerFade(delta);
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void InitializeRig()
    {
        //Note/Todo: Rewrite this to account for different rig starting heights for characters, vehicles, etc.
        Position = new Vector3(0f, 1.744f, 0f);

        _rigPitch = Rotation.X;
        _rigYaw = Rotation.Y;

        UpdateCameraTargets();

        _currentPitchLimits = _targetPitchLimits;
    }

    private void InitializeCamera()
    {
        _camera.Position = new Vector3(
            _targetCameraHorizontalOffset,
            0f,
            _targetCameraDistance
        );

        _camera.Rotation = Vector3.Zero;
    }

    private void SetupPlayerMaterial()
    {
        if (_player == null)
            return;

        if (_player.GetActiveMaterial(0) is not StandardMaterial3D material)
            return;

        var transparentMaterial = (StandardMaterial3D)material.Duplicate();

        transparentMaterial.Transparency =
            BaseMaterial3D.TransparencyEnum.Alpha;
        transparentMaterial.DepthDrawMode =
            BaseMaterial3D.DepthDrawModeEnum.Always;

        _player.SetSurfaceOverrideMaterial(0, transparentMaterial);


        if (_player.GetActiveMaterial(1) is not StandardMaterial3D material2)
            return;

        var transparentMaterial2 = (StandardMaterial3D)material2.Duplicate();

        transparentMaterial2.Transparency =
            BaseMaterial3D.TransparencyEnum.Alpha;
        transparentMaterial2.DepthDrawMode =
            BaseMaterial3D.DepthDrawModeEnum.Always;

        _player.SetSurfaceOverrideMaterial(1, transparentMaterial2);


        _playerShadowMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
    }

    // -------------------------------------------------------------------------
    // Camera Mode
    // -------------------------------------------------------------------------

    private void HandleCameraModeInput()
    {
        if (Input.IsActionJustPressed("cam_in"))
            SetCameraMode(_cameraMode - 1);

        if (Input.IsActionJustPressed("cam_out"))
            SetCameraMode(_cameraMode + 1);

    }

    private void SetCameraMode(CamMode mode)
    {
        mode = ClampCameraMode(mode);

        if (_cameraMode == mode)
            return;

        _previousCameraMode = _cameraMode;
        _cameraMode = mode;
        UpdateCameraTargets();
    }

    private void UpdateCameraTargets()
    {
        _targetCameraDistance = GetCameraDistance();
        _targetCameraHorizontalOffset = GetCameraHorizontalOffset();
        _targetPitchLimits = GetPitchLimits();
    }

    private static CamMode ClampCameraMode(CamMode mode)
    {
        return (CamMode)Mathf.Clamp(
            (int)mode,
            (int)CamMode.FirstPerson,
            (int)CamMode.Lakitu
        );
    }

    private float GetCameraDistance()
    {
        return _cameraMode switch
        {
            CamMode.FirstPerson => _firstPersonDistance,
            CamMode.CloseUp => _closeUpDistance,
            CamMode.Lakitu => _lakituDistance,
            _ => _lakituDistance
        };
    }

    private float GetCameraHorizontalOffset()
    {
        return _cameraMode switch
        {
            CamMode.CloseUp => _closeUpHorizontalOffset,
            _ => 0f
        };
    }

    private Vector2 GetPitchLimits()
    {
        return _cameraMode switch
        {
            CamMode.FirstPerson => _firstPersonPitchLimits,
            CamMode.CloseUp => _closeUpPitchLimits,
            CamMode.Lakitu => _lakituPitchLimits,
            _ => _lakituPitchLimits
        };
    }

    // -------------------------------------------------------------------------
    // Camera Transition
    // -------------------------------------------------------------------------

    private void UpdateCameraTransition(double delta)
    {
        float step = _cameraTransitionSpeed * (float)delta;

        float newX = Mathf.MoveToward(
            _camera.Position.X,
            _targetCameraHorizontalOffset,
            step
        );

        float newZ = Mathf.MoveToward(
            _camera.Position.Z,
            _targetCameraDistance,
            step
        );

        _camera.Position = new Vector3(
            newX,
            _camera.Position.Y,
            newZ
        );
    }

    // -------------------------------------------------------------------------
    // Pitch
    // -------------------------------------------------------------------------

    private void UpdatePitchLimitTransition(double delta)
    {
        float step = _cameraPitchTransitionSpeed * (float)delta;

        _currentPitchLimits.X = Mathf.MoveToward(
            _currentPitchLimits.X,
            _targetPitchLimits.X,
            step
        );

        _currentPitchLimits.Y = Mathf.MoveToward(
            _currentPitchLimits.Y,
            _targetPitchLimits.Y,
            step
        );
    }

    private void HandleRotationInput(double delta)
    {
        float rotationAmount =
            _cameraRotationPerSecond * 360f * (float)delta;

        float horizontalInput =
            Input.GetAxis("cam_left", "cam_right");

        float verticalInput =
            Input.GetAxis("cam_up", "cam_down");

        _rigYaw -= horizontalInput * rotationAmount;
        _rigPitch -= verticalInput * rotationAmount;

        CorrectPitchIfOutOfBounds(delta);

        Rotation = new Vector3(
            Mathf.DegToRad((float)_rigPitch),
            Mathf.DegToRad((float)_rigYaw),
            0f
        );
    }

    private void CorrectPitchIfOutOfBounds(double delta)
    {
        float correctionStep =
            _cameraPitchCorrectionSpeed * (float)delta;

        if (_rigPitch < _currentPitchLimits.X)
        {
            _rigPitch = Mathf.MoveToward(
                (float)_rigPitch,
                _currentPitchLimits.X,
                correctionStep
            );
        }
        else if (_rigPitch > _currentPitchLimits.Y)
        {
            _rigPitch = Mathf.MoveToward(
                (float)_rigPitch,
                _currentPitchLimits.Y,
                correctionStep
            );
        }

        _rigPitch = Mathf.Clamp(
            (float)_rigPitch,
            _currentPitchLimits.X,
            _currentPitchLimits.Y
        );
    }

    // -------------------------------------------------------------------------
    // Player Fade
    // -------------------------------------------------------------------------

    private void UpdatePlayerFade(double delta)
    {
        float targetAlpha = GetTargetPlayerAlpha();

        _playerAlpha = Mathf.MoveToward(
            _playerAlpha,
            targetAlpha,
            _playerFadeSpeed * (float)delta
        );

        SetPlayerAlpha(_playerAlpha);
    }

    private float GetTargetPlayerAlpha()
    {
        // Leaving first person: wait for the camera to move away
        // before fading the player back in.
        if (_previousCameraMode == CamMode.FirstPerson &&
            _cameraMode != CamMode.FirstPerson)
        {
            float fadeRange =
                GetCameraDistance() - _playerFadeInDistance;

            if (Mathf.IsZeroApprox(fadeRange))
                return 1f;

            return Mathf.Clamp(
                (_camera.Position.Z - _playerFadeInDistance) / fadeRange,
                0f,
                1f
            );
        }

        // Entering first person: fade the player out as the camera
        // approaches the player.
        if (_cameraMode == CamMode.FirstPerson)
        {
            return Mathf.Clamp(
                _camera.Position.Z / _playerFadeOutDistance,
                0f,
                1f
            );
        }

        return 1f;
    }

    private void SetPlayerAlpha(float alpha)
    {
        if (_player == null)
            return;

        if (_player.GetActiveMaterial(0) is not StandardMaterial3D material)
            return;

        if (alpha >= 1)
            _playerShadowMesh.Visible = true;
        else
            _playerShadowMesh.Visible = false;

        Color color = material.AlbedoColor;
        color.A = alpha;
        material.AlbedoColor = color;
    }
}