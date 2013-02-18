/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// Interface for handling tracker events.
/// </summary>
public interface ITrackerEventHandler
{
    /// <summary>
    /// Called after QCAR has finished initializing
    /// </summary>
    void OnInitialized();

    /// <summary>
    /// Called after all the trackable objects have been updated
    /// </summary>
    void OnTrackablesUpdated();
}
