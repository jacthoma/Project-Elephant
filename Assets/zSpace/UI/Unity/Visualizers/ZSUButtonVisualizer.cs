////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// Internal class for representing a Button Control.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ZSUButtonVisualizer : ZSUVisualizer<Button>
{
	public override void Synchronize()
	{
		base.Synchronize();
				
		BoxCollider boxCollider = this.GetComponent<BoxCollider>();
		boxCollider.size = this.FrameworkControl.FinalSize;
	}
}

