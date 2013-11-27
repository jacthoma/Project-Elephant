using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ZSCoreSingleton : MonoBehaviour
{
    #region UNITY CALLBACKS

    void Start()
    {
        if (_isInitialized)
        {
            // Initialize left/right detect.
            if (Screen.fullScreen)
                GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectFullscreen);
            else
                GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectWindowed);
        }
    }

    void OnApplicationQuit()
    {
        if (_isInitialized)
        {
            _isInitialized = false;
            zsup_shutdown();
        }

        _instance = null;
    }

    #endregion


    #region PUBLIC PROPERTIES

    public static ZSCoreSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(ZSCoreSingleton)) as ZSCoreSingleton;

                if (_instance == null)
                {
                    // Create a new ZSCoreSingleton GameObject.
                    GameObject instanceObject = new GameObject("ZSCoreSingleton", typeof(ZSCoreSingleton));

                    // Do not destroy the instance's GameObject on scene change.
                    DontDestroyOnLoad(instanceObject);

                    // Get a reference to the ZSCoreSingleton script component.
                    _instance = instanceObject.GetComponent<ZSCoreSingleton>();

                    if (_instance == null)
                        Debug.Log("A serious error has occurred: Could not create ZSCoreSingleton GameObject.");
                }
            }

            return _instance;
        }
    }

    public bool IsInitialized
    {
        get { return _isInitialized; }
    }

    #endregion


    #region PRIVATE METHODS

    private ZSCoreSingleton()
    {
        // Initialize the zSpace plugin.
        _isInitialized = zsup_initialize();

        // Check to see if the graphics device is initialized.
        // If not, report that stereo will be disabled.
        if (!zsup_isGraphicsDeviceInitialized())
        {
          Debug.Log("Failed to initialize graphics device: stereo is disabled. " +
                    "To enable stereo, please use -force-opengl and -enable-stereo flags.");
        }
    }

    #endregion


    #region PRIVATE MEMBERS

    private static ZSCoreSingleton _instance;
    private bool _isInitialized = false;

    #endregion


    #region ZSPACE PLUGIN IMPORT DECLARATIONS

    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_initialize();
    [DllImport("ZSUnityPlugin")]
    private static extern void zsup_shutdown();
    [DllImport("ZSUnityPlugin")]
    private static extern bool zsup_isGraphicsDeviceInitialized();

    #endregion
}
