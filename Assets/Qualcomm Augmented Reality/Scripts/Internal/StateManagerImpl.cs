/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// This class is used to manage the state of all TrackableBehaviours, create them,
/// associate them with Trackables, update their pose, etc.
/// </summary>
public class StateManagerImpl : StateManager
{
    #region PRIVATE_MEMBER_VARIABLES

    private readonly Dictionary<int, TrackableBehaviour> mTrackableBehaviours = new Dictionary<int, TrackableBehaviour>();

    private readonly List<TrackableBehaviour> mActiveTrackableBehaviours = new List<TrackableBehaviour>();

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the TrackableBehaviours currently being tracked
    /// </summary>
    public override IEnumerable<TrackableBehaviour> GetActiveTrackableBehaviours()
    {
        return mActiveTrackableBehaviours;
    }

    /// <summary>
    /// Returns all currently instantiated TrackableBehaviours
    /// </summary>
    public override IEnumerable<TrackableBehaviour> GetTrackableBehaviours()
    {
        return mTrackableBehaviours.Values;
    }


    #region INTERNAL_METHODS - rethink what should be exposed publicly!!

    /// <summary>
    /// Finds all MarkerBehaviours in the scene and associates them with existing Markers
    /// If a Marker does not exist yet, it is created in native.
    /// </summary>
    public void AssociateMarkerBehaviours()
    {
        MarkerTrackerImpl markerTracker = (MarkerTrackerImpl)TrackerManager.Instance.GetTracker(Tracker.Type.MARKER_TRACKER);
        if (markerTracker != null)
        {
            // go over all MarkerBehaviours in scene:
            MarkerBehaviour[] markerBehaviours = (MarkerBehaviour[]) Object.FindObjectsOfType(typeof (MarkerBehaviour));

            foreach (MarkerBehaviour markerBehaviour in markerBehaviours)
            {
                // check if the marker has been created in native yet:
                IEditorMarkerBehaviour editorMarkerBehaviour = markerBehaviour;
                Marker marker = markerTracker.GetMarkerByMarkerID(editorMarkerBehaviour.MarkerID);
                if (marker != null)
                {
                    // if the Marker has already been created, initialize the Behaviour:
                    InitializeMarkerBehaviour(markerBehaviour, marker);
                }
                else
                {
                    // otherwise, create it first:
                    marker = markerTracker.InternalCreateMarker(editorMarkerBehaviour.MarkerID,
                                                                editorMarkerBehaviour.TrackableName,
                                                                editorMarkerBehaviour.transform.localScale.x);

                    if (marker == null)
                    {
                        Debug.LogWarning("Disabling MarkerBehaviour named " + editorMarkerBehaviour.TrackableName);
                        markerBehaviour.enabled = false;
                    }
                    else
                    {
                        InitializeMarkerBehaviour(markerBehaviour, marker);
                        markerBehaviour.enabled = true;
                    }
                }
            }
        }
    }


    /// <summary>
    /// Finds DataSetTrackableBehaviours for this dataset and associates them with the Trackables in the DataSet.
    /// VirtualButtonBehaviours created in the scene are associated with the VirtualButtons in the DataSet or created there.
    /// 
    /// If there is a Trackable in the DataSet where no TrackableBehaviour exists yet, this Behaviour is created, together with its VirtualButtons.
    /// </summary>
    public void AssociateTrackableBehavioursForDataSet(DataSet dataSet)
    {

        // Step: Add all TrackableBehaviours that belong to this data set and
        // are already instantiated in the scene to the dictionary.
        DataSetTrackableBehaviour[] trackableBehaviours = (DataSetTrackableBehaviour[])
            Object.FindObjectsOfType(typeof(DataSetTrackableBehaviour));

        // Initialize all Image Targets
        foreach (DataSetTrackableBehaviour trackableBehaviour in trackableBehaviours)
        {
            IEditorDataSetTrackableBehaviour editorTrackableBehaviour = trackableBehaviour;
            if (editorTrackableBehaviour.TrackableName == null)
            {
                Debug.LogError("Found Trackable without name.");
                continue;
            }

            // check if the TrackableBehaviour references this DataSet
            if (editorTrackableBehaviour.DataSetPath.Equals(dataSet.Path))
            {
                bool matchFound = false;

                // find the trackable to be associated with this TrackableBehaviour:
                foreach(Trackable trackable in dataSet.GetTrackables())
                {
                    if (trackable.Name.Equals(editorTrackableBehaviour.TrackableName))
                    {
                        if (trackableBehaviour is ImageTargetBehaviour &&
                            trackable is ImageTarget)
                        {
                            IEditorImageTargetBehaviour editorImageTargetBehaviour = (ImageTargetBehaviour)trackableBehaviour;

                            matchFound = true;

                            editorImageTargetBehaviour.InitializeImageTarget((ImageTarget)trackable);
                            mTrackableBehaviours[trackable.ID] = trackableBehaviour;
                            Debug.Log("Found Trackable named " + trackableBehaviour.Trackable.Name +
                                        " with id " + trackableBehaviour.Trackable.ID);
                        }
                        else if (trackableBehaviour is MultiTargetBehaviour &&
                                 trackable is MultiTarget)
                        {
                            matchFound = true;

                            IEditorMultiTargetBehaviour editorMultiTargetBehaviour = (MultiTargetBehaviour)trackableBehaviour;
                            editorMultiTargetBehaviour.InitializeMultiTarget((MultiTarget)trackable);
                            mTrackableBehaviours[trackable.ID] = trackableBehaviour;
                            Debug.Log("Found Trackable named " + trackableBehaviour.Trackable.Name +
                                        " with id " + trackableBehaviour.Trackable.ID);
                        }
                    }
                }

                if (!matchFound)
                {
                    Debug.LogError("Could not associate DataSetTrackableBehaviour '" + editorTrackableBehaviour.TrackableName +
                        "' - no matching Trackable found in DataSet!");
                }
            }
        }


        // Step 2: Add all VirtualButtonBehaviours that belong to this data set
        // and are already instantiated in the scene to the dictionary.
        VirtualButtonBehaviour[] vbBehaviours = (VirtualButtonBehaviour[])
            Object.FindObjectsOfType(typeof(VirtualButtonBehaviour));
        AssociateVirtualButtonBehaviours(vbBehaviours, dataSet);

        // Step 3: Create TrackableBehaviours that are not existing in the scene.
        CreateMissingDataSetTrackableBehaviours(dataSet);
    }

    /// <summary>
    /// Clears the TrackableBehaviour dictionaries - called when QCARBehaviour is destroyed.
    /// </summary>
    public void ClearTrackableBehaviours()
    {
        mTrackableBehaviours.Clear();
        mActiveTrackableBehaviours.Clear();
    }


    /// <summary>
    /// Takes a given GameObject to add a new ImageTargetBehaviour to. This new Behaviour is associated with the given ImageTarget
    /// </summary>
    public ImageTargetBehaviour FindOrCreateImageTargetBehaviourForTrackable(ImageTarget trackable, GameObject gameObject)
    {
        return FindOrCreateImageTargetBehaviourForTrackable(trackable, gameObject, null);
    }


    /// <summary>
    /// Takes a given GameObject to add a new ImageTargetBehaviour to. This new Behaviour is associated with the given ImageTarget
    /// </summary>
    public ImageTargetBehaviour FindOrCreateImageTargetBehaviourForTrackable(ImageTarget trackable, GameObject gameObject, DataSet dataSet)
    {
        DataSetTrackableBehaviour trackableBehaviour = gameObject.GetComponent<DataSetTrackableBehaviour>();

        // add an ImageTargetBehaviour if none is attached yet
        if (trackableBehaviour == null)
        {
            trackableBehaviour = gameObject.AddComponent<ImageTargetBehaviour>();
            ((IEditorTrackableBehaviour)trackableBehaviour).SetInitializedInEditor(true);
        }

        // configure the new ImageTargetBehaviour instance:
        if (!(trackableBehaviour is ImageTargetBehaviour))
        {
            Debug.LogError(
                string.Format("DataSet.CreateTrackable: Trackable of type ImageTarget was created, but behaviour of type {0} was provided!",
                                trackableBehaviour.GetType()));
            return null;
        }

        IEditorImageTargetBehaviour editorImgTargetBehaviour = (ImageTargetBehaviour)trackableBehaviour;
        if (dataSet != null) editorImgTargetBehaviour.SetDataSetPath(dataSet.Path);
        editorImgTargetBehaviour.SetImageTargetType(trackable.ImageTargetType);
        editorImgTargetBehaviour.SetNameForTrackable(trackable.Name);
        editorImgTargetBehaviour.InitializeImageTarget(trackable);

        mTrackableBehaviours[trackable.ID] = trackableBehaviour;

        return trackableBehaviour as ImageTargetBehaviour;
    }


    /// <summary>
    /// Creates a new new, empty MarkerBehaviour for the given Marker
    /// </summary>
    public MarkerBehaviour CreateNewMarkerBehaviourForMarker(Marker trackable, string gameObjectName)
    {
        // Alternatively instantiate Trackable Prefabs.
        GameObject markerObject = new GameObject(gameObjectName);
        return CreateNewMarkerBehaviourForMarker(trackable, markerObject);
    }


    /// <summary>
    /// Takes a given GameObject to add a new MarkerBehaviour to. This new Behaviour is associated with the given Marker
    /// </summary>
    public MarkerBehaviour CreateNewMarkerBehaviourForMarker(Marker trackable, GameObject gameObject)
    {
        MarkerBehaviour newMB =
            gameObject.AddComponent<MarkerBehaviour>();

        float markerSize = trackable.GetSize();

        Debug.Log("Creating Marker with values: " +
                  "\n MarkerID:     " + trackable.MarkerID +
                  "\n TrackableID:  " + trackable.ID +
                  "\n Name:         " + trackable.Name +
                  "\n Size:         " + markerSize + "x" + markerSize);

        IEditorMarkerBehaviour newEditorMB = newMB;
        newEditorMB.SetMarkerID(trackable.MarkerID);
        newEditorMB.SetNameForTrackable(trackable.Name);
        newEditorMB.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
        newEditorMB.InitializeMarker(trackable);

        return newMB;
    }
     

    /// <summary>
    /// Destroys all the TrackableBehaviours for the given Trackable
    /// </summary>
    public void DestroyTrackableBehavioursForTrackable(Trackable trackable, bool destroyGameObjects = true)
    {
        TrackableBehaviour trackableBehaviour;
        if (mTrackableBehaviours.TryGetValue(trackable.ID, out trackableBehaviour))
        {
            mTrackableBehaviours.Remove(trackable.ID);
            if (destroyGameObjects)
                Object.Destroy(trackableBehaviour.gameObject);
            else
            {
                IEditorTrackableBehaviour editorTrackableBehaviour = trackableBehaviour;
                editorTrackableBehaviour.UnregisterTrackable();
            }
        }
    }


    /// <summary>
    /// Marks the TrackableBehaviours for the given Trackable as "not found"
    /// </summary>
    public void SetTrackableBehavioursForTrackableToNotFound(Trackable trackable)
    {
            TrackableBehaviour trackableBehaviour;
            if (mTrackableBehaviours.TryGetValue(trackable.ID, out trackableBehaviour))
            {
                trackableBehaviour.OnTrackerUpdate(TrackableBehaviour.Status.NOT_FOUND);
            }
    }


    /// <summary>
    /// Sets the enabled flag for TrackableBehaviours for a given Trackable
    /// </summary>
    public void EnableTrackableBehavioursForTrackable(Trackable trackable, bool enabled)
    {
        TrackableBehaviour trackableBehaviour;
        if (mTrackableBehaviours.TryGetValue(trackable.ID, out trackableBehaviour))
        {
            if (trackableBehaviour != null)
                trackableBehaviour.enabled = enabled;
        }
    }


    public void RemoveDisabledTrackablesFromQueue(ref LinkedList<int> trackableIDs)
    {
        LinkedListNode<int> idNode = trackableIDs.First;

        while (idNode != null)
        {
            LinkedListNode<int> next = idNode.Next;

            TrackableBehaviour trackableBehaviour;
            if (mTrackableBehaviours.TryGetValue(idNode.Value, out trackableBehaviour))
            {
                if (trackableBehaviour.enabled == false)
                {
                    trackableIDs.Remove(idNode);
                }
            }
            idNode = next;
        }
    }


    // method used to update the camera pose in the scene
    public void UpdateCameraPose(Camera arCamera,
                                     QCARManagerImpl.TrackableResultData[] trackableResultDataArray,
                                     int originTrackableID)
    {
        // If there is a World Center Trackable use it to position the camera.
        if (originTrackableID >= 0)
        {
            foreach (QCARManagerImpl.TrackableResultData trackableData in trackableResultDataArray)
            {
                if (trackableData.id == originTrackableID)
                {
                    if (trackableData.status ==
                        TrackableBehaviour.Status.DETECTED
                        || trackableData.status ==
                        TrackableBehaviour.Status.TRACKED)
                    {
                        TrackableBehaviour trackableBehaviour;
                        if (mTrackableBehaviours.TryGetValue(originTrackableID, out trackableBehaviour))
                        {
                            if (trackableBehaviour.enabled)
                            {
                                PositionCamera(trackableBehaviour, arCamera,
                                               trackableData.pose);
                            }
                        }
                    }
                    break;
                }
            }
        }
    }


    // Method used to update poses of all active Image Targets
    // in the scene
    public void UpdateTrackablePoses(Camera arCamera,
                                     QCARManagerImpl.TrackableResultData[] trackableResultDataArray,
                                     int originTrackableID)
    {
        Dictionary<int, QCARManagerImpl.TrackableResultData> trackableResults = new Dictionary<int, QCARManagerImpl.TrackableResultData>();

        foreach (QCARManagerImpl.TrackableResultData trackableData in trackableResultDataArray)
        {
            // create a dictionary of all trackableResults
            trackableResults.Add(trackableData.id, trackableData);

            // For each Trackable data struct from native
            TrackableBehaviour trackableBehaviourBehaviour;
            if (mTrackableBehaviours.TryGetValue(trackableData.id, out trackableBehaviourBehaviour))
            {
                // If this is the world center skip it, we never move the
                // world center Trackable in the scene
                if (trackableData.id == originTrackableID)
                {
                    continue;
                }

                if ((trackableData.status ==
                        TrackableBehaviour.Status.DETECTED
                        || trackableData.status ==
                        TrackableBehaviour.Status.TRACKED) &&
                        trackableBehaviourBehaviour.enabled)
                {
                    // The Trackable object is visible and enabled,
                    // move it into position in relation to the camera
                    // (which we moved earlier)
                    PositionTrackable(trackableBehaviourBehaviour, arCamera,
                                      trackableData.pose);
                }
            }
        }

        // Update each Trackable
        // Do this once all Trackables have been moved into place

        mActiveTrackableBehaviours.Clear();

        foreach (TrackableBehaviour trackableBehaviour in mTrackableBehaviours.Values)
        {
            if (trackableBehaviour.enabled)
            {
                QCARManagerImpl.TrackableResultData trackableData;
                if (trackableResults.TryGetValue(trackableBehaviour.Trackable.ID, out trackableData))
                {
                    trackableBehaviour.OnTrackerUpdate(trackableData.status);
                }
                else
                {
                    trackableBehaviour.OnTrackerUpdate(TrackableBehaviour.Status.NOT_FOUND);
                }

                if (trackableBehaviour.CurrentStatus == TrackableBehaviour.Status.TRACKED ||
                    trackableBehaviour.CurrentStatus == TrackableBehaviour.Status.DETECTED)
                {
                    mActiveTrackableBehaviours.Add(trackableBehaviour);
                }
            }
        }
    }



    // Update Virtual Button states.
    public void UpdateVirtualButtons(int numVirtualButtons, IntPtr virtualButtonPtr)
    {
        Dictionary<int, QCARManagerImpl.VirtualButtonData> vbResults = new Dictionary<int, QCARManagerImpl.VirtualButtonData>();

        // create a dictionary of all results
        for (int i = 0; i < numVirtualButtons; i++)
        {
            IntPtr vbPtr = new IntPtr(virtualButtonPtr.ToInt32() + i*
                                      Marshal.SizeOf(typeof (QCARManagerImpl.VirtualButtonData)));
            QCARManagerImpl.VirtualButtonData vbData = (QCARManagerImpl.VirtualButtonData)
                                                       Marshal.PtrToStructure(vbPtr, typeof (QCARManagerImpl.VirtualButtonData));

            vbResults.Add(vbData.id, vbData);
        }

        List<VirtualButtonBehaviour> vbBehavioursToUpdate = new List<VirtualButtonBehaviour>();

        // go over all trackable behaviours and find the virtual buttons to update
        foreach (TrackableBehaviour trackableBehaviour in mTrackableBehaviours.Values)
        {
            ImageTargetBehaviour it = trackableBehaviour as ImageTargetBehaviour;

            if (it != null && it.enabled)
            {
                foreach(VirtualButtonBehaviour virtualButtonBehaviour in it.GetVirtualButtonBehaviours())
                {
                    if (virtualButtonBehaviour.enabled)
                    {
                        vbBehavioursToUpdate.Add(virtualButtonBehaviour);
                    }
                }
            }
        }

        // update the virtual buttons:
        foreach (VirtualButtonBehaviour virtualButtonBehaviour in vbBehavioursToUpdate)
        {
            QCARManagerImpl.VirtualButtonData vbData;
            if (vbResults.TryGetValue(virtualButtonBehaviour.VirtualButton.ID, out vbData))
            {
                virtualButtonBehaviour.OnTrackerUpdated(vbData.isPressed > 0);
            }
            else
            {
                virtualButtonBehaviour.OnTrackerUpdated(false);
            }
        }
    }

    #endregion // INTERNAL_METHODS

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Associates existing VirtualButtonBehaviours with VirtualButtons
    private void AssociateVirtualButtonBehaviours(VirtualButtonBehaviour[] vbBehaviours, DataSet dataSet)
    {
        for (int i = 0; i < vbBehaviours.Length; ++i)
        {
            VirtualButtonBehaviour virtualButtonBehaviour = vbBehaviours[i];

            if (virtualButtonBehaviour.VirtualButtonName == null)
            {
                Debug.LogError("VirtualButton at " + i +
                                " has no name.");
                continue;
            }

            ImageTargetBehaviour imageTargetBehaviour = virtualButtonBehaviour.GetImageTargetBehaviour();

            if (imageTargetBehaviour == null)
            {
                Debug.LogError("VirtualButton named " +
                                virtualButtonBehaviour.VirtualButtonName +
                                " is not attached to an ImageTarget.");
                continue;
            }

            // Image Target is not part of this data set.
            if (!dataSet.Contains(imageTargetBehaviour.Trackable))
            {
                continue;
            }

            ((IEditorImageTargetBehaviour)imageTargetBehaviour).AssociateExistingVirtualButtonBehaviour(virtualButtonBehaviour);
        }
    }

    private void CreateMissingDataSetTrackableBehaviours(DataSet dataSet)
    {
        foreach (Trackable trackable in dataSet.GetTrackables())
        {
            if (!mTrackableBehaviours.ContainsKey(trackable.ID))
            {
                if (trackable is ImageTarget)
                {
                    ImageTargetBehaviour itb = CreateImageTargetBehaviour((ImageTarget)trackable);

                    // Create Virtual Buttons for this Image Target.
                    ((IEditorImageTargetBehaviour)itb).CreateMissingVirtualButtonBehaviours();

                    // Add newly created Image Target to dictionary.
                    mTrackableBehaviours[trackable.ID] = itb;
                }
                else if (trackable is MultiTarget)
                {
                    MultiTargetBehaviour mtb = CreateMultiTargetBehaviour((MultiTarget)trackable);

                    // Add newly created Multi Target to dictionary.
                    mTrackableBehaviours[trackable.ID] = mtb;
                }
            }
        }
    }


    private ImageTargetBehaviour CreateImageTargetBehaviour(ImageTarget imageTarget)
    {
        GameObject imageTargetObject = new GameObject();
        ImageTargetBehaviour newITB =
            imageTargetObject.AddComponent<ImageTargetBehaviour>();

        IEditorImageTargetBehaviour newEditorITB = newITB;

        Debug.Log("Creating Image Target with values: " +
                  "\n ID:           " + imageTarget.ID +
                  "\n Name:         " + imageTarget.Name +
                  "\n Path:         " + newEditorITB.DataSetPath +
                  "\n Size:         " + imageTarget.GetSize().x + "x" + imageTarget.GetSize().y);

        // Set Image Target attributes.
        newEditorITB.SetNameForTrackable(imageTarget.Name);
        newEditorITB.SetDataSetPath(newEditorITB.DataSetPath);
        newEditorITB.transform.localScale = new Vector3(imageTarget.GetSize().x, 1.0f, imageTarget.GetSize().y);
        newEditorITB.CorrectScale();
        newEditorITB.SetAspectRatio(imageTarget.GetSize()[1] / imageTarget.GetSize()[0]);
        newEditorITB.InitializeImageTarget(imageTarget);

        return newITB;
    }


    private MultiTargetBehaviour CreateMultiTargetBehaviour(MultiTarget multiTarget)
    {
        GameObject multiTargetObject = new GameObject();
        MultiTargetBehaviour newMTB =
            multiTargetObject.AddComponent<MultiTargetBehaviour>();

        IEditorMultiTargetBehaviour newEditorMTB = newMTB;

        Debug.Log("Creating Multi Target with values: " +
          "\n ID:           " + multiTarget.ID +
          "\n Name:         " + multiTarget.Name +
          "\n Path:         " + newEditorMTB.DataSetPath);


        // Set Multi Target attributes.
        newEditorMTB.SetNameForTrackable(multiTarget.Name);
        newEditorMTB.SetDataSetPath(newEditorMTB.DataSetPath);
        newEditorMTB.InitializeMultiTarget(multiTarget);

        return newMTB;
    }

    private void InitializeMarkerBehaviour(MarkerBehaviour markerBehaviour, Marker marker)
    {
        IEditorMarkerBehaviour editorMarkerBehaviour = markerBehaviour;
        editorMarkerBehaviour.InitializeMarker(marker);
        if (!mTrackableBehaviours.ContainsKey(marker.ID))
        {
            // Add MarkerBehaviour to dictionary.
            mTrackableBehaviours[marker.ID] = markerBehaviour;

            Debug.Log("Found Marker named " + marker.Name +
                        " with id " + marker.ID);
        }
    }


    // Position the camera relative to a Trackable.
    private void PositionCamera(TrackableBehaviour trackableBehaviour,
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

    // Position a Trackable relative to the Camera.
    private void PositionTrackable(TrackableBehaviour trackableBehaviour,
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

    #endregion // PRIVATE_METHODS
}