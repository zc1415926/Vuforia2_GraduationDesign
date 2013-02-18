/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


/// <summary>
/// This class serves as a singleton to retrieve the Trackers and the StateManager
/// </summary>
public abstract class TrackerManager
{
    #region PROPERTIES

    /// <summary>
    /// Returns an instance of a TrackerManager (thread safe)
    /// </summary>
    public static TrackerManager Instance
    {
        get
        {
            // Make sure only one instance of TrackerManager is created.
            if (mInstance == null)
            {
                lock (typeof(TrackerManager))
                {
                    if (mInstance == null)
                    {
                        mInstance = new TrackerManagerImpl();
                    }
                }
            }
            return mInstance;
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    private static TrackerManager mInstance = null;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the instance of the given tracker type
    /// See the Tracker base class for a list of available tracker classes.
    /// This function will return null if the tracker of the given type has
    /// not been initialized.
    /// </summary>
    public abstract Tracker GetTracker(Tracker.Type trackerType);


    /// <summary>
    /// Initializes the tracker of the given type
    /// Initializing a tracker must not be done when the CameraDevice
    /// is initialized or started. This function will return null if the
    /// CameraDevice is currently initialized.
    /// </summary>
    public abstract Tracker InitTracker(Tracker.Type trackerType);


    /// <summary>
    /// Deinitializes the tracker of the given type and frees any resources
    /// used by the tracker.
    /// Deinitializing a tracker must not be done when the CameraDevice
    /// is initialized or started. This function will return false if the
    /// tracker of the given type has not been initialized or if the
    /// CameraDevice is currently initialized.
    /// </summary>
    public abstract bool DeinitTracker(Tracker.Type trackerType);


    /// <summary>
    /// Returns the state manager instance that can be used to access
    /// all currently tracked TrackableBehaviours
    /// </summary>
    public abstract StateManager GetStateManager();

    #endregion // PUBLIC_METHODS
}
