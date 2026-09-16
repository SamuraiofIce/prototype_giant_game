using Godot;

/// <summary>
/// Handles a Camera Rig setup, where where a basic Node3D is used to assist 
/// in positioning and controlling the camera as it rotates around the active player.
/// </summary>
public partial class PlayerCamRig : Node3D
{
    /// <summary>
    /// Modes defining camera position/behavior. Lakitu behaves like a 
    /// "standard" 3D camera, akin to the behavior of Lakitu from Mario 64. 
    /// FirstPerson and OverShoulder should be self explanatory. 
    /// </summary>
    public enum CamMode
    {
        FirstPerson,
        OverShoulder,
        Lakitu
    }

    // -------------------------------------------------------------------------
    // References
    // -------------------------------------------------------------------------
    [ExportGroup("References")]
    [Export] private Camera3D _camera;
    [Export] private MeshInstance3D _player;
    [Export] private MeshInstance3D _playerShadowMesh;

    // -------------------------------------------------------------------------
    // Camera Distances
    // -------------------------------------------------------------------------
    [ExportGroup("Camera")]
    [ExportSubgroup("Camera Distances")]
    [Export] private float _firstPersonDistance = 0f;
    [Export] private float _closeUpDistance = 1.2f;
    [Export] private float _lakituDistance = 4.3f;

    // -------------------------------------------------------------------------
    // Camera Movement
    // -------------------------------------------------------------------------
    [ExportSubgroup("Camera Movement and Adjustment Properties")]
    /// <summary>
    /// Defines how fast the camera transitions between camera modes.
    /// </summary>
    [Export] private float _cameraTransitionSpeed = 15f;
    [Export] private float _cameraRotationPerSecond = 1.5f;
    /// <summary>
    /// Defines how fast the camera transitions its pitch when adjusting for limits set between modes.
    /// </summary>
    /// <remarks>
    /// For smoother camera transitions, set this lower. 
    /// </remarks>
    [Export] private float _cameraPitchTransitionSpeed = 100f;
    [Export] private float _cameraPitchCorrectionSpeed = 90f;

    [Export] private float _closeUpHorizontalOffset = 0.75f;

    // -------------------------------------------------------------------------
    // Camera Pitch Limits
    // -------------------------------------------------------------------------
    [ExportSubgroup("Camera Pitch Limits")]
    [Export] private Vector2 _firstPersonPitchLimits = new(-89f, 89f);
    [Export] private Vector2 _closeUpPitchLimits = new(-89f, 50f);
    [Export] private Vector2 _lakituPitchLimits = new(-89f, 25f);

    // -------------------------------------------------------------------------
    // Player Fade
    // -------------------------------------------------------------------------
    [ExportGroup("Player Fade Properties")]
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

    /// <summary>
    /// Initializes the camera rig. The height/attachment of the rig is hard coded currently.
    /// This should be changed in future versions.
    /// </summary>
    private void InitializeRig()
    {
        //Note/Todo: Rewrite this to account for different rig starting heights for characters, vehicles, etc.
        Position = new Vector3(0f, 1.744f, 0f);

        _rigPitch = Rotation.X;
        _rigYaw = Rotation.Y;

        UpdateCameraTargets();

        _currentPitchLimits = _targetPitchLimits;
    }

    /// <summary>
    /// Initializes the camera, setting its starting position along the rig.
    /// </summary>
    private void InitializeCamera()
    {
        _camera.Position = new Vector3(
            _targetCameraHorizontalOffset,
            0f,
            _targetCameraDistance
        );

        _camera.Rotation = Vector3.Zero;
    }

    /// <summary>
    /// Initializes an override to the player's material by copying the base material. 
    /// This allows the rig to interpolate the alpha value of the material so that the player
    /// fades properly as the camera moves to first person.
    /// </summary>
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

    /// <summary>
    /// Checks the inputs and calls SetCameraMode() as appropriate. Should be self explanatory?
    /// </summary>
    private void HandleCameraModeInput()
    {
        if (Input.IsActionJustPressed("cam_in"))
            SetCameraMode(_cameraMode - 1);

        if (Input.IsActionJustPressed("cam_out"))
            SetCameraMode(_cameraMode + 1);

    }

    /// <summary>
    /// Sets the camera mode, clamping the values. Also tracks the previous camera mode
    /// in case we need it for things, specifically finding the target alpha value for the player material.
    /// </summary>
    /// <param name="mode">The target camera mode.</param>
    private void SetCameraMode(CamMode mode)
    {
        mode = ClampCameraMode(mode);

        if (_cameraMode == mode)
            return;

        _previousCameraMode = _cameraMode;
        _cameraMode = mode;
        UpdateCameraTargets();
    }

    /// <summary>
    /// Updates the target distance, horizontal offset, and pitch limits for the camera.
    /// </summary>
    /// <remarks>
    /// Distance and horizontal offset are interpolated as the cam is adjusted, hence why they are targets.
    /// </remarks>
    private void UpdateCameraTargets()
    {
        _targetCameraDistance = GetCameraDistance();
        _targetCameraHorizontalOffset = GetCameraHorizontalOffset();
        _targetPitchLimits = GetPitchLimits();
    }

    /// <summary>
    /// Clamps the camera mode to valid enum values, to make sure that we never give an invalid mode.
    /// </summary>
    /// <param name="mode">The camera mode we want to clamp.</param>
    /// <returns>The clamped camera mode.</returns>
    private static CamMode ClampCameraMode(CamMode mode)
    {
        return (CamMode)Mathf.Clamp(
            (int)mode,
            (int)CamMode.FirstPerson,
            (int)CamMode.Lakitu
        );
    }

    /// <summary>
    /// Returns the intended camera distance, based on the current camera mode.
    /// </summary>
    /// <returns>(float) The intended camera distance, based on the current camera mode. (Should I really be doubling up on this description?)</returns>
    private float GetCameraDistance()
    {
        return _cameraMode switch
        {
            CamMode.FirstPerson => _firstPersonDistance,
            CamMode.OverShoulder => _closeUpDistance,
            CamMode.Lakitu => _lakituDistance,
            _ => _lakituDistance
        };
    }

    /// <summary>
    /// Returns the intended horizontal offset for the camera. Right now, the only mode
    /// that this effects is OverShoulder...
    /// </summary>
    /// <returns>(float) The intended horizontal offset for the camera.</returns>
    private float GetCameraHorizontalOffset()
    {
        return _cameraMode switch
        {
            CamMode.OverShoulder => _closeUpHorizontalOffset,
            _ => 0f
        };
    }

    /// <summary>
    /// Returns a Vec2 with the current pitch limits on camera rotation, based on the current cam mode.
    /// </summary>
    /// <returns>(Vector2) Current pitch limits on camera rotation based on current cam mode.</returns>
    private Vector2 GetPitchLimits()
    {
        return _cameraMode switch
        {
            CamMode.FirstPerson => _firstPersonPitchLimits,
            CamMode.OverShoulder => _closeUpPitchLimits,
            CamMode.Lakitu => _lakituPitchLimits,
            _ => _lakituPitchLimits
        };
    }

    // -------------------------------------------------------------------------
    // Camera Transition
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the camera transition for interpolating between camera modes.
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
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

    /// <summary>
    /// Updates the camera pitch limits for interpolating between camera modes.
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
    /// <remarks>
    /// This interpolation is necessary to keep the camera from being too jumpy, 
    /// giving it a more natrual transition.
    /// </remarks>
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

    /// <summary>
    /// Handles the horizontal and vertical cam inputs to adjust the rotation of the rig.
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
    private void HandleRotationInput(double delta)
    {
        float rotationAmount =
            _cameraRotationPerSecond * 360f * (float)delta;

        float horizontalInput =
            Input.GetAxis("cam_left", "cam_right");

        float verticalInput =
            Input.GetAxis("cam_up", "cam_down");

        /* QwenQwen genned this, this might not be bad later, but we'll have to see first.
        // Adjust camera based on current state if needed
        if (_stateMachine != null && _stateMachine.GetCurrentState() != null)
        {
            BaseState currentState = _stateMachine.GetCurrentState();
            
            // Let the current state handle its own camera adjustments
            currentState.HandleInput(delta);
        }
        */

        _rigYaw -= horizontalInput * rotationAmount;
        _rigPitch -= verticalInput * rotationAmount;

        CorrectPitchIfOutOfBounds(delta);

        Rotation = new Vector3(
            Mathf.DegToRad((float)_rigPitch),
            Mathf.DegToRad((float)_rigYaw),
            0f
        );
    }

    /// <summary>
    /// Helper method for correcting the pitch based on the pitch limits.
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
    /// <remarks>
    /// This affects the _rigPitch, not the Rotation directly. This ensures that it doesn't mess with
    /// any interpolation happening, and helps prevent letting the camera swing or ever hit outside of the intended bounds.
    /// Probably not completely necessary that it has its own method, but I like it for readability and finite control over 
    /// the logic here.
    /// </remarks>
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

    /// <summary>
    /// Updates the player's current alpha/transparency value. 
    /// </summary>
    /// <param name="delta">This frame's delta, per _process().</param>
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

    /// <summary>
    /// Finds the alpha value of the player that we should interpolate towards.
    /// </summary>
    /// <remarks>
    /// This method should stay separate from UpdatePlayerFade() to ensure that we
    /// can add additional logic more easily in the future. This might include situations
    /// such as giving the player an invisibility power up, where we might want them to be 
    /// slightly transparent even in lakitu cam.
    /// </remarks>
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


    /// <summary>
    /// Sets the alpha value of the player's material(s). Also activates their shadow mesh if they're fully visible.
    /// </summary>
    /// <param name="alpha">The alpha value that the player's material(s) should be set to.</param>
    /// <remarks>
    /// This focuses on adjusting the alpha values of the albedo map only. Details will still be preserved.
    /// </remarks>
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