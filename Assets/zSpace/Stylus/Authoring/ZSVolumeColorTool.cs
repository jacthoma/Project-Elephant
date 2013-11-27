////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using zSpace.Common;

/// <summary> Tool for adjusting a VolumeViz's ValueMap of density to color values. </summary>
public class ZSVolumeColorTool : ZSStylusTool
{
    /// <summary>
    /// Defines the range of values to isolate when coloring voxels.
    /// </summary>
    /// <remarks>
    /// When a voxel with density D is selected, then only voxels between D - DensitySpread*D
    /// and D + DensitySpread*D will be shown.
    /// Values at the lower round will be transparent and red.
    /// Values at the upper bound will be solid white.
    /// </remarks>
    public float DensitySpread = 0.1f;

    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSVolumeColorTool";
    }


    public override void OnStylus()
    {
        base.OnStylus();

        if (_stylusSelector.HoverObject != null && _stylusSelector.HoverObject.layer == _stylusSelector.uiLayer)
            return;

        // Find the density of the voxel under the HoverPoint.

        bool isButton = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
        if (isButton)
        {
            //TODO: Resolve via singleton?
            var viz = (_stylusSelector.HoverObject != null) ? _stylusSelector.HoverObject.GetComponentInChildren<ZSUVolumeViz>() : null;
            if (viz == null)
            {
                foreach (var modifiedViz in _modifiedVizes)
                    modifiedViz.ValueMap = ZSUVolumeViz.ValueMapping.Identity;

                _modifiedVizes.Clear();
            }
            else
            {
                // Due to an API limitation in Unity, we can't directly sample the Texture3D, so stash its (low detail) pixels here.
                if (!_modifiedVizes.Contains(viz))
                    _modifiedVizes.Add(viz);

                viz.ValueMap = ZSUVolumeViz.ValueMapping.Linear;

                Color density = viz.GetDensity(_stylusSelector.activeStylus.hotSpot);
                viz.ValueMapPoint0 = (1f - DensitySpread) * density;
                viz.ValueMapPoint1 = (1f + DensitySpread) * density;
                viz.ValueMapPoint2 = new Color(1, 0, 0, 0);
                viz.ValueMapPoint3 = new Color(.5f, .5f, .25f, .5f);
            }
        }
    }

    protected HashSet<ZSUVolumeViz> _modifiedVizes = new HashSet<ZSUVolumeViz>();
}
