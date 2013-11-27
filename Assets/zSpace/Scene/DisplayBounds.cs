////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Scales, rotates, and positions an object to cover the window surface, based on camera and display settings.
/// The resulting object or properties can be used for laying out GUI elements on the zero-parallax plane.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class DisplayBounds : MonoBehaviour
{
    /// <summary>
    /// Enumerates different ways of handling the comfort zone.
    /// </summary>
    public enum ComfortZoneMode
    {
        None = 0,
        Static = 1,
        Dynamic = 2,
    }

    /// <summary>
    /// Determines the way the comfort zone is handled.  It can be ignored, set once, or set every frame.
    /// </summary>
    public ComfortZoneMode _comfortZoneMode = ComfortZoneMode.Static;

    /// <summary> An extra border (in meters) to pad the display plane. </summary>
    public Vector2 _pad = Vector2.zero;

    /// <summary> Optional convenience object which will be automatically scaled by the zSpace world scale. </summary>
    public GameObject _worldScaleRoot;

    /// <summary> Scales the optimal comfort zone depth (or 1 if _comfortZoneMode is None) to find the actual comfort zone depth. </summary>
    public float _comfortDepthScale = 1.0f;

    protected ZSCore _core;
    protected Camera _camera;
    protected MeshFilter _meshFilter;
    protected Vector2 _displaySize;
    protected Vector2 _displayResolution;
    protected Vector2 _widgetExtents;

    protected Matrix4x4 _lastCameraProjection;
    protected bool _lastIsStereoEnabled;
    protected float _lastWorldScale;
    protected Vector3 _lastDisplayOffset;
    protected Vector2 _lastDisplayAngle;
    protected Vector2 _lastWindowResolution;
    protected Vector3 _lastCameraPosition;
    protected Quaternion _lastCameraRotation;
    protected Vector3 _lastHeadPosition;


    /// <summary> Calculates the window dimensions in world space meters. </summary>
    public Vector3 windowSize
    {
        get
        {
            Memoize();
            return _windowSize;
        }
    }

    Vector3 _windowSize = Vector3.zero;


    /// <summary> Calculates the window center in world space. </summary>
    public Vector3 windowCenter
    {
        get
        {
            Memoize();
            return _windowCenter;
        }
    }

    Vector3 _windowCenter = Vector3.zero;


    /// <summary> Calculates the window rotation in world space. </summary>
    public Quaternion windowRotation
    {
        get
        {
            Memoize();
            return _windowRotation;
        }
    }

    Quaternion _windowRotation = Quaternion.identity;


    /// <summary> Updates cached data for window properties. </summary>
    void Memoize()
    {
        Matrix4x4 cameraProjection = _camera.projectionMatrix;
        bool isStereoEnabled = _core.IsStereoEnabled();
        float worldScale = _core.GetWorldScale();
        Vector2 windowResolution = new Vector2(Screen.width, Screen.height);
        Vector3 displayOffset = _core.GetDisplayOffset();
        Vector2 displayAngle = _core.GetDisplayAngle();
        Vector3 cameraPosition = _camera.transform.position;
        Quaternion cameraRotation = _camera.transform.rotation;
        Vector3 headPosition = (_comfortZoneMode == ComfortZoneMode.Dynamic) ?
                              (Vector3)(_core.GetTrackerTargetWorldPose(ZSCore.TrackerTargetType.Head) * new Vector4(0, 0, 0, 1)) :
                              _camera.transform.position;

        bool isDirty = cameraProjection != _lastCameraProjection ||
                isStereoEnabled != _lastIsStereoEnabled ||
                worldScale != _lastWorldScale ||
                   windowResolution != _lastWindowResolution ||
                   displayOffset != _lastDisplayOffset ||
                   displayAngle != _lastDisplayAngle ||
                   cameraPosition != _lastCameraPosition ||
                   cameraRotation != _lastCameraRotation ||
                   headPosition != _lastHeadPosition;

        if (isDirty)
        {
            // Update window size.
            if (isStereoEnabled)
            {
                Vector3 dimensions = Vector3.zero;
                dimensions[0] = (_displaySize.x / _displayResolution.x) * windowResolution.x - _pad.x;
                dimensions[1] = (_displaySize.y / _displayResolution.y) * windowResolution.y - _pad.y;
    
                //Only use the near bound for now.  For z < 1 the far bound is about equal and for z > 1 it's about infinite.
                dimensions[2] = _comfortDepthScale * ComputeComfortBounds(headPosition)[0];
    
                _windowSize = worldScale * dimensions;
            }
            else
            {
                Vector3 screenPoint = new Vector3(0f, 0f, displayOffset.magnitude);
                Vector3 worldPoint = _camera.ScreenToWorldPoint(screenPoint);
                Vector3 dimensions = -2f * _camera.transform.InverseTransformPoint(worldPoint);
                dimensions[2] = _comfortDepthScale * ComputeComfortBounds(_camera.transform.position)[0];
				_windowSize = worldScale * dimensions;
            }

            // Update window center.
            if (isStereoEnabled)
            {
                Vector3 toDisplay = worldScale * displayOffset.magnitude * (cameraRotation * Vector3.forward);
                _windowCenter = cameraPosition + toDisplay;
            }
            else
            {
                Vector3 screenPoint = _camera.pixelRect.center;
                screenPoint[2] = displayOffset.magnitude;
                _windowCenter = _camera.transform.position + worldScale * (_camera.ScreenToWorldPoint(screenPoint) - _camera.transform.position);
            }
 
            // Update window rotation.
            if (isStereoEnabled)
            {
                float lookDown = Mathf.Rad2Deg * Mathf.Atan(displayOffset.y / displayOffset.z);
                float angleX = -displayAngle.x - lookDown;
                Quaternion cameraRelative = Quaternion.Euler(new Vector3(angleX, -displayAngle.y, 0.0f));
                _windowRotation = cameraRotation * cameraRelative;
            }
            else
            {
                _windowRotation = cameraRotation;
            }
     
            _lastCameraProjection = cameraProjection;
            _lastIsStereoEnabled = isStereoEnabled;
            _lastWorldScale = worldScale;
            _lastWindowResolution = windowResolution;
            _lastDisplayOffset = displayOffset;
            _lastDisplayAngle = displayAngle;
            _lastCameraPosition = cameraPosition;
            _lastCameraRotation = cameraRotation;
            _lastHeadPosition = headPosition;
        }
    }


    /// <summary>
    /// Returns approximate near and far bounds on the stereoscopic "comfort zone".
    /// The first element is the distance from the viewing plane to the near limit.
    /// The second element is the distance from the viewing plane to the far limit.
    /// </summary>
    /// <summary>
    /// Uses Shibata et al 2011.
    /// </summary>
    Vector2 ComputeComfortBounds(Vector3 headPosition)
    {
        if (_comfortZoneMode == ComfortZoneMode.None)
            return Vector2.one;
     
        float viewDistance = Vector3.Distance(headPosition, _windowCenter);
        float zMin = viewDistance * 1.035f / (1.0f + 0.626f * viewDistance);
        float zMax = viewDistance * 1.129f / (1.0f - 0.422f * viewDistance);
        return new Vector2(viewDistance - zMin, zMax - viewDistance);
    }

    void Awake()
    {
        _core = GameObject.Find("ZSCore").GetComponent<ZSCore>();
        _camera = _core.CurrentCamera.camera;
    }

    void Start()
    {
        // Get the display information
        _displaySize = _core.GetDisplaySize();
        _displayResolution = _core.GetDisplayResolution();
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        if (_worldScaleRoot != null)
        {
            Vector3 localScale = _core.GetWorldScale() * Vector3.one;
            if (_worldScaleRoot.transform.localScale != localScale)
            {
                Vector3 scaleFactor = localScale.DivideComponents(_worldScaleRoot.transform.localScale);
                _worldScaleRoot.transform.localScale = localScale;
                Vector3 offset = _worldScaleRoot.transform.position - _camera.transform.position;
                _worldScaleRoot.transform.position += (scaleFactor - Vector3.one).MultiplyComponents(offset);
            }
        }

        ApplyDisplayInfo();
    }


    /// <summary> Immediately applies the current ZSCore settings to compute the window position, size, and rotation. </summary>
    public void ApplyDisplayInfo()
    {
        transform.localScale = windowSize;
        transform.rotation = windowRotation;
        transform.position = windowCenter;
    }


    /// <summary>
    /// Scales the camera or the given "change objects" so that the given "check objects" fit into the display's comfort zone.
    /// Returns the factor of scaling that was applied.
    /// </summary>
    /// <remarks>
    /// Camera scaling can only be used if the comfort zone is None.
    /// </remarks>
    public float FitToDisplay(IEnumerable<GameObject> changeObjects, IEnumerable<GameObject> checkObjects, bool useCameraScaling)
    {
        //Make a world-space bounding box containing all of the check objects.
        Bounds sceneBounds = new Bounds();
        foreach (GameObject checkObject in checkObjects)
        {
            if (sceneBounds.extents == Vector3.zero)
                sceneBounds = checkObject.transform.ComputeBounds();
            else
                sceneBounds.Encapsulate(checkObject.transform.ComputeBounds());
        }

        Bounds localDisplayBounds = _meshFilter.mesh.bounds;

        if (sceneBounds.extents == Vector3.zero || localDisplayBounds.extents == Vector3.zero)
            return 0.0f;

        Bounds localSceneBounds = sceneBounds.Transform(transform.worldToLocalMatrix);

        //Figure out the scaling required to make it fit inside the "comfort zone".
        //TODO: Need to account for the fact that comfort zone will change in Static mode.  Dynamic mode probably can't use this.
        Vector3 localRelativeSize = new Vector3();
        for (int i = 0; i < 3; ++i)
            localRelativeSize[i] = localSceneBounds.size[i] / localDisplayBounds.size[i];

        float scaleFactor = 1.0f / Mathf.Max(new float[] {localRelativeSize.x, localRelativeSize.y, localRelativeSize.z});

        //Scale the camera or scene to apply the fit.
        if (_comfortZoneMode == ComfortZoneMode.None && useCameraScaling)
        {
            // Scale the camera so the scene fills the comfort zone.
            Physics.gravity *= scaleFactor;
            float worldScale = _core.GetWorldScale() / scaleFactor;
            _core.SetWorldScale(worldScale);

            // Translate the camera so the scene is at the center of the comfort zone.
            Vector3 cameraOffset = _camera.transform.position - transform.position;
            Vector3 cameraTranslation = sceneBounds.center + 1.0f / scaleFactor * cameraOffset - _camera.transform.position;
            _camera.transform.Translate(cameraTranslation, Space.World);
        }
        else
        {
            Bounds newSceneBounds = new Bounds();
            foreach (GameObject changeObject in changeObjects)
            {
                // Scale each object so the scene will fill the comfort zone.
                Utility.RecursivelyScale(changeObject, scaleFactor * Vector3.one);
                changeObject.transform.localPosition *= scaleFactor;

                if (newSceneBounds.extents == Vector3.zero)
                    newSceneBounds = changeObject.transform.ComputeBounds(true);
                else
                    newSceneBounds.Encapsulate(changeObject.transform.ComputeBounds(true));
            }

            // Move the scene to the center of the comfort zone.
            Vector3 offset = transform.position - newSceneBounds.center;
            foreach (GameObject changeObject in changeObjects)
                changeObject.transform.Translate(offset, Space.World);
        }

        Debug.Log("Auto-fit complete.");

        return scaleFactor;
    }


    /// <summary>
    /// Moves the given objects behind the zero-parallax plane.  Typically used to clear the way for UI at zero and negative parallax.
    /// </summary>
    public void SweepToDepth(IEnumerable<GameObject> objects, float depth = 0.0f, bool useCameraTranslation = true)
    {
        //Make a bounding box containing the scalable portion of the scene.
        Bounds bounds = new Bounds();
        foreach (GameObject go in objects)
        {
            if (bounds.extents == Vector3.zero)
                bounds = go.transform.ComputeBounds();
            else
                bounds.Encapsulate(go.transform.ComputeBounds());
        }

        float parallax = Vector3.Dot(bounds.center - transform.position, transform.forward) - bounds.extents.magnitude - Utility.Epsilon;
        if (parallax > depth)
            return;

        Vector3 offset = (parallax + depth) * transform.forward;
        if (useCameraTranslation)
        {
            _camera.transform.Translate(offset, Space.World);
        }
        else
        {
            foreach (GameObject go in objects)
                go.transform.Translate(-offset, Space.World);
        }
    }
}
