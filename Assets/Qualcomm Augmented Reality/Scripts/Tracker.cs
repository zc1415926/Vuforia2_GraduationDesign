/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

/// <summary>
/// The common base class for the ImageTracker and the MarkerTracker
/// </summary>
public abstract class Tracker
{
    #region NESTED

    /// <summary>
    /// Enumeration of the different tracker types
    /// </summary>
    public enum Type
    {
        IMAGE_TRACKER,    // Tracks ImageTargets and MultiTargets
        MARKER_TRACKER    // Tracks Markers
    }

    #endregion // NESTED



    #region PUBLIC_METHODS

    /// <summary>
    /// Starts the Tracker
    /// </summary>
    public abstract bool Start();

    /// <summary>
    /// Stops the Tracker
    /// </summary>
    public abstract void Stop();

    #endregion // PUBLIC_METHODS



    #region PROTECTED_METHODS

    /// <summary>
    /// Position the camera relative to a Trackable.
    /// </summary>
    protected void PositionCamera(TrackableBehaviour trackableBehaviour,
                                  Camera arCamera,
                                  QCARManagerImpl.PoseData camToTargetPose)
    {
        arCamera.transform.localPosition =
                trackableBehaviour.transform.rotation *
                Quaternion.AngleAxis(90, Vector3.left) *
                Quaternion.Inverse(camToTargetPose.orientation) *
                (-camToTargetPose.position) +
                trackableBehaviour.transform.position;

        arCamera.transform.rotation =
                trackableBehaviour.transform.rotation *
                Quaternion.AngleAxis(90, Vector3.left) *
                Quaternion.Inverse(camToTargetPose.orientation);
    }    
    

    /// <summary>
    /// Position a Trackable relative to the Camera.
    /// </summary>
    protected void PositionTrackable(TrackableBehaviour trackableBehaviour,
                                     Camera arCamera,
                                     QCARManagerImpl.PoseData camToTargetPose)
    {
        trackableBehaviour.transform.position =
                arCamera.transform.TransformPoint(camToTargetPose.position);

        trackableBehaviour.transform.rotation =
                arCamera.transform.rotation *
                camToTargetPose.orientation *
                Quaternion.AngleAxis(270, Vector3.left);
    }

    #endregion // PROTECTED_METHODS
}