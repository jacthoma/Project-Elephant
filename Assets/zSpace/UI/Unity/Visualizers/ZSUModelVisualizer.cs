////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Internal class for representing a Model (part of a Control).
/// </summary>
public class ZSUModelVisualizer : ZSUVisualizer<zSpace.UI.Utility.Model>
{
    /// <summary>
    /// The desired scaling mode. This affects how the model is resized to fit the layout size.
    /// </summary>
    public Scaling ScalingMode = Scaling.Stretch;
    /// <summary>
    /// Enables automatic model recentering before fitting to layout.
    /// </summary>
    public bool RecenterModel = true;
    /// <summary>
    /// Use the default state model when no matching non-default state model can be found.
    /// </summary>
    public bool UseDefaultModelForMissingStates = true;
    /// <summary>
    /// Automatically strip colliders from the child models. If the provided models
    /// do not need colliders (often the case), you should remove the colliders at design-time.
    /// This field is a quick-and-dirty convenience that is performed at run-time.
    /// </summary>
    public bool StripColliders = true;


    public override void Synchronize()
    {
        base.Synchronize();

        Initialize();

        zSpace.UI.Utility.Model modelObject = this.FrameworkControl;


        //
        // Select the model for the given state.
        //
        ModelRecord modelRecord;
        {
            string visualizationState = VisualizationDescriptor.State ?? string.Empty;

            bool modelFound = _modelsByState.TryGetValue(visualizationState, out modelRecord);
            if (!modelFound && this.UseDefaultModelForMissingStates)
            {
                _modelsByState.TryGetValue(string.Empty, out modelRecord);
            }
        }

        if (modelRecord == null)
        {
            if (_model != null)
            {
                _model.SetActiveRecursively(false);
                _model.transform.parent = this.transform;
                _model.layer = this.gameObject.layer;
            }
            _model = null;
        }
        else
        {
            GameObject newModel = modelRecord.Model;
            if (newModel != _model)
            {
                if (_model != null)
                {
                    _model.SetActiveRecursively(false);
                    _model.transform.parent = this.transform;
                    _model.layer = this.gameObject.layer;
                }
                _model = newModel;
                _model.transform.parent = _modelHostNode1.transform;
                _model.SetActiveRecursively(true);
            }
        }


        //
        // Update chosen model.
        //
        if (_model != null)
        {
            Bounds modelBounds = modelRecord.Bounds;
            modelBounds = modelBounds.RotateAboutCenter(modelObject.ModelOrientation);
            if (this.RecenterModel == false)
            {
                var negMax = -modelBounds.max;
                var negMin = -modelBounds.min;
                modelBounds.Encapsulate(negMax);
                modelBounds.Encapsulate(negMin);
            }

            Vector3 modelScaleNeeded;
            switch (this.ScalingMode)
            {
            default:
            case Scaling.None:
                modelScaleNeeded = Vector3.one;
                break;
            case Scaling.Stretch:
                modelScaleNeeded = modelObject.FinalSize.DivideComponents(modelBounds.size);
                break;
            case Scaling.Uniform:
                modelScaleNeeded = modelObject.FinalSize.DivideComponents(modelBounds.size);
                float scaleComponentMin = modelScaleNeeded.Minimum();
                modelScaleNeeded = new Vector3(scaleComponentMin, scaleComponentMin, scaleComponentMin);
                break;
            }
            modelScaleNeeded = modelScaleNeeded.MakeFinite(Vector3.one);

            Vector3 modelTranslationNeeded = -modelRecord.Bounds.center;


            // Recenter.
            _model.transform.localPosition = modelTranslationNeeded;
            _model.transform.localRotation = Quaternion.identity;
            _model.transform.localScale = Vector3.one;

            // Rotate.
            if (RecenterModel == false)
            {
                _modelHostNode1.transform.localPosition = -modelTranslationNeeded;
            }
            else
            {
                _modelHostNode1.transform.localPosition = Vector3.zero;
            }
            _modelHostNode1.transform.localRotation = modelObject.ModelOrientation;
            _modelHostNode1.transform.localScale = Vector3.one;

            // Scale.
            _modelHostNode2.transform.localPosition = Vector3.zero;
            _modelHostNode2.transform.localScale = modelScaleNeeded;
            _modelHostNode2.transform.localRotation = Quaternion.identity;
        }
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        //
        // Gather state-specific models.
        //
        _modelsByState = new Dictionary<string, ModelRecord>();
        if (this.transform.childCount >= 1)
        {
            foreach (Transform child in this.transform)
            {
                GameObject model = child.gameObject;
                if (this.StripColliders)
                {
                    Collider[] colliders = model.GetComponentsInChildren<Collider>(true);
                    foreach (Collider c in colliders)
                        GameObject.DestroyImmediate(c);
                }

                // Does the model have a state name suffix?
                string[] modelNameSplit = model.name.Split(':');
                string stateName = null;
                if (modelNameSplit.Length == 2)
                {
                    stateName = modelNameSplit[1];
                    if (string.IsNullOrEmpty(stateName))
                        stateName = string.Empty;
                }
                else
                {
                    stateName = string.Empty;
                }

                ModelRecord modelRecord = new ModelRecord();
                modelRecord.Model = model;
                modelRecord.Bounds = MeasureModel(model);
                _modelsByState[stateName] = modelRecord;
                
                // Disable model by default.
                model.SetActiveRecursively(false);
            }
        }

        //
        // Create helper transforms.
        //
        _modelHostNode2 = new GameObject("Scale");
        _modelHostNode2.transform.parent = this.transform;
        _modelHostNode2.layer = this.gameObject.layer;
        _modelHostNode2.transform.localPosition = Vector3.zero;
        _modelHostNode2.transform.localRotation = Quaternion.identity;
        _modelHostNode2.transform.localScale = Vector3.one;

        _modelHostNode1 = new GameObject("Rotate");
        _modelHostNode1.transform.parent = _modelHostNode2.transform;
        _modelHostNode1.layer = _modelHostNode2.gameObject.layer;
        _modelHostNode1.transform.localPosition = Vector3.zero;
        _modelHostNode1.transform.localRotation = Quaternion.identity;
        _modelHostNode1.transform.localScale = Vector3.one;

        _isInitialized = true;
    }

    /// <summary>
    /// Measures the given model, inspecting all meshs in the subgraph.
    /// </summary>
    private Bounds MeasureModel(GameObject model)
    {
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);


        Action<GameObject, Matrix4x4> measureSelfAndRecurse = null;
        measureSelfAndRecurse = delegate(GameObject self, Matrix4x4 transformToRootSpace)
        {
            Mesh meshToMeasure = null;
            MeshFilter meshFilter = self.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshToMeasure = meshFilter.sharedMesh;
            }
            if (meshToMeasure != null)
            {
                Bounds meshBounds = meshToMeasure.bounds;
                Bounds meshBoundsRotated = meshBounds.Transform(transformToRootSpace);
                max = max.Maximum(meshBoundsRotated.max);
                min = min.Minimum(meshBoundsRotated.min);
            }

            // Recurse
            foreach (Transform child in self.transform)
            {
                Matrix4x4 childLocalTransform = Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale);
                Matrix4x4 totalTransform = transformToRootSpace * childLocalTransform;

                measureSelfAndRecurse(child.gameObject, totalTransform);
            }
        };

        measureSelfAndRecurse(model, Matrix4x4.identity);
        Vector3 center = max.Average(min);
        return new Bounds(center, max - min);
    }


    private class ModelRecord
    {
        public GameObject Model;
        public Bounds Bounds;
    }


    private bool _isInitialized;
    /// <summary>
    /// The currently visible model.
    /// </summary>
    private GameObject _model;
    /// <summary>
    /// A transform node to assist with scale and recenter.
    /// </summary>
    private GameObject _modelHostNode1;
    /// <summary>
    /// A transform node to assist with scale and recenter.
    /// </summary>
    private GameObject _modelHostNode2;
    /// <summary>
    /// A collection of models, organized by visualization state name.
    /// </summary>
    private Dictionary<string, ModelRecord> _modelsByState;
}

