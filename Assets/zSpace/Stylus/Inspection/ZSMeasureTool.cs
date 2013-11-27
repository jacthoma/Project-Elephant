////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ZSStylusShape))]

/// <summary>
/// Stylus tool for measuring distances.  The user clicks and drags to use the tool.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item> Checks ALT key as modifier to enable facet snapping. </item>
/// <item> Checks SHIFT key as modifier to enable axis snapping. </item>
/// </list>
/// </remarks>
public class ZSMeasureTool : ZSStylusTool
{
    /// <summary> The set of widgets that should poll this tool's measurement. </summary>
    public ZSMeasureWidget[] Widgets = new ZSMeasureWidget[] {};

    /// <summary> Is the tool measuring angles or distances? </summary>
    public bool IsAngular = false;

    /// <summary> The current data measured by the tool.  Other scripts poll this to render any widgets. </summary>
    public ZSMeasureWidget.Measurement Measurement = new ZSMeasureWidget.Measurement();


    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSMeasureTool";
        Measurement.IsAngular = IsAngular;

        foreach (ZSMeasureWidget widget in Widgets)
            widget.Value = Measurement;
    }

    protected override void OnScriptDisable()
    {
        base.OnScriptDisable();

        foreach (ZSMeasureWidget widget in Widgets)
        {
            if (widget != null)
                widget.gameObject.SetActiveRecursively(false);
        }

        Measurement.CornerPoint = Vector3.zero;
        Measurement.StartPoint = Vector3.zero;
        Measurement.EndPoint = Vector3.zero;
    }

    public override void OnStylus()
    {
        base.OnStylus();

        Vector3 HoverPoint = SnappedHoverPoint;

        bool isButtonDown = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonDown(buttonId));
        if (isButtonDown)
        {
            if (_useFacetSnapping && _stylusSelector.HoverObject != null)
                _focusObjects.Add(_stylusSelector.HoverObject);

            if (Measurement.IsAngular)
            {
                if (Measurement.CornerPoint == Vector3.zero)
                {
                    Measurement.CornerPoint = HoverPoint;

                    foreach (ZSMeasureWidget widget in Widgets)
                        widget.gameObject.SetActiveRecursively(true);
                }
                else
                {
                    Measurement.StartPoint = HoverPoint;
                    ToolBegin();
                }
            }
            else
            {
                Measurement.StartPoint = HoverPoint;

                foreach (ZSMeasureWidget widget in Widgets)
                    widget.gameObject.SetActiveRecursively(true);

                ToolBegin();
            }
        }

        bool isButton = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
        if (isButton)
        {
            if (Measurement.StartPoint != Vector3.zero)
                Measurement.EndPoint = HoverPoint;

            ToolStay();
        }

        bool isButtonUp = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonUp(buttonId));
        if (isButtonUp)
        {
            if (Measurement.StartPoint != Vector3.zero)
            {
                ToolEnd();

                _focusObjects.Clear();

                if (IsAngular)
                {
                    if (Vector3.Distance(HoverPoint, Measurement.StartPoint) < _stylusSelector.minDragDistance)
                    {
                        Measurement.CornerPoint = SnappedHoverPoint;

                        foreach (ZSMeasureWidget widget in Widgets)
                            widget.gameObject.SetActiveRecursively(true);

                        if (_useFacetSnapping && _stylusSelector.HoverObject != null)
                            _focusObjects.Add(_stylusSelector.HoverObject);
                    }
                }
                else
                {
                    foreach (ZSMeasureWidget widget in Widgets)
                        widget.gameObject.SetActiveRecursively(false);
                }

                Measurement.StartPoint = Vector3.zero;
                Measurement.EndPoint = Vector3.zero;
            }
        }
    }
}
