/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// The type of an ImageTarget. An ImageTarget can be predefined in a dataset,
/// created at runtime as a user defined target, or fetched at runtime via
/// cloud recognition
/// </summary>
public enum ImageTargetType
{
    PREDEFINED = 0,
    USER_DEFINED = 1,
    CLOUD_RECO = 2
}

/// <summary>
/// The basic data struct for an ImageTarget
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ImageTargetData
{
    public int id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public Vector2 size;
}

/// <summary>
/// A trackable behaviour to represent a flat natural feature target.
/// </summary>
public interface ImageTarget : Trackable
{
    #region PROPERTIES

    /// <summary>
    /// The type of this ImageTarget (Predefined, User Defined, Cloud Reco)
    /// </summary>
    ImageTargetType ImageTargetType { get; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the size (width and height) of the target (in 3D scene units).
    /// </summary>
    Vector2 GetSize();

    /// <summary>
    /// Sets the size (width and height) of the target (in 3D scene units).
    /// This is only allowed when the dataset is not active!
    /// </summary>
    void SetSize(Vector2 size);

    /// <summary>
    /// Creates a new virtual button and adds it to the ImageTarget
    /// Returns NULL if the corresponding DataSet is currently active.
    /// </summary>
    VirtualButton CreateVirtualButton(string name, VirtualButton.RectangleData area);

    /// <summary>
    /// Returns a virtual button by its name
    /// Returns NULL if no virtual button with that name
    /// exists in this ImageTarget
    /// </summary>
    VirtualButton GetVirtualButtonByName(string name);

    /// <summary>
    ///  Returns the virtual buttons that are defined for this imageTarget
    /// </summary>
    IEnumerable<VirtualButton> GetVirtualButtons();

    /// <summary>
    /// Removes and destroys one of the ImageTarget's virtual buttons
    /// Returns false if the corresponding DataSet is currently active.
    /// </summary>
    bool DestroyVirtualButton(VirtualButton vb);

    #endregion // PUBLIC_METHODS
}