/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// A trackable behaviour for representing rectangular markers.
/// </summary>
public interface Marker:Trackable
{
    /// <summary>
    /// Returns the size (width and height) of the target (in 3D scene units).
    /// </summary>
    float GetSize();

    /// <summary>
    /// The marker id is an ID between 0 and 511 that represents different frame markers
    /// </summary>
    int MarkerID { get; }
}