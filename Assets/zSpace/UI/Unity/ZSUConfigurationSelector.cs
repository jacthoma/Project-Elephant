////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// A discrete slider which lets the user select between several GameObjects.
/// </summary>
public class ZSUConfigurationSelector : MonoBehaviour
{
    /// <summary>
    /// The list of GameObjects the user can choose between.
    /// </summary>
    public GameObject[] Configurations = new GameObject[] {};

    /// <summary> The configuration that will initially be active. </summary>
    public int InitialValue = 0;

    /// <summary> The index of the currently-active configuration. </summary>   
    public int Value
    {
        get { return (int)((float)Configurations.Length * _slider.Value); }
        set { _slider.Value = (float)value / (float)Configurations.Length; }
    }

    /// <summary>
    /// Will each GameObject be activated in addition to or instead of the previous ones?
    /// </summary>
    public bool IsCumulative = false;
    public ZSUFrameworkControlProxy SliderProxy;

    protected Slider _slider;

    void Start()
    {
        if (_slider == null)
            _slider = (SliderProxy.FrameworkControl as Slider) ?? (SliderProxy.FrameworkControl as SliderDialog).Slider;

        if (_slider != null)
        {
            // Add slider with a tick for each configuration.
    
            if (Configurations != null)
            {
                var ticks = new Slider.Tick[Configurations.Length];
                for (int i = 0; i < ticks.Length; ++i)
                {
                    ticks[i] = new Slider.Tick();
                    ticks[i].Value = (float)i / (float)Configurations.Length;
                    ticks[i].Label = (i + 1).ToString();
                }
                _slider.Ticks = ticks;
                _slider.SnapsToTicks = true;
    
                _slider.InitialValue = (float)InitialValue / ticks.Length;
            }
    
            _slider.Moved += OnSliderMoved;
        }
    }

    void OnSliderMoved(FrameworkMessage message)
    {
        if (Configurations == null)
            return;

        int level = Value;
        for (int i = 0; i < Configurations.Length; ++i)
            Configurations[i].SetActiveRecursively((IsCumulative) ? i <= level : i == level);
    }
}
