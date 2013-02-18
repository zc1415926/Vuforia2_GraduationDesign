/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

/// <summary>
/// This class serves both as an augmentation definition for a Marker in the editor
/// as well as a tracked marker result at runtime
/// </summary>
public class MarkerBehaviour : TrackableBehaviour, IEditorMarkerBehaviour
{

    #region PROPERTIES

    /// <summary>
    /// The Marker that this MarkerBehaviour augments
    /// </summary>
    public Marker Marker
    {
        get { return mMarker; }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES
    
    [SerializeField]
    [HideInInspector]
    private int mMarkerID;

    private Marker mMarker;
    
    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    public MarkerBehaviour()
    {
        mMarkerID = -1;
    }

    #endregion // CONSTRUCTION



    #region PROTECTED_METHODS

    /// <summary>
    /// This method disconnects the TrackableBehaviour from it's associated trackable.
    /// Use it only if you know what you are doing - e.g. when you want to destroy a trackable, but reuse the TrackableBehaviour.
    /// </summary>
    protected override void InternalUnregisterTrackable()
    {
        mTrackable = mMarker = null;
    }

    #endregion // PROTECTED_METHODS



    #region PROTECTED_METHODS

    /// <summary>
    /// Scales the Trackable uniformly
    /// </summary>
    protected override bool CorrectScaleImpl()
    {
        bool scaleChanged = false;

        for (int i = 0; i < 3; ++i)
        {
            // Force uniform scale:
            if (this.transform.localScale[i] != mPreviousScale[i])
            {
                this.transform.localScale =
                    new Vector3(this.transform.localScale[i],
                                this.transform.localScale[i],
                                this.transform.localScale[i]);

                mPreviousScale = this.transform.localScale;
                scaleChanged = true;
                break;
            }
        }

        return scaleChanged;
    }

    #endregion // PROTECTED_METHODS



    #region EDITOR_INTERFACE_IMPLEMENTATION

    // The marker ID (as opposed to the trackable's ID)
    int IEditorMarkerBehaviour.MarkerID
    {
        get
        {
            return mMarkerID;
        }
    }

    // sets the marker ID (not allowed at runtime)
    bool IEditorMarkerBehaviour.SetMarkerID(int markerID)
    {
        if (mTrackable == null)
        {
            mMarkerID = markerID;
            return true;
        }
        return false;
    }

    // Initializes the MarkerBehaviour with the Marker object. 
    void IEditorMarkerBehaviour.InitializeMarker(Marker marker)
    {
        mTrackable = mMarker = marker;
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION

}
