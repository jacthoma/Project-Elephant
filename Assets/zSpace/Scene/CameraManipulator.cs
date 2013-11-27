////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Moves the stereo camera rig around the scene based on user input.
/// Implements orbit, walkthrough, and auto-fitting mechanisms.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item> Checks N key to toggle between orbit and first-person shooter camera modes. </item>
/// </list>
/// </remarks>
public class CameraManipulator : MonoBehaviour
{
  /// <summary> The available types of camera motion. </summary>
  public enum Mode
  {
    Orbit,
    FPS,
  }

  /// <summary> The speed of camera rotation in degrees/s. </summary>
  public Vector3 _rotationSpeed = 180.0f * Vector3.one;

  /// <summary> The speed of camera translation in m/s. </summary>
  public Vector3 _translationSpeed = 0.1f * Vector3.one;

  /// <summary> The current mode of the camera. </summary>
  public Mode _mode = Mode.Orbit;

  /// <summary> The ID of the stylus button that will drag the camera. </summary>
  public int[] _stylusButtons = new int[] {1};

  /// <summary> The beginning position of the camera. </summary>
  public Vector3 InitialPosition { get; protected set; }

  /// <summary> The beginning rotation of the camera. </summary>
  public Quaternion InitialRotation { get; protected set; }

  ZSStylusSelector _stylusSelector;
  Vector3 _startStylusHoverPoint;
  Quaternion _startStylusRotation;
  DisplayBounds _displayBounds;
  Vector3 StereoCameraPosition;
  float _dollyFactor;

  void Awake()
  {
    _displayBounds = GameObject.Find("DisplayPlane").GetComponent<DisplayBounds>();
    _stylusSelector = GameObject.Find("ZSStylusSelector").GetComponent<ZSStylusSelector>();
  }


  void Start()
  {
    Rebase();
  }


  /// <summary> Takes the current position and rotation to be the reset state. </summary>
  public void Rebase()
  {
    InitialPosition = transform.position;
    InitialRotation = transform.rotation;

    _dollyFactor = (_displayBounds.collider.bounds.center - transform.position).magnitude;
  }


  void LateUpdate()
  {
    Vector3 orbitCenter = transform.position + _dollyFactor * transform.forward;

    bool isButtonDown = _stylusButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButtonDown(buttonId));
    if (isButtonDown)
    {
      _startStylusHoverPoint = _stylusSelector.activeStylus.hotSpot;
      _startStylusRotation = _stylusSelector.transform.rotation;
    }

    bool isButton = _stylusButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
    if (isButton)
    {
      // Compute rotation and translation.

      Vector3 stylusHoverPoint = _stylusSelector.activeStylus.hotSpot;
      Quaternion stylusRotation = _stylusSelector.transform.rotation;

      Vector3 translation = Vector3.zero;
      Quaternion rotation = Quaternion.identity;
      if (_mode == Mode.FPS)
      {
        rotation = _startStylusRotation * Quaternion.Inverse(stylusRotation);
        translation = _startStylusHoverPoint - stylusHoverPoint;
      }
      else if (_mode == Mode.Orbit)
      {
        Vector3 oldDirection = (orbitCenter - _startStylusHoverPoint).normalized;
        Vector3 newDirection = (orbitCenter - stylusHoverPoint).normalized;
        rotation = Quaternion.FromToRotation(newDirection, oldDirection);

        translation = rotation * (transform.position - orbitCenter) - transform.position;
      }

      transform.Translate(translation, Space.World);
      transform.rotation = rotation * transform.rotation;
    }
    
    {
      Vector3 rotation = new Vector3(Input.GetAxis("RotateX"), Input.GetAxis("RotateY"), Input.GetAxis("RotateZ"));
      Vector3 translation = new Vector3(Input.GetAxis("TranslateX"), Input.GetAxis("TranslateY"), Input.GetAxis("TranslateZ"));
  
      if (_mode == Mode.Orbit)
      {
        if (translation != Vector3.zero)
        {
          // Truck (XY Translate)
          Vector3 offset = Time.deltaTime * Vector3.Scale(translation, _translationSpeed);
          transform.Translate(offset);
        }
  
        if (rotation != Vector3.zero)
        {
          // Pan (Rotate)
          Vector3 angle = Time.deltaTime * Vector2.Scale(rotation, _rotationSpeed);
          transform.RotateAround(orbitCenter, Vector3.up, angle.y);
          transform.RotateAround(orbitCenter, transform.right, -angle.x);
        }
      }
      else if (_mode == Mode.FPS)
      {
        if (translation != Vector3.zero)
        {
          Vector3 offset = Time.deltaTime * Vector3.Scale(translation, _translationSpeed);
  
          Vector3 xz = transform.rotation * new Vector3(offset.x, 0.0f, offset.z);
          transform.Translate(xz, Space.World);
  
          Vector3 y = new Vector3(0.0f, offset.y, 0.0f);
          transform.Translate(y, Space.World);
        }
  
        if (rotation != Vector3.zero)
        {
          Vector3 angle = Vector2.Scale(Time.deltaTime * _rotationSpeed, rotation);
          angle[0] = -angle.x;
          angle[2] = 0.0f;
          Vector3 eulerAngles = transform.rotation.eulerAngles + angle;
          eulerAngles[0] = Utility.ClampAngle(eulerAngles[0], -85.0f, 85.0f);
          transform.rotation = Quaternion.Euler(eulerAngles);
        }
      }
    }
  }
}
