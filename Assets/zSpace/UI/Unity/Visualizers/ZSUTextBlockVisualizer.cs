////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// Internal class for representing a Text Block Control.
/// </summary>
public class ZSUTextBlockVisualizer : ZSUVisualizer<TextBlock>
{
    /// <summary>
    /// The text color.
    /// </summary>
    public Material Color = null;

    /// <summary>
    /// The font.
    /// </summary>
    public Font Font = null;


    public override void Synchronize()
    {
        base.Synchronize();

        //
        // Ensure our internal primitives exist.
        //
        if (_textObject == null)
        {
            _textObject = new GameObject("Text", typeof(TextMesh), typeof(MeshRenderer));
            _textObject.transform.parent = this.transform;
            _textObject.layer = this.gameObject.layer;
            _textObject.transform.localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            _textObject.transform.localScale = Vector3.one;

            _textMesh = _textObject.GetComponent<TextMesh>();
            if (this.Font != null)
            {
                _textMesh.font = this.Font;
            }
            _textMesh.fontSize = 72;
            _textMesh.characterSize = 1;

            _textMeshRenderer = _textObject.GetComponent<MeshRenderer>();

            // Measure font.
            {
                float magicNumber = (float)_textMesh.fontSize / (14.0f * 15.0f);
                GUIStyle fontInfo = new GUIStyle();
                fontInfo.font = _textMesh.font;
                _fontSizeEstimationScaleFactor = fontInfo.CalcSize(new GUIContent("X")).x * magicNumber;
            }
        }

        //
        // Appearance
        //
        {
            _textMeshRenderer.sharedMaterial = Color;
        }

        //
        // Adhere to layout.
        //
        {
            Vector3 layoutSize = this.FrameworkControl.FinalSize;
            _textMesh.transform.localPosition = new Vector3(-layoutSize.x, layoutSize.y, 0) * 0.5f;
        }

        //
        // Text wrapping.
        //
        {
            string text = this.FrameworkControl.Text;

            // Estimate character size.
            float characterSize = _textMesh.characterSize * _fontSizeEstimationScaleFactor;

            // Readjust text to fit.
            string textProcessed = ForceLineBreaks(text, Math.Max(1, Mathf.RoundToInt(this.FrameworkControl.FinalSize.x / characterSize)));
            _textMesh.text = textProcessed;
        }
    }


    private string ForceLineBreaks(string text, int maxCharsPerLine)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        StringBuilder buffer = new StringBuilder();
        int charactersThisLine = 0;
        for (int i = 0; i < text.Length; ++i)
        {
            if (text[i] == ' ')
            {
                if (charactersThisLine == maxCharsPerLine)
                {
                    buffer.Append('\n');
                    charactersThisLine = 0;
                }
                else
                {
                    buffer.Append(' ');
                    charactersThisLine++;
                }
                continue;
            }

            string nextSubstring = text.SubstringUntilWhitespace(i);
            if (nextSubstring.Length > maxCharsPerLine)
            {
                buffer.Append(nextSubstring);
                i += nextSubstring.Length - 1;
                charactersThisLine = 0;
                continue;
            }
            else if (nextSubstring.Length + charactersThisLine > maxCharsPerLine)
            {
                buffer.Append('\n');
                buffer.Append(nextSubstring);
                i += nextSubstring.Length - 1;
                charactersThisLine = nextSubstring.Length;
                continue;
            }
            else
            {
                if (nextSubstring.Length == 0)
                {
                    char character = text[i];
                    if (character == '\n' || character == '\r')
                    {
                        buffer.Append(character);
                        i += 1;
                        charactersThisLine = 0;
                    }
                    else
                    {
                        buffer.Append(' ');
                        i += 1;
                        charactersThisLine += 1;
                    }
                }
                else
                {
                    buffer.Append(nextSubstring);
                    i += nextSubstring.Length - 1;
                    charactersThisLine += nextSubstring.Length;
                }
            }
        }
        return buffer.ToString();
    }


    private GameObject _textObject;
    private TextMesh _textMesh;
    private MeshRenderer _textMeshRenderer;
    private float _fontSizeEstimationScaleFactor;
}
