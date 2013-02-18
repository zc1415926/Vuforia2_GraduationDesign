/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MarkerTrackerImpl : MarkerTracker
{
    #region PRIVATE_MEMBER_VARIABLES

    // Dictionary that contains Markers.
    private readonly Dictionary<int, Marker> mMarkerDict = new Dictionary<int, Marker>();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    // Starts the tracker.
    // The tracker needs to be stopped before Trackables can be modified.
    public override bool Start()
    {
        if (QCARWrapper.Instance.MarkerTrackerStart() == 0)
        {
            Debug.LogError("Could not start tracker.");
            return false;
        }

        return true;
    }


    // Stops the tracker.
    // The tracker needs to be stopped before Trackables can be modified.
    public override void Stop()
    {
        QCARWrapper.Instance.MarkerTrackerStop();

        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();

        // Mark all markers as not found:
        foreach (var marker in mMarkerDict.Values)
        {
            stateManager.SetTrackableBehavioursForTrackableToNotFound(marker);
        }
    }


    // Creates a marker with the given id, name, and size.
    // Registers the marker at native code.
    // Returns a MarkerBehaviour object to receive updates.
    public override MarkerBehaviour CreateMarker(int markerID, String trackableName, float size)
    {
        int trackableID = RegisterMarker(markerID, trackableName, size);

        if (trackableID == -1)
        {
            Debug.LogError("Could not create marker with id " + markerID + ".");
            return null;
        }

        // create a new marker object:
        Marker marker = new MarkerImpl(trackableName, trackableID, size, markerID);

        // Add newly created Marker to dictionary.
        mMarkerDict[trackableID] = marker;

        // let the state manager create a new Marker Behaviour and return it:
        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
        MarkerBehaviour newMB = stateManager.CreateNewMarkerBehaviourForMarker(marker, trackableName);

        return newMB;
    }


    // Destroys the marker associated with the given MarkerBehaviour
    // at native code.
    public override bool DestroyMarker(Marker marker, bool destroyGameObject)
    {
        if (QCARWrapper.Instance.MarkerTrackerDestroyMarker(marker.ID) == 0)
        {
            Debug.LogError("Could not destroy marker with id " + marker.MarkerID + ".");
            return false;
        }

        mMarkerDict.Remove(marker.ID);


        if (destroyGameObject)
        {
            StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
            stateManager.DestroyTrackableBehavioursForTrackable(marker);
        }

        return true;
    }


    // Returns the markers registered in native.
    public override IEnumerable<Marker> GetMarkers()
    {
        return mMarkerDict.Values;
    }

    
    // Returns the Marker with the given MarkerID.
    // Returns null if none is found.
    public override Marker GetMarkerByMarkerID(int markerID)
    {
        foreach (Marker marker in mMarkerDict.Values)
        {
            if (marker.MarkerID == markerID)
                return marker;
        }

        return null;
    }


    public Marker InternalCreateMarker(int markerID, string name, float size)
    {
        int id = RegisterMarker(markerID, name, size);

        if (id == -1)
        {
            Debug.LogWarning("Marker named " + name +
                                " could not be created");
            return null;
        }
        else
        {
            if (!mMarkerDict.ContainsKey(id))
            {
                // create a new marker object:
                Marker marker = new MarkerImpl(name, id, size, markerID);

                // Add newly created Marker to dictionary.
                mMarkerDict[id] = marker;

                Debug.Log("Created Marker named " + marker.Name +
                            " with id " + marker.ID);
            }

            return mMarkerDict[id];
        }
    }


    public override void DestroyAllMarkers(bool destroyGameObject)
    {
        List<Marker> markersToDelete = new List<Marker>(mMarkerDict.Values);

        foreach (Marker marker in markersToDelete)
        {
            DestroyMarker(marker, destroyGameObject);
        }
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Creates a marker with the given id, name, and size.
    // Registers the marker at native code.
    // Returns the unique trackable id.
    private int RegisterMarker(int markerID, String trackableName, float size)
    {
        int result = QCARWrapper.Instance.MarkerTrackerCreateMarker(markerID, trackableName, size);

        return result;
    }

    #endregion // PRIVATE_METHODS
}