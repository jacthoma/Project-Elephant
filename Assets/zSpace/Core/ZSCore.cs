using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ZSCore : MonoBehaviour
{
    #region ENUMERATIONS

    public enum Eye
    {
        Left = 0,
        Right = 1,
        Center = 2,
        NumEyes
    }

    public enum CameraType
    {
        Left  = 0,
        Right = 1,
        Final = 2,
        NumTypes
    }

    public enum TrackerTargetType
    {
        Unknown = -1,
        Head = 0,
        Primary = 1,
        Secondary = 2,
        NumTypes
    }

    public enum GlPluginEventType
    {
        SelectLeftEye = 0,
        SelectRightEye = 1,
        FrameDone = 2,
        DisableStereo = 3,
        InitializeLRDetectFullscreen = 4,
        InitializeLRDetectWindowed = 5,
        UpdateLRDetect = 6
    }

    public enum StylusLedColor
    {
        Black = 0,
        White = 1,
        Red = 2,
        Green = 3,
        Blue = 4,
        Cyan = 5,
        Magenta = 6,
        Yellow = 7
    }

    #endregion


    #region UNITY_EDITOR

    public GameObject CurrentCamera  = null;

    public bool EnableStereo         = true;
    public bool EnableHeadTracking   = true;
    public bool EnableStylusTracking = true;
    public bool EnableMouseEmulation = false;

    [Range(0.01f, 0.2f)]
    public float InterPupillaryDistance = 0.06f;

    [Range(0, 1)]
    public float StereoLevel = 1;

    [Range(0, 1)]
    public float HeadTrackingScale = 1;

    [Range(0.01f, 1000.0f)]
    public float WorldScale = 1;

    [Range(0.01f, 1000.0f)]
    public float FieldOfViewScale = 1;

    #endregion


    #region UNITY_CALLBACKS

    void Awake()
    {
        // Grab the ZSCoreSingleton and verify that it is initialized.
        _coreSingleton = ZSCoreSingleton.Instance;

        // If the CurrentCamera is null, default to Camera.main.
        if (!this.IsCurrentCameraValid() && Camera.main != null)
            this.CurrentCamera = Camera.main.gameObject;

        // Initialization.
        this.Initialize();
        this.InitializeStereoCameras();
        this.CheckForUpdates();

        // Temporarily re-enable the camera in case other MonoBehaviour scripts
        // want to reference Camera.main in their Awake() method.
        if (this.IsCurrentCameraValid())
            this.CurrentCamera.camera.enabled = true;

        if (_coreSingleton.IsInitialized)
        {
            // Set the window size.
            zsup_setWindowSize(Screen.width, Screen.height);

            // Start the update coroutine.
            StartCoroutine("UpdateCoroutine");
        }
        else
        {
            Debug.Log("Could not initialize zSpace. Please confirm required plugin DLLs are present.");
        }
    }

    void OnDestroy()
    {
        // Stop the update coroutine.
        StopCoroutine("UpdateCoroutine");

        if (_coreSingleton.IsInitialized)
        {
            GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.DisableStereo);
            GL.InvalidateState();
        }
    }

    void LateUpdate()
    {
        if (this.IsStereoEnabled())
            CurrentCamera.camera.enabled = false;
    }		

    #endregion


    #region ZSPACE_APIS

    /// <summary>
    /// Set whether or not stereoscopic 3D is enabled.
    /// </summary>
    /// <param name="isStereoEnabled">True to enable stereoscopic 3D.  False otherwise.</param>
    public void SetStereoEnabled(bool isStereoEnabled)
    {
        if (!_coreSingleton.IsInitialized)
            return;

        if (this.IsCurrentCameraValid())
        {
            this.CurrentCamera.camera.enabled = !isStereoEnabled;

            if (_stereoCameras[(int)CameraType.Left] != null)
                _stereoCameras[(int)CameraType.Left].enabled = isStereoEnabled;

            if (_stereoCameras[(int)CameraType.Right] != null)
                _stereoCameras[(int)CameraType.Right].enabled = isStereoEnabled;

            this.EnableStereo = isStereoEnabled;
            _isStereoEnabled  = isStereoEnabled;
        }
    }

    /// <summary>
    /// Check whether or not stereoscopic 3D rendering is enabled.
    /// </summary>
    /// <returns>True if stereoscopic 3D is enabled.  False if not.</returns>
    public bool IsStereoEnabled()
    {
        return _isStereoEnabled;
    }

    /// <summary>
    /// Set whether or not the left and right eyes are swapped.
    /// </summary>
    /// <param name="areEyesSwapped">Whether or not the left and right eyes are swapped.</param>
    public void SetEyesSwapped(bool areEyesSwapped)
    {
        _areEyesSwapped = areEyesSwapped;
    }

    /// <summary>
    /// Check whether or not the left and right eyes are swapped.
    /// </summary>
    /// <returns>Whether or not the left and right eyes are swapped.</returns>
    public bool AreEyesSwapped()
    {
        return _areEyesSwapped;
    }

    /// <summary>
    /// Get the offset of the current display.
    /// </summary>
    /// <returns>The display offset (in meters) in Vector3 format.</returns>
    public Vector3 GetDisplayOffset()
    {
        float[] displayOffsetData = new float[3];
        zsup_getDisplayOffset(displayOffsetData);
        return this.ConvertToVector3(displayOffsetData);
    }

    /// <summary>
    /// Get the virtual (x, y) position of the current display.
    /// </summary>
    /// <returns>The display position (virtual x, y coordinates) in Vector2 format.</returns>
    public Vector2 GetDisplayPosition()
    {
        float[] displayPositionData = new float[2];
        zsup_getDisplayPosition(displayPositionData);
        return this.ConvertToVector2(displayPositionData);
    }

    /// <summary>
    /// Get the angle of the current display.
    /// </summary>
    /// <returns>The display angle (in degrees) in Vector2 format.</returns>
    public Vector2 GetDisplayAngle()
    {
        float[] displayAngleData = new float[2];
        zsup_getDisplayAngle(displayAngleData);
        return this.ConvertToVector2(displayAngleData);
    }

    /// <summary>
    /// Get the resolution of the current display.
    /// </summary>
    /// <returns>The display resolution (in pixels) in Vector2 format.</returns>
    public Vector2 GetDisplayResolution()
    {
        float[] displayResolutionData = new float[2];
        zsup_getDisplayResolution(displayResolutionData);
        return this.ConvertToVector2(displayResolutionData);
    }

    /// <summary>
    /// Get the size of the current display.
    /// </summary>
    /// <returns>The display size (in meters) in Vector2 format.</returns>
    public Vector2 GetDisplaySize()
    {
        float[] displaySizeData = new float[2];
        zsup_getDisplaySize(displaySizeData);
        return this.ConvertToVector2(displaySizeData);
    }

    /// <summary>
    /// Get the stereo window's x position.
    /// </summary>
    /// <returns>The left-most position in absolute Window's screen coordinates.</returns>
    public int GetWindowX()
    {
        return zsup_getWindowX();
    }

    /// <summary>
    /// Get the stereo window's y position.
    /// </summary>
    /// <returns>The top-most position in absolute Window's screen coordinates.</returns>
    public int GetWindowY()
    {
        return zsup_getWindowY();
    }

    /// <summary>
    /// Get the stereo window's width.
    /// </summary>
    /// <returns>Window width in pixels.</returns>
    public int GetWindowWidth()
    {
        return zsup_getWindowWidth();
    }

    /// <summary>
    /// Get the stereo window's height.
    /// </summary>
    /// <returns>Window height in pixels.</returns>
    public int GetWindowHeight()
    {
        return zsup_getWindowHeight();
    }

    /// <summary>
    /// Set the inter-pupillary distance - the physical distance between the user's eyes.
    /// </summary>
    /// <param name="interPupillaryDistance">The inter-pupillary distance (in meters).</param>
    public void SetInterPupillaryDistance(float interPupillaryDistance)
    {
        this.InterPupillaryDistance = interPupillaryDistance;
        zsup_setInterPupillaryDistance(interPupillaryDistance);
    }

    /// <summary>
    /// Get the inter-pupillary distance - the physical distance between the user's eyes.
    /// </summary>
    /// <returns>The inter-pupillary distance (in meters).</returns>
    public float GetInterPupillaryDistance()
    {
        return zsup_getInterPupillaryDistance();
    }

    /// <summary>
    /// Set the stereo level.
    /// </summary>
    /// <param name="stereoLevel">
    /// The stereo level from 0.0f to 1.0f.  A stereo level of 1.0f represents
    /// full stereo.  A stereo level of 0.0f represents no stereo.
    /// </param>
    public void SetStereoLevel(float stereoLevel)
    {
        this.StereoLevel = stereoLevel;
        zsup_setStereoLevel(stereoLevel);
    }

    /// <summary>
    /// Get the stereo level.
    /// </summary>
    /// <returns>
    /// The stereo level from 0.0f to 1.0f.  A stereo level of 1.0f represents
    /// full stereo.  A stereo level of 0.0f represents no stereo.
    /// </returns>
    public float GetStereoLevel()
    {
        return zsup_getStereoLevel();
    }

    /// <summary>
    /// Set the world scale.
    /// </summary>
    /// <param name="worldScale">The world scale.</param>
    public void SetWorldScale(float worldScale)
    {
        this.WorldScale = worldScale;
        zsup_setWorldScale(worldScale);
    }

    /// <summary>
    /// Get the world scale.
    /// </summary>
    /// <returns>The world scale.</returns>
    public float GetWorldScale()
    {
        return zsup_getWorldScale();
    }

    /// <summary>
    /// Set the field of view scale.
    /// </summary>
    /// <param name="fieldOfViewScale">The field of view scale.</param>
    public void SetFieldOfViewScale(float fieldOfViewScale)
    {
        this.FieldOfViewScale = fieldOfViewScale;
        zsup_setFieldOfViewScale(fieldOfViewScale);
    }

    /// <summary>
    /// Get the field of view scale.
    /// </summary>
    /// <returns>The field of view scale.</returns>
    public float GetFieldOfViewScale()
    {
        return zsup_getFieldOfViewScale();
    }

    /// <summary>
    /// Set the zero parallax offset.
    /// </summary>
    /// <param name="zeroParallaxOffset">The zero parallax offset.</param>
    public void SetZeroParallaxOffset(float zeroParallaxOffset)
    {
        zsup_setZeroParallaxOffset(zeroParallaxOffset);
    }

    /// <summary>
    /// Get the zero parallax offset.
    /// </summary>
    /// <returns>The zero parallax offset.</returns>
    public float GetZeroParallaxOffset()
    {
        return zsup_getZeroParallaxOffset();
    }

    /// <summary>
    /// Set the near clip distance.
    /// </summary>
    /// <param name="nearClip">The near clip distance (in meters).</param>
    public void SetNearClip(float nearClip)
    {
        zsup_setNearClip(nearClip);

        if (_stereoCameras[(int)CameraType.Left] != null)
            _stereoCameras[(int)CameraType.Left].nearClipPlane = nearClip;

        if (_stereoCameras[(int)CameraType.Right] != null)
            _stereoCameras[(int)CameraType.Right].nearClipPlane = nearClip;
    }

    /// <summary>
    /// Get the near clip distance.
    /// </summary>
    /// <returns>The near clip distance (in meters).</returns>
    public float GetNearClip()
    {
        return zsup_getNearClip();
    }

    /// <summary>
    /// Set the far clip distance.
    /// </summary>
    /// <param name="farClip">The far clip distance (in meters).</param>
    public void SetFarClip(float farClip)
    {
        zsup_setFarClip(farClip);

        if (_stereoCameras[(int)CameraType.Left] != null)
            _stereoCameras[(int)CameraType.Left].farClipPlane = farClip;

        if (_stereoCameras[(int)CameraType.Right] != null)
            _stereoCameras[(int)CameraType.Right].farClipPlane = farClip;
    }

    /// <summary>
    /// Get the far clip distance.
    /// </summary>
    /// <returns>The far clip distance (in meters).</returns>
    public float GetFarClip()
    {
        return zsup_getFarClip();
    }

    /// <summary>
    /// Get the view matrix for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The view matrix in Matrix4x4 format.</returns>
    public Matrix4x4 GetViewMatrix(Eye eye)
    {
        return _viewMatrices[(int)eye];
    }

    /// <summary>
    /// Get the projection matrix for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The projection matrix in Matrix4x4 format.</returns>
    public Matrix4x4 GetProjectionMatrix(Eye eye)
    {
        return _projectionMatrices[(int)eye];
    }

    /// <summary>
    /// Get the position of a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <returns>The position of the eye in Vector3 format.</returns>
    public Vector3 GetEyePosition(Eye eye)
    {
        float[] positionData = new float[3];
        zsup_getEyePosition((int)eye, positionData);
        return this.ConvertToVector3(positionData);
    }

    /// <summary>
    /// Get the frustum bounds for a specified eye.
    /// </summary>
    /// <param name="eye">The eye: left, right, or center.</param>
    /// <param name="bounds">The frustum bounds corresponding to a specified eye laid out as follows:\n\n
    /// [left, right, bottom, top, nearClip, farClip]</param>
    public void GetFrustumBounds(Eye eye, float[/*6*/] bounds)
    {
        zsup_getFrustumBounds((int)eye, bounds);
    }

    /// <summary>
    /// Set whether or not head tracking is enabled.
    /// </summary>
    /// <param name="isHeadTrackingEnabled">Flag to specify whether or not head tracking is enabled.</param>
    public void SetHeadTrackingEnabled(bool isHeadTrackingEnabled)
    {
        this.EnableHeadTracking = isHeadTrackingEnabled;
        zsup_setHeadTrackingEnabled(isHeadTrackingEnabled);
    }

    /// <summary>
    /// Check if head tracking is enabled.
    /// </summary>
    /// <returns>True if head tracking is enabled.  False if not.</returns>
    public bool IsHeadTrackingEnabled()
    {
        return zsup_isHeadTrackingEnabled();
    }

    /// <summary>
    /// Set the uniform scale that is to be applied to the head tracked position.
    /// </summary>
    /// <param name="headTrackingScale">The scale applied to head tracking.</param>
    public void SetHeadTrackingScale(float headTrackingScale)
    {
        this.HeadTrackingScale = headTrackingScale;
        zsup_setHeadTrackingScale(headTrackingScale);
    }

    /// <summary>
    /// Get the uniform scale that is applied to the head tracked position.
    /// </summary>
    /// <returns>The scale applied to head tracking.</returns>
    public float GetHeadTrackingScale()
    {
        return zsup_getHeadTrackingScale();
    }

    /// <summary>
    /// Set whether or not stylus tracking is enabled.
    /// </summary>
    /// <param name="isStylusTrackingEnabled">Flag to specify whether or not to enabled stylus tracking.</param>
    public void SetStylusTrackingEnabled(bool isStylusTrackingEnabled)
    {
        this.EnableStylusTracking = isStylusTrackingEnabled;
        zsup_setStylusTrackingEnabled(isStylusTrackingEnabled);
    }

    /// <summary>
    /// Check whether or not stylus tracking is enabled.
    /// </summary>
    /// <returns>True if stylus tracking is enabled.  False if not.</returns>
    public bool IsStylusTrackingEnabled()
    {
        return zsup_isStylusTrackingEnabled();
    }

    /// <summary>
    /// Set whether or no mouse emulation is enabled.
    /// </summary>
    /// <param name="isMouseEmulationEnabled">True to enable mouse emulation, false otherwise.</param>
    public void SetMouseEmulationEnabled(bool isMouseEmulationEnabled)
    {
        this.EnableMouseEmulation = isMouseEmulationEnabled;
        zsup_setMouseEmulationEnabled(isMouseEmulationEnabled);
    }

    /// <summary>
    /// Check whether or not mouse emulation is enabled.
    /// </summary>
    /// <returns>True if mouse emulation is enabled.  False if not.</returns>
    public bool IsMouseEmulationEnabled()
    {
        return zsup_isMouseEmulationEnabled();
    }

    /// <summary>
    /// Set the distance at which mouse emulation will be enabled.
    /// </summary>
    /// <param name="mouseEmulationDistance">The mouse emulation distance.</param>
    public void SetMouseEmulationDistance(float mouseEmulationDistance)
    {
        zsup_setMouseEmulationDistance(mouseEmulationDistance);
    }

    /// <summary>
    /// Get the distance at which mouse emulation will be enabled.
    /// </summary>
    /// <returns>The mouse emulation distance.</returns>
    public float GetMouseEmulationDistance()
    {
        return zsup_getMouseEmulationDistance();
    }

    /// <summary>
    /// Set whether or not the LED on the stylus is enabled.
    /// </summary>
    /// <param name="isStylusLedEnabled">Whether or not to enable the stylus LED.</param>
    public void SetStylusLedEnabled(bool isStylusLedEnabled)
    {
        zsup_setStylusLedEnabled(isStylusLedEnabled);
    }

    /// <summary>
    /// Check whether or not the LED on the stylus is enabled.
    /// </summary>
    /// <returns>Whether or not the stylus LED is enabled.</returns>
    public bool IsStylusLedEnabled()
    {
        return zsup_isStylusLedEnabled();
    }

    /// <summary>
    /// Set the stylus LED color.
    /// </summary>
    /// <param name="stylusLedColor">The stylus LED color.</param>
    public void SetStylusLedColor(StylusLedColor stylusLedColor)
    {
        zsup_setStylusLedColor(_stylusLedColors[(int)stylusLedColor]);
    }

    /// <summary>
    /// Get the stylus LED color.
    /// </summary>
    /// <returns>The current color of the stylus LED.</returns>
    public StylusLedColor GetStylusLedColor()
    {
        int[] stylusLedColor = new int[3];
        zsup_getStylusLedColor(stylusLedColor);

        for (int i = 0; i < _stylusLedColors.Count; ++i)
        {
            int[] color = _stylusLedColors[i];

            if (stylusLedColor[0] == color[0] && stylusLedColor[1] == color[1] && stylusLedColor[2] == color[2])
                return (StylusLedColor)i;
        }

        return StylusLedColor.Black;
    }

    /// <summary>
    /// Set whether or not stylus vibration is enabled.  This only determines
    /// whether the appropriate command is sent to the hardware if StartStylusVibration()
    /// is called.  If the stylus is already vibrating, StopStylusVibration() should
    /// be called to stop the current vibration.
    /// </summary>
    /// <param name="isStylusVibrationEnabled">Whether or not stylus vibration is enabled.</param>
    public void SetStylusVibrationEnabled(bool isStylusVibrationEnabled)
    {
        zsup_setStylusVibrationEnabled(isStylusVibrationEnabled);
    }

    /// <summary>
    /// Check whether or not stylus vibration is enabled.
    /// </summary>
    /// <returns>True if stylus vibration is enabled.  False if vibration is disabled.</returns>
    public bool IsStylusVibrationEnabled()
    {
        return zsup_isStylusVibrationEnabled();
    }

    /// <summary>
    /// Set the period for how long the stylus should vibrate.  Note, the actual period set will
    /// depend on the resolution of the motor in the stylus.
    /// </summary>
    /// <param name="stylusVibrationOnPeriod">The on period in seconds.</param>
    public void SetStylusVibrationOnPeriod(float stylusVibrationOnPeriod)
    {
        zsup_setStylusVibrationOnPeriod(stylusVibrationOnPeriod);
    }

    /// <summary>
    /// Get the on period of the stylus vibration.
    /// </summary>
    /// <returns>The on period in seconds.</returns>
    public float GetStylusVibrationOnPeriod()
    {
        return zsup_getStylusVibrationOnPeriod();
    }

    /// <summary>
    /// Set the period for how long the stylus should not vibrate.  Note, the actual period set will
    /// depend on the resolution of the motor in the stylus.
    /// </summary>
    /// <param name="stylusVibrationOffPeriod">The off period in seconds.</param>
    public void SetStylusVibrationOffPeriod(float stylusVibrationOffPeriod)
    {
        zsup_setStylusVibrationOffPeriod(stylusVibrationOffPeriod);
    }

    /// <summary>
    /// Get the off period of the stylus vibration.
    /// </summary>
    /// <returns>The off period in seconds.</returns>
    public float GetStylusVibrationOffPeriod()
    {
        return zsup_getStylusVibrationOffPeriod();
    }

    /// <summary>
    /// Set the repeat count of the stylus vibration.  
    /// 
    /// This corresponds to the number of vibration cycles that occur after the initial vibration.  
    /// If the value passed in is non-negative, one period of "on" then "off" will occur, followed by 
    /// [stylusVibrationRepeatCount] additional cycles.  If the value is negative, it will continue
    /// to vibrate indefinitely or until StopStylusVibration() is called.
    /// 
    /// (stylusVibrationRepeatCount =  0:  1 vibration + 0 additional = 1 total)
    /// 
    /// (stylusVibrationRepeatCount =  1:  1 vibration + 1 additional = 2 total)
    /// 
    /// (stylusVibrationRepeatCount = -1:  infinite)
    /// 
    /// </summary>
    /// <param name="stylusVibrationRepeatCount">The number of times the stylus vibration on/off
    /// pattern should repeat after the initial vibration.  A negative value denotes infinite repetition.</param>
    public void SetStylusVibrationRepeatCount(int stylusVibrationRepeatCount)
    {
        zsup_setStylusVibrationRepeatCount(stylusVibrationRepeatCount);
    }

    /// <summary>
    /// Get the repeat count of the stylus vibration.  This corresponds to the number of vibration cycles
    /// that occur after the initial vibration.
    /// </summary>
    /// <returns>The repeat count of the stylus vibration.</returns>
    public int GetStylusVibrationRepeatCount()
    {
        return zsup_getStylusVibrationRepeatCount();
    }

    /// <summary>
    /// Start vibrating the stylus by repeating the specified "on" and "off" cycles.
    /// </summary>
    public void StartStylusVibration()
    {
        zsup_startStylusVibration();
    }

    /// <summary>
    /// Stop vibrating the stylus if it is currently vibrating.  If StartStylusVibration() is
    /// called again, the stylus will start vibrating the full sequence of "on" and "off" cycles.
    /// </summary>
    public void StopStylusVibration()
    {
        zsup_stopStylusVibration();
    }

    /// <summary>
    /// Set whether or not secondary tracking is enabled.
    /// </summary>
    /// <param name="isSecondaryTrackingEnabled">Flag to specify whether or not to enabled secondary tracking.</param>
    public void SetSecondaryTrackingEnabled(bool isSecondaryTrackingEnabled)
    {
        zsup_setSecondaryTrackingEnabled(isSecondaryTrackingEnabled);
    }

    /// <summary>
    /// Check whether or not secondary tracking is enabled.
    /// </summary>
    /// <returns>True if secondary tracking is enabled.  False if not.</returns>
    public bool IsSecondaryTrackingEnabled()
    {
        return zsup_isSecondaryTrackingEnabled();
    }

    /// <summary>
    /// Check whether or not the pose for a specified TrackerTarget is valid
    /// for the current frame.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>True if the pose is valid, false otherwise.</returns>
    public bool IsTrackerTargetPoseValid(TrackerTargetType trackerTargetType)
    {
        return _isTrackerTargetPoseValid[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the tracker space pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>The Matrix4x4 pose in tracker space.</returns>
    public Matrix4x4 GetTrackerTargetPose(TrackerTargetType trackerTargetType)
    {
        return _trackerTargetPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the camera space pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>The Matrix4x4 pose in camera space.</returns>
    public Matrix4x4 GetTrackerTargetCameraPose(TrackerTargetType trackerTargetType)
    {
        return _trackerTargetCameraPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Get the world space pose of a specified default TrackerTarget.
    /// This forces a recalculation based on the current camera's local
    /// to world matrix.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>The Matrix4x4 pose in world space.</returns>
    public Matrix4x4 GetTrackerTargetWorldPose(TrackerTargetType trackerTargetType)
    {
        Matrix4x4 trackerTargetWorldPose = _trackerTargetCameraPoses[(int)trackerTargetType];

        // Scale the position based on world and field of view scales.
        trackerTargetWorldPose[0, 3] *= this.WorldScale * this.FieldOfViewScale;
        trackerTargetWorldPose[1, 3] *= this.WorldScale * this.FieldOfViewScale;
        trackerTargetWorldPose[2, 3] *= this.WorldScale;

        // Convert the camera space pose to world space.
        if (this.IsCurrentCameraValid())
            trackerTargetWorldPose = this.CurrentCamera.transform.localToWorldMatrix * trackerTargetWorldPose;

        return trackerTargetWorldPose;
    }

    /// <summary>
    /// Get the cached world space pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>The Matrix4x4 pose in world space.</returns>
    public Matrix4x4 GetCachedTrackerTargetWorldPose(TrackerTargetType trackerTargetType)
    {
        return _trackerTargetWorldPoses[(int)trackerTargetType];
    }

    /// <summary>
    /// Set whether or not pose buffering is enabled for a specified TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <param name="isPoseBufferingEnabled">Flag denoting whether or not to enable pose buffering.</param>
    public void SetTrackerTargetPoseBufferingEnabled(TrackerTargetType trackerTargetType, bool isPoseBufferingEnabled)
    {
        zsup_setTrackerTargetPoseBufferingEnabled((int)trackerTargetType, isPoseBufferingEnabled);
    }

    /// <summary>
    /// Check whether or not pose buffering is enabled for a specified TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <returns>True if pose buffering is enabled.  False if not.</returns>
    public bool IsTrackerTargetPoseBufferingEnabled(TrackerTargetType trackerTargetType)
    {
        return zsup_isTrackerTargetPoseBufferingEnabled((int)trackerTargetType);
    }

    /// <summary>
    /// Get the tracker space buffered pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <param name="lookBackTime"></param>
    /// <returns></returns>
    public Matrix4x4 GetTrackerTargetBufferedPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        zsup_getTrackerTargetBufferedPose((int)trackerTargetType, lookBackTime, matrixData);
        return this.ConvertToMatrix4x4(matrixData);
    }

    /// <summary>
    /// Get the camera space buffered pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <param name="lookBackTime"></param>
    /// <returns></returns>
    public Matrix4x4 GetTrackerTargetBufferedCameraPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        zsup_getTrackerTargetBufferedCameraPose((int)trackerTargetType, lookBackTime, matrixData);

        Matrix4x4 trackerTargetCameraPose = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(matrixData));
        return trackerTargetCameraPose;
    }

    /// <summary>
    /// Get the world space buffered pose of a specified default TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <param name="lookBackTime"></param>
    /// <returns></returns>
    public Matrix4x4 GetTrackerTargetBufferedWorldPose(TrackerTargetType trackerTargetType, float lookBackTime)
    {
        float[] matrixData = new float[16];
        zsup_getTrackerTargetBufferedCameraPose((int)trackerTargetType, lookBackTime, matrixData);

        Matrix4x4 trackerTargetWorldPose = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(matrixData));

        // Scale the position based on world and field of view scales.
        trackerTargetWorldPose[0, 3] *= this.WorldScale * this.FieldOfViewScale;
        trackerTargetWorldPose[1, 3] *= this.WorldScale * this.FieldOfViewScale;
        trackerTargetWorldPose[2, 3] *= this.WorldScale;

        // Convert the camera space pose to world space.
        if (this.IsCurrentCameraValid())
            trackerTargetWorldPose = CurrentCamera.transform.localToWorldMatrix * trackerTargetWorldPose;

        return trackerTargetWorldPose;
    }

    /// <summary>
    /// Get the number of buttons associated with a specified TrackerTarget.
    /// </summary>
    /// <param name="trackerTargetType">The type of the TrackerTarget.</param>
    /// <returns>The number of buttons contained by a TrackerTarget.</returns>
    public int GetNumTrackerTargetButtons(TrackerTargetType trackerTargetType)
    {
        return zsup_getNumTrackerTargetButtons((int)trackerTargetType);
    }

    /// <summary>
    /// Check whether or not a specified target button is pressed.
    /// </summary>
    /// <param name="trackerTargetType">The type of TrackerTarget.</param>
    /// <param name="buttonId">The id of the button.</param>
    /// <returns>True if the button is pressed, false otherwise.</returns>
    public bool IsTrackerTargetButtonPressed(TrackerTargetType trackerTargetType, int buttonId)
    {
        return zsup_isTrackerTargetButtonPressed((int)trackerTargetType, buttonId);
    }

    /// <summary>
    /// Get the viewport offset of the stereo window.
    /// </summary>
    /// <returns>The viewport offset.</returns>
    public Vector3 GetViewportOffset()
    {
        float[] viewportOffsetData = new float[3];
        zsup_getViewportOffset(viewportOffsetData);
        return this.ConvertToVector3(viewportOffsetData);
    }

    /// <summary>
    /// Get the matrix to convert tracker space to camera space.
    /// </summary>
    public Matrix4x4 GetTrackerToCameraSpaceTransform()
    {
        float[] matrixData = new float[16];
        zsup_getTrackerToCameraSpaceTransform(matrixData);
        return this.ConvertToMatrix4x4(matrixData);
    }

    /// <summary>
    /// Get a camera from the ZSCore stereo rig based on a
    /// specified camera type.
    /// </summary>
    /// <param name="cameraType">The camera type: Left, Right, or Final</param>
    /// <returns>Reference to the underlying Unity camera</returns>
    public Camera GetStereoCamera(CameraType cameraType)
    {
        return _stereoCameras[(int)cameraType];
    }

    /// <summary>
    /// Convert a matrix in right handed space to left handed space.
    /// </summary>
    /// <param name="rightHandMatrix">A right handed matrix.</param>
    /// <returns>A left handed matrix.</returns>
    public static Matrix4x4 ConvertFromRightToLeft(Matrix4x4 right)
    {
        return RIGHT_TO_LEFT * right * RIGHT_TO_LEFT;
    }

    #endregion


    #region EVENTS

    public delegate void CoreEventHandler(ZSCore sender);
    public event CoreEventHandler Updated;

    protected void RaiseUpdated()
    {
        if (Updated != null)
            Updated(this);
    }

    #endregion


    #region PRIVATE_HELPERS

    /// <summary>
    /// Check whether or not the CurrentCamera is valid.
    /// </summary>
    private bool IsCurrentCameraValid()
    {
        return (this.CurrentCamera != null && this.CurrentCamera.camera != null);
    }


    private void Initialize()
    {
        // Initialize the cached stereo information.
        for (int i = 0; i < (int)Eye.NumEyes; ++i)
        {
            _viewMatrices[i]        = Matrix4x4.identity;
            _projectionMatrices[i]  = Matrix4x4.identity;
        }

        // Initialize the cached tracker information.
        for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
        {
            _isTrackerTargetPoseValid[i] = false;
            _trackerTargetPoses[i]       = Matrix4x4.identity;
            _trackerTargetCameraPoses[i] = Matrix4x4.identity;
            _trackerTargetWorldPoses[i]  = Matrix4x4.identity;
        }
    }

    /// <summary>
    /// Initialize the left and right stereo cameras.
    /// </summary>
    private void InitializeStereoCameras()
    {
        _stereoCameras[(int)CameraType.Left]  = GameObject.Find("ZSLeftCamera").camera;
        _stereoCameras[(int)CameraType.Right] = GameObject.Find("ZSRightCamera").camera;
        _stereoCameras[(int)CameraType.Final] = GameObject.Find("ZSFinalCamera").camera;

        _stereoCameras[(int)CameraType.Left].enabled  = false;
        _stereoCameras[(int)CameraType.Right].enabled = false;
        _stereoCameras[(int)CameraType.Final].enabled = false;

        this.CheckCurrentCameraChanged();
    }

    /// <summary>
    /// Copy a certain subset of camera attributes from a 
    /// source camera to a destination camera.
    /// </summary>
    private void CopyCameraAttributes(Camera source, ref Camera destination)
    {
        if (source != null && destination != null)
        {
            destination.clearFlags      = source.clearFlags;
            destination.backgroundColor = source.backgroundColor;
            destination.cullingMask     = source.cullingMask;
        }
    }

    /// <summary>
    /// Check to see if the current camera has changed.
    /// </summary>
    private void CheckCurrentCameraChanged()
    {
        if (_previousCamera != this.CurrentCamera)
        {
            float currentCameraDepth = 0.0f;

            if (this.IsCurrentCameraValid())
            {
                Camera currentCamera = this.CurrentCamera.camera;

                // Grab the current camera depth.
                currentCameraDepth = currentCamera.depth;
        
                // Set the near/far clip planes.
                this.SetNearClip(currentCamera.nearClipPlane);
                this.SetFarClip(currentCamera.farClipPlane);

                // Copy a subset of camera attributes from the
                // CurrentCamera to the Left/Right cameras.
                this.CopyCameraAttributes(currentCamera, ref _stereoCameras[(int)CameraType.Left]);
                this.CopyCameraAttributes(currentCamera, ref _stereoCameras[(int)CameraType.Right]);
            }

            // Set the Left, Right, and Final Camera depth values.
            if (_stereoCameras[(int)CameraType.Left] != null)
                _stereoCameras[(int)CameraType.Left].depth = currentCameraDepth + 1.0f;

            if (_stereoCameras[(int)CameraType.Right] != null)
                _stereoCameras[(int)CameraType.Right].depth = currentCameraDepth + 2.0f;

            if (_stereoCameras[(int)CameraType.Final] != null)
                _stereoCameras[(int)CameraType.Final].depth = currentCameraDepth + 3.0f;

            _previousCamera = this.CurrentCamera;
        }
    }

    /// <summary>
    /// Check for any updates to public properties.
    /// </summary>
    private void CheckForUpdates()
    {
        if (this.EnableStereo != this.IsStereoEnabled())
            this.SetStereoEnabled(this.EnableStereo);

        if (this.EnableHeadTracking != zsup_isHeadTrackingEnabled())
            zsup_setHeadTrackingEnabled(this.EnableHeadTracking);

        if (this.EnableStylusTracking != zsup_isStylusTrackingEnabled())
            zsup_setStylusTrackingEnabled(this.EnableStylusTracking);

        if (this.EnableMouseEmulation != zsup_isMouseEmulationEnabled())
            zsup_setMouseEmulationEnabled(this.EnableMouseEmulation);

        if (this.InterPupillaryDistance != zsup_getInterPupillaryDistance())
            zsup_setInterPupillaryDistance(this.InterPupillaryDistance);

        if (this.StereoLevel != zsup_getStereoLevel())
            zsup_setStereoLevel(this.StereoLevel);

        if (this.HeadTrackingScale != zsup_getHeadTrackingScale())
            zsup_setHeadTrackingScale(this.HeadTrackingScale);

        if (this.WorldScale != zsup_getWorldScale())
            zsup_setWorldScale(this.WorldScale);

        if (this.FieldOfViewScale != zsup_getFieldOfViewScale())
            zsup_setFieldOfViewScale(this.FieldOfViewScale);
    }

    /// <summary>
    /// Update all of the stereo and tracker information.
    /// </summary>
    private void UpdateInternal()
    {
        if (_coreSingleton.IsInitialized)
        {
            this.CheckCurrentCameraChanged();
            this.CheckForUpdates();

            // Perform an update on the TrackerTargets and StereoFrustum.
            zsup_update();
            GL.IssuePluginEvent((int)GlPluginEventType.UpdateLRDetect);

            this.UpdateStereoInternal();
            this.UpdateTrackerInternal();

            // Set the final camera to be enabled so that it can reset the draw buffer
            // to the back buffer for the next frame.
            if (this.IsStereoEnabled() && _stereoCameras[(int)CameraType.Final] != null)
                _stereoCameras[(int)CameraType.Final].enabled = true;

            // Set the current camera to be enabled so that Camera.main does not return null
            // when referenced in Awake, Start, Update, etc. 
            if (this.IsStereoEnabled() && this.IsCurrentCameraValid())
                this.CurrentCamera.camera.enabled = true;

            // Raise the Updated event.
            this.RaiseUpdated();
        }
    }

    /// <summary>
    /// Update all of the stereo information.
    /// </summary>
    private void UpdateStereoInternal()
    {
        // Update the window dimensions if they have changed.
        if (Screen.width != zsup_getWindowWidth() || Screen.height != zsup_getWindowHeight())
            zsup_setWindowSize(Screen.width, Screen.height);

        // Get the view and projection matrices.
        for (int i = 0; i < (int)Eye.NumEyes; ++i)
        {
            zsup_getViewMatrix(i, _matrixData);
            _viewMatrices[i] = this.ConvertToMatrix4x4(_matrixData);

            zsup_getProjectionMatrix(i, _matrixData);
            _projectionMatrices[i] = this.ConvertToMatrix4x4(_matrixData);
        }
    }

    /// <summary>
    /// Update all of the tracker information.
    /// </summary>
    private void UpdateTrackerInternal()
    {
        // Get the tracker, camera, and world space target poses.
        for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
        {
            // Get whether or not pose is valid.
            _isTrackerTargetPoseValid[i] = zsup_isTrackerTargetPoseValid(i);

            if (_isTrackerTargetPoseValid[i])
            {
                // Tracker space poses.
                zsup_getTrackerTargetPose(i, _matrixData);
                _trackerTargetPoses[i] = this.ConvertToMatrix4x4(_matrixData);

                // Camera space poses.
                zsup_getTrackerTargetCameraPose(i, _matrixData);
                _trackerTargetCameraPoses[i] = ZSCore.ConvertFromRightToLeft(this.ConvertToMatrix4x4(_matrixData));

                // World space poses.
                _trackerTargetWorldPoses[i] = _trackerTargetCameraPoses[i];

                // Scale the position based on world and field of view scales.
                _trackerTargetWorldPoses[i][0, 3] *= this.WorldScale * this.FieldOfViewScale;
                _trackerTargetWorldPoses[i][1, 3] *= this.WorldScale * this.FieldOfViewScale;
                _trackerTargetWorldPoses[i][2, 3] *= this.WorldScale;

                // Convert the camera space pose to world space.
                if (this.IsCurrentCameraValid())
                    _trackerTargetWorldPoses[i] = this.CurrentCamera.transform.localToWorldMatrix * _trackerTargetWorldPoses[i];
            }
        }
    }

    /// <summary>
    /// Convert an array of 16 floats to Unity's Matrix4x4 format.
    /// </summary>
    /// <param name="matrixData">The matrix data stored in a float array.</param>
    /// <returns>The matrix data in Matrix4x4 format.</returns>
    private Matrix4x4 ConvertToMatrix4x4(float[/*16*/] matrixData)
    {
        Matrix4x4 matrix = new Matrix4x4();

        for (int i = 0; i < 16; i++)
            matrix[i] = matrixData[i];

        return matrix;
    }

    /// <summary>
    /// Convert an array of 2 floats to Unity's Vector2 format.
    /// </summary>
    /// <param name="vectorData">The vector data stored in a float array.</param>
    /// <returns>The vector data in Vector2 format.</returns>
    private Vector2 ConvertToVector2(float[/*2*/] vectorData)
    {
        return new Vector2(vectorData[0], vectorData[1]);
    }

    /// <summary>
    /// Convert an array of 3 floats to Unity's Vector3 format.
    /// </summary>
    /// <param name="vectorData">The vector data stored in a float array.</param>
    /// <returns>The vector data in Vector3 format.</returns>
    private Vector3 ConvertToVector3(float[/*3*/] vectorData)
    {
        return new Vector3(vectorData[0], vectorData[1], vectorData[2]);
    }

    /// <summary>
    /// The update coroutine.
    /// This will continue after the end of the frame has been hit.
    /// </summary>
    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            // Perform an update.
            this.UpdateInternal();

            // Wait for the end of the frame.
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion


    #region PRIVATE_MEMBERS

    // Constants
    private readonly static Matrix4x4 RIGHT_TO_LEFT = Matrix4x4.Scale(new Vector4(1.0f, 1.0f, -1.0f));

    private readonly static int[] BLACK     = { 0, 0, 0 };
    private readonly static int[] WHITE     = { 1, 1, 1 };
    private readonly static int[] RED       = { 1, 0, 0 };
    private readonly static int[] GREEN     = { 0, 1, 0 };
    private readonly static int[] BLUE      = { 0, 0, 1 };
    private readonly static int[] CYAN      = { 0, 1, 1 };
    private readonly static int[] MAGENTA   = { 1, 0, 1 };
    private readonly static int[] YELLOW    = { 1, 1, 0 };


    // Non-Constants
    private ZSCoreSingleton _coreSingleton   = null;

    private bool            _isStereoEnabled = false;
    private bool            _areEyesSwapped  = false;

    private float[]         _matrixData      = new float[16];

    private Matrix4x4[]     _viewMatrices       = new Matrix4x4[(int)Eye.NumEyes];
    private Matrix4x4[]     _projectionMatrices = new Matrix4x4[(int)Eye.NumEyes];

    private bool[]          _isTrackerTargetPoseValid   = new bool[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetPoses         = new Matrix4x4[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetCameraPoses   = new Matrix4x4[(int)TrackerTargetType.NumTypes];
    private Matrix4x4[]     _trackerTargetWorldPoses    = new Matrix4x4[(int)TrackerTargetType.NumTypes];

    private GameObject      _previousCamera  = null;
    private Camera[]        _stereoCameras   = new Camera[(int)CameraType.NumTypes];
    
    private List<int[]>     _stylusLedColors = new List<int[]>() { BLACK, WHITE, RED, GREEN, BLUE, CYAN, MAGENTA, YELLOW };

    #endregion


    #region ZSPACE_PLUGIN_IMPORT_DECLARATIONS

    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_initialize();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_update();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_shutdown();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getDisplayOffset([In] float[/*3*/] displayOffset);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getDisplayPosition([In] float[/*2*/] displayPosition);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getDisplayAngle([In] float[/*2*/] displayAngle);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getDisplayResolution([In] float[/*2*/] displayResolution);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getDisplaySize([In] float[/*2*/] displaySize);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setWindowPosition([In] int x, [In] int y);
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getWindowX();
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getWindowY();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setWindowSize([In] int width, [In] int height);
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getWindowWidth();
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getWindowHeight();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setInterPupillaryDistance([In] float interPupillaryDistance);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getInterPupillaryDistance();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStereoLevel([In] float stereoLevel);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getStereoLevel();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setWorldScale([In] float worldScale);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getWorldScale();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setFieldOfViewScale([In] float fieldOfViewScale);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getFieldOfViewScale();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setZeroParallaxOffset([In] float zeroParallaxOffset);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getZeroParallaxOffset();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setNearClip([In] float nearClip);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getNearClip();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setFarClip([In] float farClip);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getFarClip();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getViewMatrix([In] int eye, [Out] float[/*16*/] viewMatrix);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getProjectionMatrix([In] int eye, [Out] float[/*16*/] projectionMatrix);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getEyePosition([In] int eye, [Out] float[/*3*/] eyePosition);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getFrustumBounds([In] int eye, [Out] float[/*6*/] frustumBounds);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setHeadTrackingEnabled([In] bool isHeadTrackingEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isHeadTrackingEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setHeadTrackingScale([In] float headTrackingScale);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getHeadTrackingScale();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusTrackingEnabled([In] bool isStylusTrackingEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isStylusTrackingEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setMouseEmulationEnabled([In] bool isMouseEmulationEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isMouseEmulationEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setMouseEmulationDistance([In] float mouseEmulationDistance);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getMouseEmulationDistance();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusLedEnabled([In] bool isStylusLedEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isStylusLedEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusLedColor([In] int[/*3*/] ledColor);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getStylusLedColor([Out] int[/*3*/] ledColor);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusVibrationEnabled([In] bool isStylusVibrationEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isStylusVibrationEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusVibrationOnPeriod([In] float stylusVibrationOnPeriod);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getStylusVibrationOnPeriod();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusVibrationOffPeriod([In] float stylusVibrationOffPeriod);
    [DllImport("ZSUnityPlugin")]
    private static extern float zsup_getStylusVibrationOffPeriod();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setStylusVibrationRepeatCount([In] int stylusVibrationRepeatCount);
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getStylusVibrationRepeatCount();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_startStylusVibration();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_stopStylusVibration();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setSecondaryTrackingEnabled([In] bool isSecondaryTrackingEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isSecondaryTrackingEnabled();
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isTrackerTargetPoseValid([In] int targetType);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getTrackerTargetPose([In] int targetType, [Out] float[/*16*/] pose);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getTrackerTargetCameraPose([In] int targetType, [Out] float[/*16*/] cameraPose);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_setTrackerTargetPoseBufferingEnabled([In] int targetType, [In] bool isPoseBufferingEnabled);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isTrackerTargetPoseBufferingEnabled([In] int targetType);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getTrackerTargetBufferedPose([In] int targetType, [In] float lookBackTime, [Out] float[/*16*/] bufferedPose);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getTrackerTargetBufferedCameraPose([In] int targetType, [In] float lookBackTime, [Out] float[/*16*/] bufferedPose);
    [DllImport("ZSUnityPlugin")]
    private static extern int zsup_getNumTrackerTargetButtons([In] int targetType);
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isTrackerTargetButtonPressed([In] int targetType, [In] int buttonId);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getViewportOffset([Out] float[/*3*/] viewportOffset);
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_getTrackerToCameraSpaceTransform([Out] float[/*16*/] trackerToCameraSpaceTransform);

    #endregion
}
