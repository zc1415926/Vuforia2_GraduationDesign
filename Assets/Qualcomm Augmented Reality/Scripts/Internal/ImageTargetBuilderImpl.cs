/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This class encapsulates all functionality needed to create a user defined target on the fly.
/// The State property exposes information about the current mode, frame quality and the building process.
/// </summary>
public class ImageTargetBuilderImpl : ImageTargetBuilder
{
    #region PRIVATE_MEMBER_VARIABLES

    // last created trackable source
    private TrackableSource mTrackableSource = null;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    ///  Build an Image Target Trackable source from the next available camera frame 
    /// </summary>
    public override bool Build(string targetName, float sceenSizeWidth)
    {
        if (targetName.Length > 64)
        {
            Debug.LogError("Invalid parameters to build User Defined Target:" +
                           "Target name exceeds 64 character limit");
            return false;
        }
        mTrackableSource = null;
        return QCARWrapper.Instance.ImageTargetBuilderBuild(targetName, sceenSizeWidth) == 1;
    }

    /// <summary>
    /// Start the scanning mode, allowing calls to getFrameQuality()
    /// </summary>
    public override void StartScan()
    {
        QCARWrapper.Instance.ImageTargetBuilderStartScan();
    }

    /// <summary>
    /// Stop the scanning mode
    /// </summary>
    public override void StopScan()
    {
        QCARWrapper.Instance.ImageTargetBuilderStopScan();
    }

    /// <summary>
    /// Get frame quality, available after startScan is called.
    /// </summary>
    public override FrameQuality GetFrameQuality()
    {
        return (FrameQuality)QCARWrapper.Instance.ImageTargetBuilderGetFrameQuality();
    }

    /// <summary>
    /// Returns a trackable source object to be used in adding a new target to a dataset
    /// </summary>
    public override TrackableSource GetTrackableSource()
    {
        IntPtr trackableSourcePtr = QCARWrapper.Instance.ImageTargetBuilderGetTrackableSource();
        if ((mTrackableSource == null) && (trackableSourcePtr != IntPtr.Zero))
            mTrackableSource = new TrackableSourceImpl(trackableSourcePtr);

        return mTrackableSource;
    }

    #endregion // PUBLIC_METHODS
}
