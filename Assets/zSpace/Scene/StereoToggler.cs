////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.UI;

/// <summary>
/// Smoothly turns stereo on or off depending on whether the head pose is valid and currently moving.
/// </summary>
public class StereoToggler : ZSUMonoBehavior
{
    protected class TargetInfo
    {
        public Matrix4x4 Pose = Matrix4x4.identity;
        public float TimeSinceChange = 0f;

        public TargetInfo(ZSCore core, ZSCore.TrackerTargetType type)
        {
            _core = core;
            _type = type;
        }

        public void Update()
        {
            var pose = _core.GetTrackerTargetPose(_type);
            if (pose != Pose)
            {
                TimeSinceChange = 0f;
                Pose = pose;
            }
            else
            {
                TimeSinceChange += Time.deltaTime;
            }
        }

        private ZSCore _core;
        private ZSCore.TrackerTargetType _type;
    }

    /// <summary>
    /// The rate (1/s) at which the stereo level will change.
    /// </summary>
    /// <remarks>
    /// If head tracking is lost, a value of 1 will cause stereo to be turned off in 1s. A value of 10 will cause it to be turned of in 0.1s.
    /// As soon as tracking is regained, the stereo level will be restored again at the same rate.
    /// </remarks>
    public float ToggleSpeed = 1f;

    /// <summary>
    /// The time (s) the system will wait after tracking is lost but before stereo starts to turn off.
    /// </summary>
    public float ToggleDelay = 0f;

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        _core = GameObject.FindObjectOfType(typeof(ZSCore)) as ZSCore;

        for (int i = 0; i < _targetInfos.Length; ++i)
            _targetInfos[i] = new TargetInfo(_core, (ZSCore.TrackerTargetType)i);
    }

    protected override void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        if (_core == null)
            return;
        
        float increment = Time.deltaTime * ToggleSpeed;

        for (int i = 0; i < _targetInfos.Length; ++i)
            _targetInfos[i].Update();

        if (_targetInfos[(int)ZSCore.TrackerTargetType.Head].TimeSinceChange == 0f)
        {
            _core.SetStereoLevel(Mathf.Min(1f, _core.GetStereoLevel() + increment));
            _core.SetHeadTrackingScale(Mathf.Min(1f, _core.GetHeadTrackingScale() + increment));
            //Debug.Log("Increasing stereo level.");
        }
        else
        {
            bool decreaseStereo = true;
            for (int i = 0; i < _targetInfos.Length; ++i)
                decreaseStereo &= _targetInfos[i].TimeSinceChange > ToggleDelay;

            if (decreaseStereo)
            {
                _core.SetStereoLevel(Mathf.Max(0f, _core.GetStereoLevel() - increment));
                _core.SetHeadTrackingScale(Mathf.Max(0f, _core.GetHeadTrackingScale() - increment));
                //Debug.Log("Reducing stereo level.");
            }
        }
    }

    protected ZSCore _core;
    protected TargetInfo[] _targetInfos = new TargetInfo[(int)ZSCore.TrackerTargetType.NumTypes];
    protected float _timeSinceLastHeadPose = 0f;
}
