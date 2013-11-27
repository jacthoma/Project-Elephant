using UnityEditor;
using UnityEngine;
using System.Collections;

public class ZSCoreDiagnosticWindow : EditorWindow
{
  #region UNITY_CALLBACKS
	
  [MenuItem ("Window/zSpace/Core Diagnostic")]
  static void Init()
  {
    EditorWindow.GetWindow(typeof(ZSCoreDiagnosticWindow));
  }

  public void Awake()
  {
    m_isStereoEnabled = EditorPrefs.GetBool("IsStereoEnabled", true);
    m_areEyesSwapped = EditorPrefs.GetBool("AreEyesSwapped", false);

    m_interPupillaryDistance = EditorPrefs.GetFloat("InterPupillaryDistance", 0.06f);
    m_stereoLevel = EditorPrefs.GetFloat("StereoLevel", 1.0f);
    m_worldScale = EditorPrefs.GetFloat("WorldScale", 1.0f);
    m_fieldOfViewScale = EditorPrefs.GetFloat("FieldOfViewScale", 1.0f);
    m_zeroParallaxOffset = EditorPrefs.GetFloat("ZeroParallaxOffset", 0.0f);
    m_nearClip = EditorPrefs.GetFloat("NearClip", 0.1f);
    m_farClip = EditorPrefs.GetFloat("FarClip", 100000.0f);

    m_isHeadTrackingEnabled = EditorPrefs.GetBool("IsHeadTrackingEnabled", true);
    m_headTrackingScale = EditorPrefs.GetFloat("HeadTrackingScale", 1.0f);

    m_isStylusTrackingEnabled = EditorPrefs.GetBool("IsStylusTrackingEnabled", true);
    m_isStylusVisualizationEnabled = EditorPrefs.GetBool("IsStylusVisualizationEnabled", true);

    m_isMouseEmulationEnabled = EditorPrefs.GetBool("IsMouseEmulationEnabled", false);
    m_mouseEmulationDistance = EditorPrefs.GetFloat("MouseEmulationDistance", 1.0f);

    m_isStylusLedEnabled = EditorPrefs.GetBool("IsStylusLedEnabled", false);
    m_stylusLedColor = (ZSCore.StylusLedColor)EditorPrefs.GetInt("StylusLedColor", (int)ZSCore.StylusLedColor.White);

    m_isStylusVibrationEnabled = EditorPrefs.GetBool("IsStylusVibrationEnabled", false);
    m_stylusVibrationOnPeriod = EditorPrefs.GetFloat("StylusVibrationOnPeriod", 0.0f);
    m_stylusVibrationOffPeriod = EditorPrefs.GetFloat("StylusVibrationOffPeriod", 0.0f);
    m_stylusVibrationRepeatCount = EditorPrefs.GetInt("StylusVibrationRepeatCount", 0);
    m_startStylusVibration = EditorPrefs.GetBool("StartStylusVibration", false);
    m_stopStylusVibration = EditorPrefs.GetBool("StopStylusVibration", false); 
  }

  public void OnDestroy()
  {
    EditorPrefs.SetBool("IsStereoEnabled", m_isStereoEnabled);
    EditorPrefs.SetBool("AreEyesSwapped", m_areEyesSwapped);

    EditorPrefs.SetFloat("InterPupillaryDistance", m_interPupillaryDistance);
    EditorPrefs.SetFloat("StereoLevel", m_stereoLevel);
    EditorPrefs.SetFloat("WorldScale", m_worldScale);
    EditorPrefs.SetFloat("FieldOfViewScale", m_fieldOfViewScale);
    EditorPrefs.SetFloat("ZeroParallaxOffset", m_zeroParallaxOffset);
    EditorPrefs.SetFloat("NearClip", m_nearClip);
    EditorPrefs.SetFloat("FarClip", m_farClip);

    EditorPrefs.SetBool("IsHeadTrackingEnabled", m_isHeadTrackingEnabled);
    EditorPrefs.SetFloat("HeadTrackingScale", m_headTrackingScale);

    EditorPrefs.SetBool("IsStylusTrackingEnabled", m_isStylusTrackingEnabled);
    EditorPrefs.SetBool("IsStylusVisualizationEnabled", m_isStylusVisualizationEnabled);

    EditorPrefs.SetBool("IsMouseEmulationEnabled", m_isMouseEmulationEnabled);
    EditorPrefs.SetFloat("MouseEmulationDistance", m_mouseEmulationDistance);

    EditorPrefs.SetBool("IsStylusLedEnabled", m_isStylusLedEnabled);
    EditorPrefs.SetInt("StylusLedColor", (int)m_stylusLedColor);

    EditorPrefs.SetBool("IsStylusVibrationEnabled", m_isStylusVibrationEnabled);
    EditorPrefs.SetFloat("StylusVibrationOnPeriod", m_stylusVibrationOnPeriod);
    EditorPrefs.SetFloat("StylusVibrationOffPeriod", m_stylusVibrationOffPeriod);
    EditorPrefs.SetInt("StylusVibrationRepeatCount", m_stylusVibrationRepeatCount);
    EditorPrefs.SetBool("StartStylusVibration", m_startStylusVibration);
    EditorPrefs.SetBool("StopStylusVibration", m_stopStylusVibration);
		
    if (m_stylusVisualizationObject != null)
    {
      Destroy(m_stylusVisualizationObject);
      m_stylusVisualizationObject = null;
    }
  }

  void OnGUI()
  {
    m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

    if (GUILayout.Button("Restore Defaults", GUILayout.Width(118), GUILayout.Height(20)))
      RestoreDefaults();

    GUILayout.Label("\nStereo Settings:", EditorStyles.boldLabel);

    m_isStereoEnabled = EditorGUILayout.Toggle("Enable Stereo", m_isStereoEnabled);
    m_areEyesSwapped = EditorGUILayout.Toggle("Swap Eyes", m_areEyesSwapped);

    m_interPupillaryDistance = EditorGUILayout.Slider("Inter Pupillary Distance", m_interPupillaryDistance, 0, 1);
    m_stereoLevel = EditorGUILayout.Slider("Stereo Level", m_stereoLevel, 0, 1);
    m_worldScale = EditorGUILayout.Slider("World Scale", m_worldScale, 0.0001f, 1000f);
    m_fieldOfViewScale = EditorGUILayout.Slider("Field of View Scale", m_fieldOfViewScale, 0, 100);
    m_zeroParallaxOffset = EditorGUILayout.Slider("Zero Parallax Offset", m_zeroParallaxOffset, -10, 10);
    m_nearClip = EditorGUILayout.Slider("Near Clip", m_nearClip, 0, 1000);
    m_farClip = EditorGUILayout.Slider("Far Clip", m_farClip, 0, 100000);

    GUILayout.Label("\nHead Tracker Settings:", EditorStyles.boldLabel);

    m_isHeadTrackingEnabled = EditorGUILayout.Toggle("Enable Head Tracking", m_isHeadTrackingEnabled);
    m_headTrackingScale = EditorGUILayout.Slider("Head Tracking Scale", m_headTrackingScale, 0, 1000);

    GUILayout.Label("\nStylus Tracker Settings:", EditorStyles.boldLabel);
    m_isStylusTrackingEnabled = EditorGUILayout.Toggle("Enable Stylus Tracking", m_isStylusTrackingEnabled);
    m_isStylusVisualizationEnabled = EditorGUILayout.Toggle("Enable Stylus Visualization", m_isStylusVisualizationEnabled);

    GUILayout.Label("\nStylus LED Settings:", EditorStyles.boldLabel);
    m_isStylusLedEnabled = EditorGUILayout.Toggle("Enable Stylus LED", m_isStylusLedEnabled);
    m_stylusLedColor = (ZSCore.StylusLedColor)EditorGUILayout.EnumPopup("Stylus Led Color", m_stylusLedColor);

    GUILayout.Label("\nStylus Vibration Settings:", EditorStyles.boldLabel);
    m_isStylusVibrationEnabled = EditorGUILayout.Toggle("Enable Stylus Vibration", m_isStylusVibrationEnabled);
    m_stylusVibrationOnPeriod = EditorGUILayout.Slider("Vibration On Period", m_stylusVibrationOnPeriod, 0, 100);
    m_stylusVibrationOffPeriod = EditorGUILayout.Slider("Vibration Off Period", m_stylusVibrationOffPeriod, 0, 100);
    m_stylusVibrationRepeatCount = EditorGUILayout.IntSlider("Vibration Repeat Count", m_stylusVibrationRepeatCount, 0, 100);
    m_startStylusVibration = EditorGUILayout.Toggle("Start Vibration", m_startStylusVibration);
    m_stopStylusVibration = EditorGUILayout.Toggle("Stop Vibration", m_stopStylusVibration);

    GUILayout.Label("\nMouse Emulation:", EditorStyles.boldLabel);
    m_isMouseEmulationEnabled = EditorGUILayout.Toggle("Enable Mouse Emulation", m_isMouseEmulationEnabled);
    m_mouseEmulationDistance = EditorGUILayout.Slider("Emulation Distance", m_mouseEmulationDistance, 0, 5);

    GUILayout.Label("\nDisplay Information (read only):", EditorStyles.boldLabel);
    m_displayOffset = EditorGUILayout.Vector3Field("Display Offset", m_displayOffset);
    m_displayPosition = EditorGUILayout.Vector2Field("Display Position", m_displayPosition);
    m_displayAngle = EditorGUILayout.Vector2Field("Display Angle", m_displayAngle);
    m_displayResolution = EditorGUILayout.Vector2Field("Display Resolution", m_displayResolution);
    m_displaySize = EditorGUILayout.Vector2Field("Display Size", m_displaySize);

    GUILayout.Label("\nHead Tracker Information (read only):", EditorStyles.boldLabel);
    m_headPosition = EditorGUILayout.Vector3Field("Position (tracker space)", m_headPosition);
    m_headDirection = EditorGUILayout.Vector3Field("Direction (tracker space)", m_headDirection);
    m_headCameraPosition = EditorGUILayout.Vector3Field("Position (camera space)", m_headCameraPosition);
    m_headCameraDirection = EditorGUILayout.Vector3Field("Direction (camera space)", m_headCameraDirection);
    m_headWorldPosition = EditorGUILayout.Vector3Field("Position (world space)", m_headWorldPosition);
    m_headWorldDirection = EditorGUILayout.Vector3Field("Direction (world space)", m_headWorldDirection);
    
    GUILayout.Label("\nStylus Tracker Information (read only):", EditorStyles.boldLabel);
    m_stylusPosition = EditorGUILayout.Vector3Field("Position (tracker space)", m_stylusPosition);
    m_stylusDirection = EditorGUILayout.Vector3Field("Direction (tracker space)", m_stylusDirection);
    m_stylusCameraPosition = EditorGUILayout.Vector3Field("Position (camera space)", m_stylusCameraPosition);
    m_stylusCameraDirection = EditorGUILayout.Vector3Field("Direction (camera space)", m_stylusCameraDirection);
    m_stylusWorldPosition = EditorGUILayout.Vector3Field("Position (world space)", m_stylusWorldPosition);
    m_stylusWorldDirection = EditorGUILayout.Vector3Field("Direction (world space)", m_stylusWorldDirection);

    m_isStylusButton0Pressed = EditorGUILayout.Toggle("Button 0 Pressed", m_isStylusButton0Pressed);
    m_isStylusButton1Pressed = EditorGUILayout.Toggle("Button 1 Pressed", m_isStylusButton1Pressed);
    m_isStylusButton2Pressed = EditorGUILayout.Toggle("Button 2 Pressed", m_isStylusButton2Pressed);

    EditorGUILayout.EndScrollView();
  }
	
  void Update()
  {
    UpdateInternal();
  }

  void OnInspectorUpdate()
  {		
    // Repaint the editor window.
    Repaint();
  }

  #endregion

  #region PRIVATE_HELPERS

  private void UpdateInternal()
  {
    if (!Application.isPlaying)
    {
      m_core = null;
      m_stylusVisualizationObject = null;
      return;
    }
		
    if (m_core == null)
    {
      GameObject coreObject = GameObject.Find("ZSCore");

      if (coreObject != null)
        m_core = coreObject.GetComponent<ZSCore>();
		
      // Set up the stylus visualization.
      m_stylusVisualizationObject = new GameObject("StylusVisualization");
      LineRenderer lineRenderer = m_stylusVisualizationObject.AddComponent<LineRenderer>();
      lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
      lineRenderer.SetColors(Color.white, Color.white);
      lineRenderer.enabled = false;
    }
		
    if (m_core == null)
      return;
		
    // Stereo Settings.
    if (m_isStereoEnabled != m_core.IsStereoEnabled())
      m_core.SetStereoEnabled(m_isStereoEnabled);

    if (m_areEyesSwapped != m_core.AreEyesSwapped())
      m_core.SetEyesSwapped(m_areEyesSwapped);

    if (m_interPupillaryDistance != m_core.GetInterPupillaryDistance())
      m_core.SetInterPupillaryDistance(m_interPupillaryDistance);

    if (m_stereoLevel != m_core.GetStereoLevel())
      m_core.SetStereoLevel(m_stereoLevel);

    if (m_worldScale != m_core.GetWorldScale())
      m_core.SetWorldScale(m_worldScale);

    if (m_fieldOfViewScale != m_core.GetFieldOfViewScale())
      m_core.SetFieldOfViewScale(m_fieldOfViewScale);

    if (m_zeroParallaxOffset != m_core.GetZeroParallaxOffset())
      m_core.SetZeroParallaxOffset(m_zeroParallaxOffset);

    if (m_nearClip != m_core.GetNearClip())
      m_core.SetNearClip(m_nearClip);

    if (m_farClip != m_core.GetFarClip())
      m_core.SetFarClip(m_farClip);

    // Head Tracker Settings.
    if (m_isHeadTrackingEnabled != m_core.IsHeadTrackingEnabled())
      m_core.SetHeadTrackingEnabled(m_isHeadTrackingEnabled);

    if (m_headTrackingScale != m_core.GetHeadTrackingScale())
      m_core.SetHeadTrackingScale(m_headTrackingScale);

    // Stylus Tracker Settings.
    if (m_isStylusTrackingEnabled != m_core.IsStylusTrackingEnabled())
      m_core.SetStylusTrackingEnabled(m_isStylusTrackingEnabled);

    // Stylus LED Settings.
    if (m_isStylusLedEnabled != m_core.IsStylusLedEnabled())
      m_core.SetStylusLedEnabled(m_isStylusLedEnabled);

    if (m_stylusLedColor != m_core.GetStylusLedColor())
      m_core.SetStylusLedColor(m_stylusLedColor);

    // Stylus Vibration Settings.
    if (m_isStylusVibrationEnabled != m_core.IsStylusVibrationEnabled())
      m_core.SetStylusVibrationEnabled(m_isStylusVibrationEnabled);

    if (m_stylusVibrationOnPeriod != m_core.GetStylusVibrationOnPeriod())
      m_core.SetStylusVibrationOnPeriod(m_stylusVibrationOnPeriod);

    if (m_stylusVibrationOffPeriod != m_core.GetStylusVibrationOffPeriod())
      m_core.SetStylusVibrationOffPeriod(m_stylusVibrationOffPeriod);

    if (m_stylusVibrationRepeatCount != m_core.GetStylusVibrationRepeatCount())
      m_core.SetStylusVibrationRepeatCount(m_stylusVibrationRepeatCount);

    if (m_startStylusVibration)
    {
      m_core.StartStylusVibration();
      m_startStylusVibration = false;
    }

    if (m_stopStylusVibration)
    {
      m_core.StopStylusVibration();
      m_stopStylusVibration = false;
    }

    // Mouse Emulation Settings.
    if (m_isMouseEmulationEnabled != m_core.IsMouseEmulationEnabled())
      m_core.SetMouseEmulationEnabled(m_isMouseEmulationEnabled);

    if (m_mouseEmulationDistance != m_core.GetMouseEmulationDistance())
      m_core.SetMouseEmulationDistance(m_mouseEmulationDistance);

    // Read Only Display Information.
    m_displayOffset = m_core.GetDisplayOffset();
    m_displayPosition = m_core.GetDisplayPosition();
    m_displayAngle = m_core.GetDisplayAngle();
    m_displayResolution = m_core.GetDisplayResolution();
    m_displaySize = m_core.GetDisplaySize();
		
	// Read Only Head Tracker Information.
    Matrix4x4 headPose = m_core.GetTrackerTargetPose(ZSCore.TrackerTargetType.Head);
    m_headPosition = new Vector3(headPose[0, 3], headPose[1, 3], headPose[2, 3]);
    m_headDirection = headPose * new Vector3(0, 0, 1.0f);

    Matrix4x4 headCameraPose = m_core.GetTrackerTargetCameraPose(ZSCore.TrackerTargetType.Head);
    m_headCameraPosition = new Vector3(headCameraPose[0, 3], headCameraPose[1, 3], headCameraPose[2, 3]);
    m_headCameraDirection = headCameraPose * new Vector3(0, 0, 1.0f);
    
    Matrix4x4 headWorldPose = m_core.GetTrackerTargetWorldPose(ZSCore.TrackerTargetType.Head);
    m_headWorldPosition = new Vector3(headWorldPose[0, 3], headWorldPose[1, 3], headWorldPose[2, 3]);
    m_headWorldDirection = headWorldPose * new Vector3(0, 0, 1.0f);

    // Read Only Stylus Tracker Information.
    Matrix4x4 stylusPose = m_core.GetTrackerTargetPose(ZSCore.TrackerTargetType.Primary);
    m_stylusPosition = new Vector3(stylusPose[0, 3], stylusPose[1, 3], stylusPose[2, 3]);
    m_stylusDirection = stylusPose * new Vector3(0, 0, 1.0f);

    Matrix4x4 stylusCameraPose = m_core.GetTrackerTargetCameraPose(ZSCore.TrackerTargetType.Primary);
    m_stylusCameraPosition = new Vector3(stylusCameraPose[0, 3], stylusCameraPose[1, 3], stylusCameraPose[2, 3]);
    m_stylusCameraDirection = stylusCameraPose * new Vector3(0, 0, 1.0f);

    Matrix4x4 stylusWorldPose = m_core.GetTrackerTargetWorldPose(ZSCore.TrackerTargetType.Primary);
    m_stylusWorldPosition = new Vector3(stylusWorldPose[0, 3], stylusWorldPose[1, 3], stylusWorldPose[2, 3]);
    m_stylusWorldDirection = stylusWorldPose * new Vector3(0, 0, 1.0f);
		
    bool isAnyStylusButtonPressed = false;
    
    for (int i = 0; i < m_core.GetNumTrackerTargetButtons(ZSCore.TrackerTargetType.Primary); ++i)
    {
      bool isButtonPressed = m_core.IsTrackerTargetButtonPressed(ZSCore.TrackerTargetType.Primary, i);
      
	  if (i == 0)
        m_isStylusButton0Pressed = isButtonPressed;
      else if (i == 1)
        m_isStylusButton1Pressed = isButtonPressed;
      else if (i == 2)
        m_isStylusButton2Pressed = isButtonPressed;
	
      isAnyStylusButtonPressed |= isButtonPressed;
    }
		
    // Draw the stylus visualization
    float stylusBeamWidth = 0.0004f * m_worldScale;
    float stylusBeamLength = 0.1f * m_worldScale;
		
    if (m_stylusVisualizationObject != null)
    {
	  LineRenderer lineRenderer = m_stylusVisualizationObject.GetComponent<LineRenderer>();
      lineRenderer.enabled = m_isStylusVisualizationEnabled;

      if (lineRenderer.enabled)
      {
        lineRenderer.SetWidth(stylusBeamWidth, stylusBeamWidth);
        lineRenderer.SetPosition(0, m_stylusWorldPosition);
        lineRenderer.SetPosition(1, (m_stylusWorldPosition + (stylusBeamLength * m_stylusWorldDirection)));

        if (!isAnyStylusButtonPressed)
        {
          lineRenderer.SetColors(Color.white, Color.white);
        }
        else
        {
          if (!m_wasAnyButtonPressed)
          {
            if (m_isStylusButton0Pressed)
              lineRenderer.SetColors(Color.red, Color.red);
            else if (m_isStylusButton1Pressed)
              lineRenderer.SetColors(Color.green, Color.green);
            else if (m_isStylusButton2Pressed)
              lineRenderer.SetColors(Color.blue, Color.blue);
          }
        }
      }
    }

    m_wasAnyButtonPressed = isAnyStylusButtonPressed;		
  }

  private void RestoreDefaults()
  {
    // Stereo Members
    m_isStereoEnabled = true;
    m_areEyesSwapped = false;

    m_interPupillaryDistance = 0.06f;
    m_stereoLevel = 1.0f;
    m_worldScale = 1.0f;
    m_fieldOfViewScale = 1.0f;
    m_zeroParallaxOffset = 0.0f;
    m_nearClip = 0.1f;
    m_farClip = 100000.0f;

    // Tracker Members
    m_isHeadTrackingEnabled = true;
    m_headTrackingScale = 1.0f;

    m_isStylusTrackingEnabled = true;
    m_isStylusVisualizationEnabled = true;

    m_isMouseEmulationEnabled = false;
    m_mouseEmulationDistance = 1.0f;
    
    m_isStylusLedEnabled = false;
    m_stylusLedColor = ZSCore.StylusLedColor.White;

    m_isStylusVibrationEnabled = false;
    m_stylusVibrationOnPeriod = 0.0f;
    m_stylusVibrationOffPeriod = 0.0f;
    m_stylusVibrationRepeatCount = 0;
    m_startStylusVibration = false;
    m_stopStylusVibration = false;
  }

  #endregion

  #region PRIVATE_MEMBERS

  private ZSCore m_core = null;
  private GameObject m_stylusVisualizationObject = null;
  
  // Stereo Members
  private bool m_isStereoEnabled = true;
  private bool m_areEyesSwapped = false;

  private float m_interPupillaryDistance = 0.06f;
  private float m_stereoLevel = 1.0f;
  private float m_worldScale = 1.0f;
  private float m_fieldOfViewScale = 1.0f;
  private float m_zeroParallaxOffset = 0.0f;
  private float m_nearClip = 0.1f;
  private float m_farClip = 100000.0f;

  // Tracker Members
  private bool m_isHeadTrackingEnabled = true;
  private float m_headTrackingScale = 1.0f;

  private bool m_isStylusTrackingEnabled = true;
  private bool m_isStylusVisualizationEnabled = true;

  private bool m_isMouseEmulationEnabled = false;
  private float m_mouseEmulationDistance = 1.0f;
  
  private bool m_isStylusLedEnabled = false;
  private ZSCore.StylusLedColor m_stylusLedColor = ZSCore.StylusLedColor.White;

  private bool m_isStylusVibrationEnabled = false;
  private float m_stylusVibrationOnPeriod = 0.0f;
  private float m_stylusVibrationOffPeriod = 0.0f;
  private int m_stylusVibrationRepeatCount = 0;
  private bool m_startStylusVibration = false;
  private bool m_stopStylusVibration = false;

  // Read Only Members
  private Vector3 m_displayOffset = Vector3.zero;
  private Vector2 m_displayPosition = Vector2.zero;
  private Vector2 m_displayAngle = Vector2.zero;
  private Vector2 m_displayResolution = Vector2.zero;
  private Vector2 m_displaySize = Vector2.zero;

  private Vector3 m_headPosition = Vector3.zero;
  private Vector3 m_headDirection = Vector3.zero;
  private Vector3 m_headCameraPosition = Vector3.zero;
  private Vector3 m_headCameraDirection = Vector3.zero;
  private Vector3 m_headWorldPosition = Vector3.zero;
  private Vector3 m_headWorldDirection = Vector3.zero;

  private Vector3 m_stylusPosition = Vector3.zero;
  private Vector3 m_stylusDirection = Vector3.zero;
  private Vector3 m_stylusCameraPosition = Vector3.zero;
  private Vector3 m_stylusCameraDirection = Vector3.zero;
  private Vector3 m_stylusWorldPosition = Vector3.zero;
  private Vector3 m_stylusWorldDirection = Vector3.zero;
	
  private bool m_wasAnyButtonPressed = false;	
	
  private bool m_isStylusButton0Pressed = false;
  private bool m_isStylusButton1Pressed = false;
  private bool m_isStylusButton2Pressed = false;	
	
  private Vector2 m_scrollPosition = Vector2.zero;

  #endregion
}
