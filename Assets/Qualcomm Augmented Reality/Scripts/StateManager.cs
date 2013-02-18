/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to manage the state of all TrackableBehaviours, create them,
/// associate them with Trackables, update their pose, etc.
/// </summary>
public abstract class StateManager
{
    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the TrackableBehaviours currently being tracked
    /// </summary>
    public abstract IEnumerable<TrackableBehaviour> GetActiveTrackableBehaviours();

    /// <summary>
    /// Returns all currently instantiated TrackableBehaviours
    /// </summary>
    public abstract IEnumerable<TrackableBehaviour> GetTrackableBehaviours();

    #endregion // PUBLIC_METHODS
}