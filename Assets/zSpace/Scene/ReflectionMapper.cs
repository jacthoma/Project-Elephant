////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

/// <summary>
/// Attach this script to an object that uses a Reflective shader.
/// </summary>
/// <remarks>
/// Use only with reflective or otherwise cube-mapped shaders.
/// The dynamic cube map replaces any cubemap already used by the object.
/// </remarks>
// Adapted from https://docs.unity3d.com/Documentation/ScriptReference/Camera.RenderToCubemap.html?from=RenderTexture
[ExecuteInEditMode]
public class ReflectionMapper : MonoBehaviour
{
    /// <summary>
    /// Resolution of the cube map to be rendered.
    /// </summary>
    public int CubemapSize = 256;

    /// <summary>
    /// If true, updating will be spread out over a 6-frame interval, one face being rendered per frame.
    /// </summary>
    public bool OneFacePerFrame = false;

    void Start()
    {
        // render all six faces at startup
        UpdateCubemap(63);
    }

    void LateUpdate()
    {
        if (OneFacePerFrame)
        {
            var faceToRender = Time.frameCount % 6;
            var faceMask = 1 << faceToRender;
            UpdateCubemap(faceMask);
        }
        else
        {
            UpdateCubemap(63); // all six faces
        }
    }

    void UpdateCubemap(int faceMask)
    {
        if (!cam)
        {
            var go = new GameObject("CubemapCamera", typeof(Camera));
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.identity;
            cam = go.camera;
            cam.fieldOfView = 90f;
            cam.aspect = 1f;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 10;
            cam.enabled = false;
        }

        if (!rtex)
        {
            rtex = new RenderTexture(CubemapSize, CubemapSize, 16);
            rtex.isCubemap = true;
            rtex.hideFlags = HideFlags.HideAndDontSave;
            renderer.sharedMaterial.SetTexture("_Cube", rtex);
        }

        cam.transform.position = transform.position;
        cam.RenderToCubemap(rtex, faceMask);
    }

    void OnDisable()
    {
        DestroyImmediate(cam);
        DestroyImmediate(rtex);
    }

    private Camera cam;
    private RenderTexture rtex;
}
