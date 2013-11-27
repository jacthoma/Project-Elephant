////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using zSpace.Common;

/// <summary>
/// Loads specified scenes when one of the specified objects is dragged onto it.
/// </summary>
/// <remarks>
/// Uses a table of GameObject-to-scene mappings to transition between scenes.
/// Uses the collider on this GameObject's Collider to track overlap with objects in the scene.
/// If the Collider is not already a trigger, it will be made into one when the script starts.
/// When an object from the table overlaps, the corresponding scene is loaded.
/// </remarks>
[RequireComponent(typeof(Collider))]
public class SceneTransitionPortal : MonoBehaviour
{
    /// <summary>
    /// Maps an "icon" GameObject to the name of the scene that will be loaded when it is thrown into the portal.
    /// </summary>
    [System.Serializable]
    public class Mapping
    {
        public GameObject Icon;
        public string SceneName;
    }
 
    /// <summary>
    /// Set of icons and the scenes that will be loaded when they are thrown into the portal.
    /// </summary>
    public Mapping[] _mappings = new Mapping[0];

    /// <summary>
    /// The (optional) scene that will be loaded when the portal itself is clicked.
    /// </summary>
    public string _clickScene;
    
    /// <summary>
    /// If true, the dropped objects will spin as they enter the portal.
    /// </summary>
    public bool _doAnimateRotation = false;

    /// <summary>
    /// The time it takes for an icon to fall into the portal and subsequently start loading the next scene.
    /// </summary>
    float _transitionTime = 0.5f;
 
    /// <summary>
    /// When a GameObject's Collider intersects with the portal, this function will determine which object should be taken as the "icon".
    /// For example, if the collider is on the child of the icon, this should return the collider's parent.
    /// </summary>
    public Utility.ObjectResolver _objectResolver;
 
    /// <summary>
    /// An (optional) object that will activate whenever an actual icon is touching the portal.
    /// This shows users which objects can be thrown in and which cannot.
    /// </summary>
    public GameObject _toolTip;
    protected ZSUIStylusInput _stylusInput;
    protected GameObject _collidingObject;
    protected Matrix4x4[] _startPoses;
    protected bool _isTransitioning = false;
  
    /// <summary>
    /// Is the portal visible and responding to input?
    /// </summary>
    public bool IsActive
    {
        get { return renderer.enabled; }
        set
        {            
            renderer.enabled = value;
            collider.enabled = value;
         
            foreach (Mapping mapping in _mappings)
                mapping.Icon.SetActiveRecursively(value);
        }
    }
 
    void Awake()
    {
        GameObject stylusObject = GameObject.Find("ZSStylusSelector");
        if (stylusObject != null)
            _stylusInput = stylusObject.GetComponent<ZSStylusSelector>();
    }

    void Start()
    {
        if (!collider.isTrigger)
        {
            Debug.LogWarning("SceneTransitionPortal's Collider is not a trigger.  Making it into one.");
            collider.isTrigger = true;
        }
    }
 
    void Update()
    {
        ZSStylusSelector stylusSelector = _stylusInput as ZSStylusSelector;
        if (_objectResolver == null && stylusSelector != null)
            _objectResolver = stylusSelector.objectResolver;

        bool isDragTool = stylusSelector == null || stylusSelector.activeStylus.Tool.GetType().IsSubclassOf(typeof(ZSDragTool));

        if (!_isTransitioning && isDragTool && _stylusInput.GetButtonUp(_stylusInput.SelectButton))
        {
            if (!String.IsNullOrEmpty(_clickScene) && _stylusInput.HoverObject == gameObject)
                StartCoroutine(ActivateSceneCoroutine(gameObject));
         
            if (_collidingObject != null)
                StartTransition(_collidingObject);
        }
     
        if (_toolTip != null && _toolTip.active != (_collidingObject != null))
            _toolTip.SetActiveRecursively(_collidingObject != null);
    }
 
    void StartTransition(GameObject icon)
    {
        _isTransitioning = true;

        Vector3 newPosition = (icon.transform.parent == null) ?
          transform.position :
          icon.transform.parent.InverseTransformPoint(transform.position);
          
        icon.transform.AnimateTo("localPosition", new Vector3[] {icon.transform.localPosition, newPosition}, _transitionTime);
        icon.transform.AnimateTo("localScale", new Vector3[] {icon.transform.localScale, 0.001f * Vector3.one}, _transitionTime);
        if (_doAnimateRotation)
            icon.transform.AnimateTo("localRotation", new Quaternion[] {icon.transform.localRotation, Quaternion.identity}, _transitionTime);
     
        StartCoroutine(ActivateSceneCoroutine(icon, _transitionTime));
    }
         
    IEnumerator ActivateSceneCoroutine(GameObject icon, float delay = 0f)
    {
        if (delay != 0f)
            yield return new WaitForSeconds(delay);
     
        if (_toolTip != null)
            _toolTip.SetActiveRecursively(false);
 
        if (icon == gameObject)
        {
            if (!String.IsNullOrEmpty(_clickScene))
            {
                AsyncOperation async = Application.LoadLevelAsync(_clickScene);
                while (!async.isDone)
                {
                    Debug.Log("Loading " + _clickScene + ": " + 100f * async.progress + "%");
                    yield return null;
                }
            }
        }
        else
        {            
            foreach (Mapping mapping in _mappings)
            {
                if (mapping.Icon == icon)
                {
                    AsyncOperation async = Application.LoadLevelAsync(mapping.SceneName);
                    while (!async.isDone)
                    {
                        Debug.Log("Loading " + mapping.SceneName + ": " + 100f * async.progress + "%");
                        yield return null;
                    }
                }
            }
        }
     
        _collidingObject = null;
        _isTransitioning = false;
    }
 
    void OnTriggerEnter(Collider collider)
    {
        if (_isTransitioning || _collidingObject != null)
            return;
     
        GameObject go = _objectResolver(collider.gameObject);
     
        foreach (Mapping mapping in _mappings)
        {
            if (go == mapping.Icon)
            {
                _collidingObject = go;
                if (_toolTip != null)
                    _toolTip.SetActiveRecursively(true);
                break;
            }
        }
    }
 
    void OnTriggerExit(Collider collider)
    {
        if (_isTransitioning || _collidingObject == null)
            return;
     
        if (_objectResolver(collider.gameObject) == _collidingObject)
        {
            _collidingObject = null;
            if (_toolTip != null)
                _toolTip.SetActiveRecursively(false);
        }
    }
}
