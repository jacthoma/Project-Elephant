////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

/// <summary>
/// Internal class for representing a Window Control.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ZSUWindowVisualizer : ZSUVisualizer<zSpace.UI.Utility.Window>
{
	public override void Synchronize()
	{
		base.Synchronize();
		
		BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = this.FrameworkControl.FinalSize;
	}
}

