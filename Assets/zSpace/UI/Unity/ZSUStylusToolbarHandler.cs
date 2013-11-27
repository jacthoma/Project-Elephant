////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using zSpace.UI;

/// <summary>
/// Handles messages from the Toolbar for changing the active stylus.
/// </summary>
public class ZSUStylusToolbarHandler : MonoBehaviour
{
    /// <summary>
    /// Associates a stylus shape with buttons that will be shown when it is active or inactive.
    /// </summary>
    [System.Serializable]
    public class Record
    {
        public ZSStylusShape Shape;
        public ZSUFrameworkControlProxy ButtonOn;
        public ZSUFrameworkControlProxy ButtonOff;
    }

    protected enum Mode
    {
        Move,
        Scale,
        Sketch,
    }

    public Record MovePointer;
    public Record ScalePointer;
    public Record SketchPointer;

    protected Record[] _mappings;

    protected ZSStylusSelector _stylusSelector;

    void Awake() { _stylusSelector = GameObject.Find("ZSStylusSelector").GetComponent<ZSStylusSelector>(); }

    void Start() { _mappings = new Record[] { MovePointer, ScalePointer, SketchPointer }; }

    void Update()
    {
        for (int i = 0; i < _mappings.Length; ++i)
        {
            if (_mappings[i].ButtonOn != null && _mappings[i].ButtonOff != null &&
                _stylusSelector.activeStylus != _mappings[i].Shape &&
                _mappings[i].ButtonOn.FrameworkControl.Visible)
            {
                _mappings[i].ButtonOn.FrameworkControl.Visible = false;
                _mappings[i].ButtonOff.FrameworkControl.Visible = true;
            }
        }
    }

    public void OnMoveButton(FrameworkMessage message) { ActivateStylus((int)Mode.Move); }
    public void OnScaleButton(FrameworkMessage message) { ActivateStylus((int)Mode.Scale); }
    public void OnSketchButton(FrameworkMessage message) { ActivateStylus((int)Mode.Sketch); }

    protected void ActivateStylus(int index)
    {
        for (int i = 0; i < _mappings.Length; ++i)
        {
            bool isActive = (i == index);

            if (isActive)
                _stylusSelector.activeStylus = _mappings[i].Shape;

            if (_mappings[i].ButtonOn != null && _mappings[i].ButtonOff != null)
            {
                _mappings[i].ButtonOn.FrameworkControl.Visible = isActive;
                _mappings[i].ButtonOff.FrameworkControl.Visible = !isActive;
            }
        }
    }
}
