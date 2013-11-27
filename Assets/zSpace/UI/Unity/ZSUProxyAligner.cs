////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary> Places a UI framework control's proxy on a target object or the screen based on camera and display settings. </summary>
/// <remarks> This does not layout the internals of the control. </remarks>
public class ZSUProxyAligner : ZSUMonoBehavior
{
    /// <summary> If true, position the element based on the center of its bounds instead of its position. </summary>
    public bool UseBoundsCenter = true;

    /// <summary> Controls how the control will be scaled to fill the viewport. </summary>
    public Scaling ScalingMode = Scaling.Stretch;

    /// <summary>The object whose bounds will be used for auto-sizing this object's framework control.</summary>
    public GameObject Target;
    protected ZSCore _core;
    protected DisplayBounds _displayBounds;
    protected LayoutAttributes _layoutAttributes;
    protected Vector3 _widgetOffset = Vector3.zero;
    protected Vector3 _initialSize;
    protected FrameworkControl _control;
    protected Vector3 _initialScale;

    override protected void OnScriptAwake()
    {
        base.OnScriptAwake();

        _core = GameObject.Find("ZSCore").GetComponent<ZSCore>();

        if (Target == null)
            _displayBounds = GameObject.Find("DisplayPlane").GetComponent<DisplayBounds>();
    }

    override protected void OnScriptStart()
    {
        base.OnScriptStart();

        _control = gameObject.GetComponent<ZSUFrameworkControlProxy>().FrameworkControl;
        _initialSize = _control.LayoutAttributes.Size;
        _initialScale = transform.lossyScale;
    }

    override protected void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        // Start at the top right far corner and apply alignment.
        float worldScale = (Target != null) ? 1f : _core.GetWorldScale();
        Vector3 widgetExtents
            = _initialSize
            .MultiplyComponents(_initialScale)
            * worldScale
            * 0.5f;

        Vector3 targetExtents = ComputeTargetSize() * 0.5f;

        Vector3 offset = targetExtents - worldScale * widgetExtents;

        LayoutAttributes layoutAttributes = _control.LayoutAttributes;
        Vector3 fullExtents
            = widgetExtents
            + layoutAttributes.MarginsNegative * worldScale
            + layoutAttributes.MarginsPositive * worldScale;

        Vector3 relativeSize = targetExtents.DivideComponents(fullExtents);
        if (ScalingMode == Scaling.Uniform)
            relativeSize = relativeSize.Minimum() * Vector3.one;

        Vector3 localScale = worldScale * _initialScale;
        if (transform.parent != null)
            localScale = localScale.DivideComponents(transform.parent.lossyScale);

        if (!relativeSize.IsGreaterThan(Vector3.one))
        {
            float shrinkage = relativeSize.Minimum();
            localScale *= shrinkage;
            relativeSize /= shrinkage;
        }

        Vector3 centeredOffset = layoutAttributes.MarginsNegative - layoutAttributes.MarginsPositive;

        //FIXME: If aligning to the display, positive should align to positive parallax, etc.
        for (int i = 0; i < 3; ++i)
        {
            switch (layoutAttributes.Alignment[i])
            {
            case Alignment.Negative:
                offset[i] = -offset[i];
                offset[i] += layoutAttributes.MarginsNegative[i];
                break;
            case Alignment.Center:
                offset[i] = centeredOffset[i];
                break;
            case Alignment.Positive:
                offset[i] -= layoutAttributes.MarginsPositive[i];
                break;
            default:
            case Alignment.Stretch:
                if (transform.localScale != localScale)
                    transform.localScale = localScale;

                offset[i] = centeredOffset[i];
                Vector3 size = _control.LayoutAttributes.Size;
                size[i] = relativeSize[i] * _initialSize[i];
                _control.LayoutAttributes.Size = size;
                break;
            }
        }

        offset += worldScale * _widgetOffset;

        transform.rotation = ComputeTargetOrientation();
        transform.position = ComputeTargetPosition() + transform.rotation * offset;
    }

    //TODO: Cache bounds if these are called many times per frame.

    Vector3 ComputeTargetPosition()
    {
        if (Target == null)
            return _displayBounds.transform.position;

        return Target.transform.ComputeBounds(true).center;
    }

    Quaternion ComputeTargetOrientation()
    {
        if (Target != null)
            // Target's bounding box is in world space so don't rotate.
            return Quaternion.identity;

        return _displayBounds.transform.rotation;
    }

    Vector3 ComputeTargetSize()
    {
        if (Target != null)
            return Target.transform.ComputeBounds(true).size;

        return _displayBounds.windowSize;
    }
}
