/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ImageTrackerImpl : ImageTracker
{
    #region PRIVATE_MEMBER_VARIABLES

    private List<DataSetImpl> mActiveDataSets = new List<DataSetImpl>();
    private List<DataSet> mDataSets = new List<DataSet>();
    private ImageTargetBuilder mImageTargetBuilder;
    private TargetFinder mTargetFinder;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    // exposes the ImageTargetBuilder member to other classes
    public override ImageTargetBuilder ImageTargetBuilder
    {
        get { return mImageTargetBuilder; }
    }

    // exposes the ImageTargetBuilder member to other classes
    public override TargetFinder TargetFinder
    {
        get { return mTargetFinder; }
    }

    #endregion // PROPERTIES
    


    #region CONSTRUCTION

    public ImageTrackerImpl()
    {
        mImageTargetBuilder = new ImageTargetBuilderImpl();
        mTargetFinder = new TargetFinderImpl();
    }

    #endregion // CONSTRUCITON



    #region PUBLIC_METHODS

    // Starts the tracker.
    // The tracker must have loaded a dataset before it can start.
    // The tracker needs to be stopped before Trackables can be modified.
    public override bool Start()
    {
        if (QCARWrapper.Instance.ImageTrackerStart() == 0)
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
        QCARWrapper.Instance.ImageTrackerStop();

        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();

        // If a dataset is active, than mark all trackables as not found:
        foreach (DataSetImpl activeDataSet in mActiveDataSets)
        {
            foreach(Trackable trackable in activeDataSet.GetTrackables())
            {
                stateManager.SetTrackableBehavioursForTrackableToNotFound(trackable);
            }
        }
    }


    // Creates a new empty dataset.
    public override DataSet CreateDataSet()
    {
        IntPtr dataSetPtr = QCARWrapper.Instance.ImageTrackerCreateDataSet();
        if (dataSetPtr == IntPtr.Zero)
        {
            Debug.LogError("Could not create dataset.");
            return null;
        }

        DataSet dataSet = new DataSetImpl(dataSetPtr);
        mDataSets.Add(dataSet);

        return dataSet;
    }


    // Destroy the given dataset.
    // Returns false if the given dataset is active.
    public override bool DestroyDataSet(DataSet dataSet, bool destroyTrackables)
    {
        if (dataSet == null)
        {
            Debug.LogError("Dataset is null.");
            return false;
        }

        if (destroyTrackables)
        {
            dataSet.DestroyAllTrackables(true);
        }

        DataSetImpl dataSetImpl = (DataSetImpl) dataSet;
        if (QCARWrapper.Instance.ImageTrackerDestroyDataSet(dataSetImpl.DataSetPtr) == 0)
        {
            Debug.LogError("Could not destroy dataset.");
            return false;
        }

        mDataSets.Remove(dataSet);

        return true;
    }


    // Activates the given dataset.
    // Datasets can only be activated when the tracker is not running.
    public override bool ActivateDataSet(DataSet dataSet)
    {
        if (dataSet == null)
        {
            Debug.LogError("Dataset is null.");
            return false;
        }

        DataSetImpl dataSetImpl = (DataSetImpl)dataSet;
        if (QCARWrapper.Instance.ImageTrackerActivateDataSet(dataSetImpl.DataSetPtr) == 0)
        {
            Debug.LogError("Could not activate dataset.");
            return false;
        }

        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();

        // Activate all Trackables.
        foreach(Trackable trackable in dataSetImpl.GetTrackables())
            stateManager.EnableTrackableBehavioursForTrackable(trackable, true);

        mActiveDataSets.Add(dataSetImpl);
        return true;
    }


    // Deactivates the given dataset.
    // This can only be done when the tracker is not running.
    public override bool DeactivateDataSet(DataSet dataSet)
    {
        if (dataSet == null)
        {
            Debug.LogError("Dataset is null.");
            return false;
        }

        DataSetImpl dataSetImpl = (DataSetImpl)dataSet;
        if (QCARWrapper.Instance.ImageTrackerDeactivateDataSet(dataSetImpl.DataSetPtr) == 0)
        {
            Debug.LogError("Could not deactivate dataset.");
            return false;
        }

        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();

        // Deactivate all Trackables.
        foreach (Trackable trackable in dataSet.GetTrackables())
            stateManager.EnableTrackableBehavioursForTrackable(trackable, false);

        mActiveDataSets.Remove(dataSetImpl);
        return true;
    }

    // Returns the currently activated datasets. 
    public override IEnumerable<DataSet> GetActiveDataSets()
    {
        return mActiveDataSets.Cast<DataSet>();
    }

    // Returns all datasets.
    public override IEnumerable<DataSet> GetDataSets()
    {
        return mDataSets;
    }


    // Deactivates the currently active dataset and
    // destroys all datasets
    public override void DestroyAllDataSets(bool destroyTrackables)
    {
        // do not deactivate while iterating over the same list:
        List<DataSetImpl> tmpActiveDatasets = new List<DataSetImpl>(mActiveDataSets);
        foreach (DataSetImpl dataSet in tmpActiveDatasets)
        {
            DeactivateDataSet(dataSet);
        }

        for (int i = mDataSets.Count - 1; i >= 0; i--)
        {
            DestroyDataSet(mDataSets[i], destroyTrackables);
        }

        mDataSets.Clear();
    }


    #endregion // PUBLIC_METHODS
}