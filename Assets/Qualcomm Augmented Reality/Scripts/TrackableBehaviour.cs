/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// The base class for all TrackableBehaviours in Vuforia
/// This class serves both as an augmentation definition for a Trackable in the editor
/// as well as a tracked Trackable result at runtime
/// </summary>
public abstract class TrackableBehaviour : MonoBehaviour, IEditorTrackableBehaviour
{    
    #region NESTED

    /// <summary>
    /// The tracking status of the TrackableBehaviour.
    /// </summary>
    public enum Status
    {
        NOT_FOUND = -1,
        UNKNOWN = 0,            ///< The state of the TrackableResult is unknown
        UNDEFINED = 1,          ///< The state of the TrackableResult is not defined
        DETECTED = 2,           ///< The TrackableResult was detected
        TRACKED = 3             ///< The TrackableResult was tracked
    }

    #endregion //NESTED



    #region PROPERTIES

    /// <summary>
    /// The tracking status of the TrackableBehaviour
    /// </summary>
    public Status CurrentStatus
    {
        get { return mStatus; }
    }

    /// <summary>
    /// The Trackable created at runtime that is augmented by this TrackableBehaviour
    /// </summary>
    public Trackable Trackable
    {
        get { return mTrackable; }
    }

    /// <summary>
    /// The name of the Trackable.
    /// </summary>
    public string TrackableName
    {
        get
        {
            return mTrackableName;
        }
    }

    #endregion // PROPERTIES



    #region PROTECTED_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    protected string mTrackableName = "";

    [SerializeField]
    [HideInInspector]
    protected Vector3 mPreviousScale = Vector3.zero;

    [SerializeField]
    [HideInInspector]
    protected bool mPreserveChildSize = false;

    [SerializeField]
    [HideInInspector]
    protected bool mInitializedInEditor = false;

    protected Status mStatus = Status.UNKNOWN;
    protected Trackable mTrackable;
    
    #endregion // PROTECTED_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private List<ITrackableEventHandler> mTrackableEventHandlers =
                                new List<ITrackableEventHandler>();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    /// This method registers a new Tracker event handler at the Tracker.
    /// These handlers are called as soon as ALL Trackables have been updated
    /// in this frame.
    /// </summary>
    public void RegisterTrackableEventHandler(
                                ITrackableEventHandler trackableEventHandler)
    {
        mTrackableEventHandlers.Add(trackableEventHandler);
    }


    /// <summary>
    /// This method unregisters a Tracker event handler.
    /// Returns "false" if event handler does not exist.
    /// </summary>
    public bool UnregisterTrackableEventHandler(
                                ITrackableEventHandler trackableEventHandler)
    {
        return mTrackableEventHandlers.Remove(trackableEventHandler);
    }


    /// <summary>
    /// Is triggered by the TrackerBehavior after it has updated.
    /// </summary>
    public void OnTrackerUpdate(Status newStatus)
    {
        // Update status:
        Status prevStatus = mStatus;
        mStatus = newStatus;

        if (prevStatus != newStatus)
        {
            foreach (ITrackableEventHandler handler in mTrackableEventHandlers)
            {
                handler.OnTrackableStateChanged(prevStatus, newStatus);
            }
        }
    }

    #endregion // PUBLIC_METHODS



    #region PROTECTED_METHODS

    /// <summary>
    /// This method disconnects the TrackableBehaviour from it's associated trackable.
    /// Use it only if you know what you are doing - e.g. when you want to destroy a trackable, but reuse the TrackableBehaviour.
    /// </summary>
    protected abstract void InternalUnregisterTrackable();

    #endregion // PROTECTED_METHODS



    #region UNITY_MONOBEHAVIOUR_METHODS

    // Overriding standard Unity MonoBehaviour methods.

    void Start()
    {
        // Note: Empty, but this forces the enabled checkbox in the editor
        // to become visible.
    }


    void OnDisable()
    {
        // Update status:
        Status prevStatus = mStatus;
        mStatus = Status.NOT_FOUND;

        if (prevStatus != Status.NOT_FOUND)
        {
            foreach (ITrackableEventHandler handler in mTrackableEventHandlers)
            {
                handler.OnTrackableStateChanged(prevStatus, Status.NOT_FOUND);
            }
        }
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region EDITOR_INTERFACE_IMPLEMENTATION

    // Scales Trackable uniformly
    bool IEditorTrackableBehaviour.CorrectScale()
    {
        return CorrectScaleImpl();
    }

    protected virtual bool CorrectScaleImpl()
    {
        return false;
    }

    // Sets the name of the trackable before it's created
    // can be used from the editor or during runtime for newly created TrackableBehaviours
    bool IEditorTrackableBehaviour.SetNameForTrackable(string name)
    {
        // using this method is only allowed
        if (mTrackable == null)
        {
            mTrackableName = name;
            return true;
        }
        return false;
    }

    Vector3 IEditorTrackableBehaviour.PreviousScale
    {
        get { return mPreviousScale; }
    }

    // Sets the PreviousScale to a given value
    // can be used from the editor or during runtime for newly created TrackableBehaviours
    bool IEditorTrackableBehaviour.SetPreviousScale(Vector3 previousScale)
    {
        if (Trackable == null)
        {
            mPreviousScale = previousScale;
            return true;
        }

        return false;
    }

    bool IEditorTrackableBehaviour.PreserveChildSize
    {
        get { return mPreserveChildSize; }
    }

    // Sets the PreserveChildSize flag
    // can be used from the editor or during runtime for newly created TrackableBehaviours
    bool IEditorTrackableBehaviour.SetPreserveChildSize(bool preserveChildSize)
    {
        if (Trackable == null)
        {
            mPreserveChildSize = preserveChildSize;
            return true;
        }

        return false;
    }

    bool IEditorTrackableBehaviour.InitializedInEditor
    {
        get { return mInitializedInEditor; }
    }

    // used to remember if the Trackable behaviour has been initialized in the editor
    // can be used from the editor or during runtime for newly created TrackableBehaviours
    bool IEditorTrackableBehaviour.SetInitializedInEditor(bool initializedInEditor)
    {
        if (Trackable == null)
        {
            mInitializedInEditor = initializedInEditor;
            return true;
        }
        return false;
    }

    /// <summary>
    /// This method disconnects the TrackableBehaviour from it's associated trackable.
    /// Use it only if you know what you are doing - e.g. when you want to destroy a trackable, but reuse the TrackableBehaviour.
    /// </summary>
    void IEditorTrackableBehaviour.UnregisterTrackable()
    {
        InternalUnregisterTrackable();
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION
}
