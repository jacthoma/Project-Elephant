////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Maintains a database of "visualizations" which act as a theme for Controls in a user interface.
/// </summary>
public class ZSUAppearanceSet : ZSUMonoBehavior
{
    /// <summary>
    /// If true, warnings will be emitted when Controls request Visualizer content that is not available.
    /// </summary>
    public bool WarnIfMissingContent;

    // TODO: phase these out. 'Default' anything should instead be
    // stored as a visualizer in the appearance set using a node 
    // name of a target control type and no .[vis-class] suffix.
    public Material DefaultBackground;
    public Material DefaultForeground;

    /// <summary>
    /// Finds an appropriate visualizer for the given descriptor and attaches it to the given object.
    /// </summary>
    public ZSUVisualizerBase ResolveVisualizerAndAttach(VisualizationDescriptor descriptor, GameObject targetObject)
    {
        if (targetObject.gameObject.GetComponent<ZSUVisualizerBase>() != null)
        {
            Debug.LogError("Target object already has a visualizer.");
            return null;
        }

        GameObject visualizerNode = null;
        ZSUVisualizerBase visualizer = null;

        //
        // Phase 1: Attempt to locate a compatible descendent prefab or game object.
        //
        if (visualizer == null)
        {
            var matchingChild = SelectObject(descriptor);
            if (matchingChild != null)
            {
                var matchingChildVisualizer = matchingChild.GetComponent<ZSUVisualizerBase>();
                if (matchingChildVisualizer != null)
                {
                    // todo: check subclass to ensure its generic argument matches descriptor.type or is simply ZSUVisualizer.
                    visualizerNode = (GameObject)GameObject.Instantiate(matchingChild);
                    visualizer = visualizerNode.GetComponent<ZSUVisualizerBase>();
                }
            }
        }

        //
        // Phase 2: Fallback to type-based lookup.
        //
        if (visualizer == null)
        {
            Type visualizerType = Utility.FindType("ZSU" + descriptor.TargetType.Name + "Visualizer");
            if (visualizerType == null)
            {
                IsMissing("Visualizer", descriptor);
                visualizerType = typeof(ZSUVisualizerBase);
            }

            // Created dedicated child.
            {
                visualizerNode = new GameObject();
            }

            // Create visualizer.
            visualizer = (ZSUVisualizerBase)visualizerNode.AddComponent(visualizerType);
        }

        // 
        // Phase 3: common enforcements.
        //

        //visualizerNode.hideFlags = HideFlags.HideAndDontSave;
        visualizerNode.transform.parent = targetObject.transform;
        visualizerNode.layer = targetObject.layer;
        visualizerNode.transform.localPosition = Vector3.zero;
        visualizerNode.transform.localRotation = Quaternion.identity;
        visualizerNode.transform.localScale = Vector3.one;

        visualizerNode.name = string.Format("<{0}>", visualizer.GetType().Name);
        visualizer.AppearanceSet = this;

        return visualizer;
    }

    /// <summary>
    /// Returns the first object (if any) that matches the given descriptor.
    /// </summary>
    protected GameObject SelectObject(VisualizationDescriptor descriptor)
    {
        var descriptorChain = descriptor.AsChain();

        // Select all immediate children that have visualization components.
        foreach (ZSUVisualizerBase visualizer
            in this.transform.Cast<Transform>().Select(t => t.GetComponent<ZSUVisualizerBase>()).Where(v => v != null))
        {
            SelectorNode[] selectorChain = SelectorNode.Parse(visualizer.gameObject.name);
            if (selectorChain == null)
            {
                continue;
            }

            bool isVisualizerMatched = DoesSelectorMatchDescriptorChain(selectorChain, descriptorChain);
            if (isVisualizerMatched)
            {
                return visualizer.gameObject;
            }
        }

        // Fall back to old method.
        // TODO: Phase this out and return null.
        return SelectObject2(descriptor, false);
    }

    /// <summary>
    /// Deprecated: Selects an object based on a descriptor with support for incomplete matches.
    /// </summary>
    protected GameObject SelectObject2(VisualizationDescriptor descriptor, bool allowPartialMatch)
    {
        // todo: use CSS-style precedence/search rules?


        var descriptorChain = descriptor.AsChain();
        GameObject candidateObject = null;

        for (int i = 1; i <= descriptorChain.Length; ++i)
        {
            GameObject candidateObjectForChainLengthI = this.gameObject;

            foreach (VisualizationDescriptor descriptorTemp in descriptorChain.Skip(descriptorChain.Length - i))
            {
                string targetType = descriptorTemp.TargetType.Name;
                string targetClass = descriptorTemp.Class;
                string[] searchStrings = new string[]
                {
                    targetType + "." + targetClass, // type.class
                    "." + targetClass, // .class
                    targetType // type
                };

                bool childFound = false;
                foreach (string searchString in searchStrings)
                {
                    var child = candidateObjectForChainLengthI.transform.FindChild(searchString);
                    if (child == null)
                    {
                        continue;
                    }

                    candidateObjectForChainLengthI = child.gameObject;
                    childFound = true;
                }

                if (childFound == false)
                {
                    if (!allowPartialMatch)
                    {
                        candidateObjectForChainLengthI = null;
                    }
                    break;
                }
            }

            if (candidateObjectForChainLengthI != null)
            {
                candidateObject = candidateObjectForChainLengthI;
            }
        }

        return candidateObject;
    }

    /// <summary>
    /// CSS-inspired, simplified selector matching.
    /// </summary>
    protected bool DoesSelectorMatchDescriptorChain(SelectorNode[] selectorChain, VisualizationDescriptor[] descriptorChain)
    {
        // 
        // Algorithm: Starting at the leaf selector node and leaf descriptor node,
        // we move up both chains, acknowledging compatible node pairs on the way up.
        // The ParentRelationship value is important in permitting descriptor nodes
        // to be skipped or not. The algorithm terminates with a match if we have fully traversed
        // up the selector chain without finding any incompatibility between the chains,
        // and terminates without a match if otherwise.
        //

        VisualizationDescriptor descriptorNode = descriptorChain.LastOrDefault();
        bool isLeafMatched = false;
        for (int i = selectorChain.Length - 1; i >= 0; --i)
        {
            if (descriptorNode == null)
            {
                // Ran out of descriptor nodes, thus the selector was not fully matched.
                return false;
            }
            SelectorNode selectorNode = selectorChain[i];
            if ((selectorNode.Class != null && selectorNode.Class != descriptorNode.Class)
                || (selectorNode.Type != null && selectorNode.Type != descriptorNode.TargetType.Name))
            {
                if (isLeafMatched)
                {
                    // Not a matching node. 
                    if (selectorNode.ParentRelationship == SelectorNode.SelectorNodeRelationship.Direct)
                    {
                        // We needed to have directly matched the parent here or bust.
                        return false;
                    }
                    else
                    {
                        // Let's keep our selector position, but move up the descriptor chain.
                        i++;
                        descriptorNode = descriptorNode.ParentDescriptor;
                        continue;
                    }
                }
                else
                {
                    // We needed to at least match the leaf node.
                    return false;
                }
            }
            else
            {
                // Found a match.
                if (!isLeafMatched)
                {
                    isLeafMatched = true;
                }
                descriptorNode = descriptorNode.ParentDescriptor;
            }
        }

        // Completed traversal of the selector chain without returning early. 
        // Complete match.
        return true;
    }

    protected void IsMissing(string contentType, VisualizationDescriptor descriptor)
    {
        if (!this.WarnIfMissingContent)
        {
            return;
        }

        if (_warnings == null)
        {
            _warnings = new HashSet<string>();
        }

        string warningMessage = string.Format("Missing '{0}' for descriptor '{1}'.", contentType, descriptor.ToString());
        if (_warnings.Contains(warningMessage))
        {
            return;
        }

        Debug.LogWarning(warningMessage);
        _warnings.Add(warningMessage);
    }

    /// <summary>
    /// Represents a parsed node of a selector string.
    /// </summary>
    protected class SelectorNode
    {
        public enum SelectorNodeRelationship
        {
            Direct,
            Indirect,
        }

        public SelectorNode Parent;
        public SelectorNodeRelationship ParentRelationship;
        public string Class;
        public string Type;

        private SelectorNode() { }

        /// <summary>
        /// Parses the selector string into a sequence of selector nodes.
        /// Returns the leaf node.
        /// </summary>
        public static SelectorNode[] Parse(string fullSelector)
        {
            Regex selectorNodeStringValidator = new Regex(@"^\w+\.\w+|\w+|\.\w+$");

            List<string> nodeStrings = fullSelector.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<SelectorNode> nodes = new List<SelectorNode>(nodeStrings.Count);
            SelectorNode node = null;
            for (int i = nodeStrings.Count - 1; i >= 0; --i)
            {
                string nodeString = nodeStrings[i];

                if (nodeString == ">")
                {
                    if (node == null)
                    {
                        // We don't have a leaf node yet. '>' is invalid to discover first.
                        Debug.LogError("Bad syntax in selector \"" + fullSelector + "\".");
                        return null;
                    }
                    else
                    {
                        node.ParentRelationship = SelectorNodeRelationship.Direct;
                        continue;
                    }
                }
                else
                {
                    if (!selectorNodeStringValidator.IsMatch(nodeString))
                    {
                        Debug.LogError("Bad syntax in selector \"" + fullSelector + "\".");
                        return null;
                    }

                    SelectorNode newNode = new SelectorNode();
                    newNode.Parent = node;
                    newNode.ParentRelationship = SelectorNodeRelationship.Indirect; // Indirect until we know otherwise.

                    string[] nodeStringSplit = nodeString.Split('.');

                    if (nodeStringSplit.Length == 1)
                    {
                        if (nodeString.StartsWith("."))
                        {
                            newNode.Class = nodeString;
                        }
                        else
                        {
                            newNode.Type = nodeString;
                        }
                    }
                    else
                    {
                        newNode.Type = nodeStringSplit[0];
                        newNode.Class = nodeStringSplit[1];
                    }

                    nodes.Add(newNode);
                    node = newNode;
                }
            }

            if (nodes.Count == 0)
            {
                return null;
            }
            else
            {
                return nodes.AsEnumerable().Reverse().ToArray();
            }
        }
    }

    private HashSet<string> _warnings = null;
}

