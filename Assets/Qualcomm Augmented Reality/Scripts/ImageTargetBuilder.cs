/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This class encapsulates all functionality needed to create a user defined target on the fly.
/// </summary>
public abstract class ImageTargetBuilder
{
    #region NESTED

    /// <summary>
    /// The frame quality is used to determine if the current frame can be used to create a good target
    /// </summary>
    public enum FrameQuality
    {
        FRAME_QUALITY_NONE = -1,    ///< getFrameQualty was called oustside of scanning mode
        FRAME_QUALITY_LOW = 0,      ///< Poor number of features for tracking
        FRAME_QUALITY_MEDIUM = 1,   ///< Sufficient number features for tracking
        FRAME_QUALITY_HIGH = 2,     ///< Ideal number of features for tracking
    };

    #endregion // NESTED



    #region PUBLIC_METHODS

    /// <summary>
    /// Build an Image Target Trackable source from the next available camera frame 
    /// 
    /// Build an Image Target Trackable Source from the next available camera frame.
    /// This is an asynchronous process, the result of which will be available from
    ///  getTrackableSource()
    /// 
    /// This method will return true if the build was successfully started, and false
    /// if an invalid name or sceenSizeWidth is provided.
    /// </summary>
    public abstract bool Build(string targetName, float sceenSizeWidth);

    /// <summary>
    /// Start the scanning mode, allowing calls to getFrameQuality()
    /// 
    /// Starts the internal frame scanning process, allowing calls to getFrameQuality()
    /// </summary>
    public abstract void StartScan();

    /// <summary>
    /// Stop the scanning mode
    /// 
    /// Stop the scanning mode, getFrameQuality will return FRAME_QUALITY_NONE until
    /// startScan is called again.  Stopping scan mode will reduce the overall system
    /// utilization when not building ImageTargets.
    /// </summary>
    public abstract void StopScan();

    /// <summary>
    /// Get frame quality, available after startScan is called.
    /// 
    /// Will return the frame quality for the last available camera frame, a value
    /// of FRAME_QUALITY_NONE will be returned if the scanning mode was not enabled.
    /// via the startScan() method.
    /// 
    /// This methods makes a marshalling call into native, so buffer the result
    /// instead of calling it multiple times a frame for better performance.
    /// </summary>
    public abstract FrameQuality GetFrameQuality();

    /// <summary>
    /// Returns a trackable source object to be used in adding a new target to a dataset
    /// 
    /// This method will return a TrackableSource to be provided to the DataSet.  This 
    /// API will return NULL until a trackable source is available.  This trackable
    /// source will be provided via this api until build() is called again, at which
    /// point it will return NULL again until a successful build step has occured.
    /// 
    /// This methods makes a marshalling call into native, so buffer the result
    /// instead of calling it multiple times a frame for better performance.
    /// </summary>
    public abstract TrackableSource GetTrackableSource();


    #endregion // PUBLIC_METHODS
}
