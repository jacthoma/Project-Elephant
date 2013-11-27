////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Toggles the current GameObject when one of the specified keys is pressed.
/// </summary>
public class ObjectToggler : MonoBehaviour
{
    /// <summary>
    /// Maps a keycode to a GameObject. The object's active/inactive state will be toggled whenever DownKey goes down and whenever UpKey is released.
    /// </summary>
    [System.Serializable]
    public class Mapping
    {
        /// <summary>
        /// When this key goes down, the target object's active state will be toggled.
        /// </summary>
        public KeyCode DownKey;

        /// <summary>
        /// When this key goes up, the target object's active state will be toggled.
        /// </summary>
        public KeyCode UpKey;

        /// <summary>
        /// The GameObject whose active state will be controlled by key up/down events.
        /// </summary>
        /// <remarks>
        /// Generally, this should not be the same object that is running this instance of ObjectToggler.
        /// Disabling that object would stop the script from running.  A common setup is to put ObjectToggler
        /// on the parent of the Target GameObject.
        /// </remarks>
        public GameObject Target;
    }

    /// <summary>
    /// The set of mappings from keycodes to GameObjects.
    /// </summary>
    public Mapping[] Mappings = new Mapping[] {};

    void Update()
    {
        foreach (Mapping mapping in Mappings)
        {
            if (Input.GetKeyDown(mapping.DownKey) || Input.GetKeyUp (mapping.UpKey))
                mapping.Target.SetActiveRecursively(!mapping.Target.active);
        }
    }
}
