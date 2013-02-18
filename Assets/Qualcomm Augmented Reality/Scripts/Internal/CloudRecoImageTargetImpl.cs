/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CloudRecoImageTargetImpl : TrackableImpl, ImageTarget
{
    #region PRIVATE_MEMBER_VARIABLES

    private readonly Vector2 mSize;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    public CloudRecoImageTargetImpl(string name, int id, Vector2 size)
        : base(name, id)
    {
        Type = TrackableType.IMAGE_TARGET;
        mSize = size;
    }

    #endregion // CONSTRUCTION



    #region PROPERTIES

    /// <summary>
    /// The type of this ImageTarget (Predefined, User Defined, Cloud Reco)
    /// </summary>
    public ImageTargetType ImageTargetType
    {
        get { return ImageTargetType.CLOUD_RECO; }
    }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the size (width and height) of the target (in 3D scene units).
    /// </summary>
    public Vector2 GetSize()
    {
        return mSize;
    }


    /// <summary>
    /// Sets the size (width and height) of the target (in 3D scene units).
    /// This is not supported for CloudReco targets.
    /// </summary>
    public void SetSize(Vector2 size)
    {
        Debug.LogError("Setting the size of cloud reco targets is currently not supported.");
    }


    /// <summary>
    /// Creates a new virtual button and adds it to the ImageTarget
    /// This is not supported for CloudReco targets.
    /// </summary>
    public VirtualButton CreateVirtualButton(string name, VirtualButton.RectangleData area)
    {
        Debug.LogError("Virtual buttons are currently not supported for cloud reco targets.");
        return null;
    }


    /// <summary>
    /// Returns a virtual button by its name
    /// This is not supported for CloudReco targets.
    /// </summary>
    public VirtualButton GetVirtualButtonByName(string name)
    {
        Debug.LogError("Virtual buttons are currently not supported for cloud reco targets.");
        return null;
    }

    /// <summary>
    ///  Returns the virtual buttons that are defined for this imageTarget
    /// </summary>
    public IEnumerable<VirtualButton> GetVirtualButtons()
    {
        Debug.LogError("Virtual buttons are currently not supported for cloud reco targets.");
        return new List<VirtualButton>();
    }

    /// <summary>
    /// Removes and destroys one of the ImageTarget's virtual buttons
    /// This is not supported for CloudReco targets.
    /// </summary>
    public bool DestroyVirtualButton(VirtualButton vb)
    {
        Debug.LogError("Virtual buttons are currently not supported for cloud reco targets.");
        return false;
    }

    #endregion // PUBLIC_METHODS


}