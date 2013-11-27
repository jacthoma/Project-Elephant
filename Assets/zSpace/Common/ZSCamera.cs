////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// Maintains a GameObject and Camera with the correct stereo projection of a specified eye (left, right, or center).
/// </summary>
/// <remarks>
/// Also causes stereo rendering of the left and right eye cameras. Keeps the overall stereo rig at the same transform as the current camera.
/// </remarks>
public class ZSCamera : MonoBehaviour
{
    /// <summary>
    /// The stereo eye corresponding to this object's camera.
    /// </summary>
    public ZSCore.Eye Eye = ZSCore.Eye.Center;

    void Start()
    {
        _core = GameObject.FindObjectOfType(typeof(ZSCore)) as ZSCore;
        _stereoRig = GameObject.Find("ZSStereoRig");
    }

    void OnPreCull()
    {
        if (_core != null)
        {
            // Specify the correct eye.
            ZSCore.Eye eye = Eye;

            if (eye != ZSCore.Eye.Center && _core.AreEyesSwapped())
                eye = (eye == ZSCore.Eye.Left) ? ZSCore.Eye.Right : ZSCore.Eye.Left;

            if (Eye == ZSCore.Eye.Left && _core.CurrentCamera != null && _stereoRig != null)
            {
                // Grab the current monoscopic camera's transform and apply it
                // to the ZSStereoRig.
                _stereoRig.transform.position = _core.CurrentCamera.transform.position;
                _stereoRig.transform.rotation = _core.CurrentCamera.transform.rotation;
                _stereoRig.transform.localScale = _core.CurrentCamera.transform.localScale;
            }

            // Calculate left camera's transform.
            Matrix4x4 viewMatrixInverse = ZSCore.ConvertFromRightToLeft(_core.GetViewMatrix(eye).inverse);
            transform.localPosition = viewMatrixInverse.GetColumn(3);
            transform.localRotation = Quaternion.LookRotation(viewMatrixInverse.GetColumn(2), viewMatrixInverse.GetColumn(1));

            // Set the left camera's projection matrix.
            gameObject.camera.projectionMatrix = _core.GetProjectionMatrix(eye);
        }

        if (Eye != ZSCore.Eye.Center)
        {
            ZSCore.GlPluginEventType eventType = (Eye == ZSCore.Eye.Left) ? ZSCore.GlPluginEventType.SelectLeftEye : ZSCore.GlPluginEventType.SelectRightEye;
            GL.IssuePluginEvent((int)eventType);
        }
    }

    private ZSCore _core = null;
    private GameObject _stereoRig = null;
}
