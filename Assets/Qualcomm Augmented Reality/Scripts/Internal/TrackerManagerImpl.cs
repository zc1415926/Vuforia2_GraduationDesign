/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System.Runtime.InteropServices;

public class TrackerManagerImpl : TrackerManager
{
    #region PRIVATE_MEMBER_VARIABLES

    private ImageTracker mImageTracker = null;

    private MarkerTracker mMarkerTracker = null;

    private StateManager mStateManager = new StateManagerImpl();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    // Returns the instance of the given tracker type
    // See the Tracker base class for a list of available tracker classes.
    // This function will return null if the tracker of the given type has
    // not been initialized.
    public override Tracker GetTracker(Tracker.Type trackerType)
    {
        if (trackerType == Tracker.Type.IMAGE_TRACKER)
        {
            return mImageTracker;
        }
        else if (trackerType == Tracker.Type.MARKER_TRACKER)
        {
            return mMarkerTracker;
        }
        else
        {
            Debug.LogError("Could not return tracker. Unknow tracker type.");
            return null;
        }
    }


    // Initializes the tracker of the given type
    // Initializing a tracker must not be done when the CameraDevice
    // is initialized or started. This function will return null if the
    // CameraDevice is currently initialized.
    public override Tracker InitTracker(Tracker.Type trackerType)
    {
        if (!QCARRuntimeUtilities.IsQCAREnabled())
        {
            return null;
        }

        if (QCARWrapper.Instance.TrackerManagerInitTracker((int)trackerType) == 0)
        {
            Debug.LogError("Could not initialize the tracker.");
            return null;
        }

        if (trackerType == Tracker.Type.IMAGE_TRACKER)
        {
            if (mImageTracker == null)
            {
                mImageTracker = new ImageTrackerImpl();
            }
            return mImageTracker;
        }
        else if (trackerType == Tracker.Type.MARKER_TRACKER)
        {
            if (mMarkerTracker == null)
            {
                mMarkerTracker = new MarkerTrackerImpl();
            }
            return mMarkerTracker;
        }
        else
        {
            Debug.LogError("Could not initialize tracker. Unknown tracker type.");
            return null;
        }
    }


    // Deinitializes the tracker of the given type and frees any resources
    // used by the tracker.
    // Deinitializing a tracker must not be done when the CameraDevice
    // is initialized or started. This function will return false if the
    // tracker of the given type has not been initialized or if the
    // CameraDevice is currently initialized.
    public override bool DeinitTracker(Tracker.Type trackerType)
    {
        if (QCARWrapper.Instance.TrackerManagerDeinitTracker((int)trackerType) == 0)
        {
            Debug.LogError("Could not deinitialize the tracker.");
            return false;
        }

        if (trackerType == Tracker.Type.IMAGE_TRACKER)
        {
            mImageTracker = null;
        }
        else if (trackerType == Tracker.Type.MARKER_TRACKER)
        {
            mMarkerTracker = null;
        }
        else
        {
            Debug.LogError("Could not deinitialize tracker. Unknown tracker type.");
            return false;
        }

        return true;
    }

    // Returns the state manager instance that can be used to access
    // all currently tracked TrackableBehaviours
    public override StateManager GetStateManager()
    {
        return mStateManager;
    }

    #endregion // PUBLIC_METHODS
}
