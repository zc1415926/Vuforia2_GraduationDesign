/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This class handles marker creation and management at runtime
/// </summary>
public abstract class MarkerTracker : Tracker
{
    #region PUBLIC_METHODS

    /// <summary>
    /// Creates a marker with the given id, name, and size.
    /// Registers the marker at native code.
    /// Returns a MarkerBehaviour object to receive updates.
    /// </summary>
    public abstract MarkerBehaviour CreateMarker(int markerID, String trackableName, float size);


    /// <summary>
    /// Destroys the Marker at native code.
    /// </summary>
    public abstract bool DestroyMarker(Marker marker, bool destroyGameObject);


    /// <summary>
    /// Returns the markers registered in native.
    /// </summary>
    public abstract IEnumerable<Marker> GetMarkers();


    /// <summary>
    /// Returns the Marker with the given MarkerID.
    /// Returns null if none is found.
    /// </summary>
    public abstract Marker GetMarkerByMarkerID(int markerID);


    /// <summary>
    /// Destroys all existing markers
    /// </summary>
    public abstract void DestroyAllMarkers(bool destroyGameObject);


    #endregion // PUBLIC_METHODS
}