////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Associates a message with an animation which will be played back when the message is raised.
/// </summary>
[System.Serializable]
public class AnimationMapping
{
    /// <summary>
    /// The message that will initiate the animation.
    /// </summary>
    public string MessageName;

    /// <summary>
    /// The Animation that will be played when the message is raised.
    /// </summary>
    public Animation Animation;

    /// <summary>
    /// The name of the clip in the Animation which will be played.
    /// </summary>
    public string AnimationName;
}


/// <summary>
/// Responds to messages by playing animations, which can affect any float or vector field in the representation or its descendants.
/// </summary>
public class ZSUAnimationTriggerSet : ZSUTriggerSet
{
    /// <summary>
    /// The set of mappings between messages and animations.
    /// </summary>
    public AnimationMapping[] AnimationMappings = new AnimationMapping[0];

    protected override void OnScriptAwake()
    {
        _mappings = new Mapping[AnimationMappings.Length];
        for (int i = 0; i < AnimationMappings.Length; ++i)
        {
            Animation animation = AnimationMappings[i].Animation;
            string animationName = AnimationMappings[i].AnimationName;
            FrameworkMessageHandler handler = (FrameworkMessage message) =>
            {
                if (String.IsNullOrEmpty(animationName))
                    animation.Play();
                else
                    animation.Play(animationName);
            };

            _mappings[i].Handler = handler;
            _mappings[i].MessageName = AnimationMappings[i].MessageName;
        }

        base.OnScriptAwake();
    }
}
