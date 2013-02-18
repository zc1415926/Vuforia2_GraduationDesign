/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataSetImpl : DataSet
{
    #region PROPERTIES

    // Returns a data set instance.
    public IntPtr DataSetPtr
    {
        get { return mDataSetPtr; }
    }

    // Returns the path to the data set.
    public override string Path
    {
        get { return mPath; }
    }

    // Returns the storage type of the data set.
    public override StorageType FileStorageType
    {
        get { return mStorageType; }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    // Pointer stores address of a native DataSet instance.
    private IntPtr mDataSetPtr = IntPtr.Zero;
    // Path to the data set file on device storage.
    private string mPath = "";
    // Storage type of the data set file.
    private DataSet.StorageType mStorageType = DataSet.StorageType.STORAGE_APPRESOURCE;
    // Dictionary that contains Trackables that belong to this data set.
    private readonly Dictionary<int, Trackable> mTrackablesDict = new Dictionary<int, Trackable>();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    // Constructor allows to set native pointer.
    public DataSetImpl(IntPtr dataSetPtr)
    {
        mDataSetPtr = dataSetPtr;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS


    // Checks if a data set exists at the given path.
    // Storage type is used to correctly interpret the given path.
    public static bool ExistsImpl(String path, StorageType storageType)
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            path = "Assets/StreamingAssets/" + path;
        }

        return (QCARWrapper.Instance.DataSetExists(path, (int)storageType) == 1);
    }


    // Loads a data set from the default "StreamingAssets/QCAR" directory.
    public override bool Load(String name)
    {
        String path = "QCAR/" + name + ".xml"; 

        return Load(path, StorageType.STORAGE_APPRESOURCE);
    }


    // Loads data set from the given path.
    // Storage type is used to correctly interpret the given path.
    public override bool Load(String path, StorageType storageType)
    {
        if (mDataSetPtr == IntPtr.Zero)
        {
            Debug.LogError("Called Load without a data set object");
            return false;
        }

        // copied on purpose because original value is needed later on
        String actualPath = path;
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            actualPath = "Assets/StreamingAssets/" + actualPath;
        }

        if (QCARWrapper.Instance.DataSetLoad(actualPath, (int)storageType, mDataSetPtr) == 0)
        {
            Debug.LogError("Did not load: " + path);
            return false;
        }

        // Set path and storage type to associate data sets with Trackables.
        mPath = path;
        mStorageType = storageType;


        // Create Trackabls in this dataset.
        CreateImageTargets();
        CreateMultiTargets();

        // associate existing TrackableBehaviours with the trackables in this dataset and create missing behaviours:
        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
        stateManager.AssociateTrackableBehavioursForDataSet(this);

        return true;
    }

    // Returns the trackables that are defined in the data set.
    public override IEnumerable<Trackable> GetTrackables()
    {
        return mTrackablesDict.Values;
    }

    // creates a new trackable behaviour attached to a fresh gameobject with the given name and adds it to this dataset
    public override DataSetTrackableBehaviour CreateTrackable(TrackableSource trackableSource, string gameObjectName)
    {
        GameObject gameObject = new GameObject(gameObjectName);
        return CreateTrackable(trackableSource, gameObject);
    }

    // Adds a single trackable from a source and a given gameobject to this dataset
    public override DataSetTrackableBehaviour CreateTrackable(TrackableSource trackableSource, GameObject gameObject)
    {
        // create a trackable in native from the trackable source handle:
        TrackableSourceImpl trackableSourceImpl = (TrackableSourceImpl)trackableSource;

        // QCAR support names up to 64 characters in length, but here we allocate 
        // a slightly larger buffer:
        int nameLength = 128;
        System.Text.StringBuilder trackableName = new System.Text.StringBuilder(nameLength);

        IntPtr targetDataPtr =
            Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SimpleTargetData)));
        TrackableType trackableType = (TrackableType)QCARWrapper.Instance.DataSetCreateTrackable(mDataSetPtr, trackableSourceImpl.TrackableSourcePtr, trackableName, nameLength, targetDataPtr);

        SimpleTargetData targetData = (SimpleTargetData)
                Marshal.PtrToStructure(targetDataPtr, typeof(SimpleTargetData));

        Marshal.FreeHGlobal(targetDataPtr);
        
        // currently only supported for ImageTargets:
        if (trackableType == TrackableType.IMAGE_TARGET)
        {
            ImageTarget newImageTarget = new ImageTargetImpl(trackableName.ToString(), targetData.id, ImageTargetType.USER_DEFINED, this);

            // Add newly created Image Target to dictionary.
            mTrackablesDict[targetData.id] = newImageTarget;

            Debug.Log(string.Format("Trackable created: {0}, {1}", trackableType, trackableName));

            // Find or create ImageTargetBehaviour for this ImageTarget:
            StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
            return stateManager.FindOrCreateImageTargetBehaviourForTrackable(newImageTarget, gameObject, this);
        }
        else
        {
            Debug.LogError("DataSet.CreateTrackable returned unknown or incompatible trackable type!");
            return null;
        }
    }

    /// <summary>
    /// This method must not be called while the dataset is active or it will return false.
    /// </summary>
    public override bool Destroy(Trackable trackable, bool destroyGameObject)
    {
        if (QCARWrapper.Instance.DataSetDestroyTrackable(mDataSetPtr, trackable.ID) == 0)
        {
            Debug.LogError("Could not destroy trackable with id " + trackable.ID + ".");
            return false;
        }

        mTrackablesDict.Remove(trackable.ID);

        if (destroyGameObject)
        {
            StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
            stateManager.DestroyTrackableBehavioursForTrackable(trackable);
        }
        return true;
    }

    /// <summary>
    /// Checks if this DataSet's Trackable capacity is reached.
    /// Returns true if the number of Trackables created in this DataSet
    /// has reached the maximum capacity, false otherwise.
    /// </summary>
    public override bool HasReachedTrackableLimit()
    {
        return (QCARWrapper.Instance.DataSetHasReachedTrackableLimit(mDataSetPtr) == 1);
    }


    /// <summary>
    /// Checks if the given trackable is contained in the DataSet
    /// </summary>
    public override bool Contains(Trackable trackable)
    {
        return mTrackablesDict.ContainsValue(trackable);
    }


    /// <summary>
    /// Destroys all existing trackables
    /// </summary>
    public override void DestroyAllTrackables(bool destroyGameObject)
    {
        List<Trackable> trackablesToDelete = new List<Trackable>(mTrackablesDict.Values);

        foreach (Trackable trackable in trackablesToDelete)
        {
            Destroy(trackable, destroyGameObject);
        }
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS


    private void CreateImageTargets()
    {
        // Allocate array for all Image Targets.
        int numImageTargets = QCARWrapper.Instance.DataSetGetNumTrackableType(
            (int)TrackableType.IMAGE_TARGET,
            mDataSetPtr);
        IntPtr imageTargetDataPtr =
            Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ImageTargetData)) * numImageTargets);

        // Copy Image Target properties from native.
        if (QCARWrapper.Instance.DataSetGetTrackablesOfType((int)TrackableType.IMAGE_TARGET,
                                       imageTargetDataPtr, numImageTargets,
                                       mDataSetPtr) == 0)
        {
            Debug.LogError("Could not create Image Targets");
            return;
        }

        // Create Image Target Behaviours.
        for (int i = 0; i < numImageTargets; ++i)
        {
            IntPtr trackablePtr = new IntPtr(imageTargetDataPtr.ToInt32() + i *
                    Marshal.SizeOf(typeof(ImageTargetData)));
            ImageTargetData trackableData = (ImageTargetData)
                    Marshal.PtrToStructure(trackablePtr, typeof(ImageTargetData));

            // Do not overwrite existing Trackables.
            if (mTrackablesDict.ContainsKey(trackableData.id))
            {
                continue;
            }

            // QCAR support names up to 64 characters in length, but here we allocate 
            // a slightly larger buffer:
            int nameLength = 128;
            System.Text.StringBuilder trackableName = new System.Text.StringBuilder(nameLength);
            QCARWrapper.Instance.DataSetGetTrackableName(mDataSetPtr, trackableData.id, trackableName, nameLength);

            ImageTarget imageTarget = new ImageTargetImpl(trackableName.ToString(), trackableData.id, ImageTargetType.PREDEFINED, this);

            // Add newly created Image Target to dictionary.
            mTrackablesDict[trackableData.id] = imageTarget;
        }

        Marshal.FreeHGlobal(imageTargetDataPtr);
    }


    private void CreateMultiTargets()
    {
        // Allocate array for all Multi Targets.
        int numMultiTargets = QCARWrapper.Instance.DataSetGetNumTrackableType(
            (int)TrackableType.MULTI_TARGET,
            mDataSetPtr);
        IntPtr multiTargetDataPtr =
            Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SimpleTargetData)) * numMultiTargets);

        // Copy Multi Target properties from native.
        if (QCARWrapper.Instance.DataSetGetTrackablesOfType((int)TrackableType.MULTI_TARGET,
                                       multiTargetDataPtr, numMultiTargets,
                                       mDataSetPtr) == 0)
        {
            Debug.LogError("Could not create Multi Targets");
            return;
        }

        // Create Multi Target Behaviours.
        for (int i = 0; i < numMultiTargets; ++i)
        {
            IntPtr trackablePtr = new IntPtr(multiTargetDataPtr.ToInt32() + i *
                    Marshal.SizeOf(typeof(SimpleTargetData)));
            SimpleTargetData trackableData = (SimpleTargetData)
                    Marshal.PtrToStructure(trackablePtr, typeof(SimpleTargetData));

            // Do not overwrite existing Trackables.
            if (mTrackablesDict.ContainsKey(trackableData.id))
            {
                continue;
            }

            // QCAR support names up to 64 characters in length, but here we allocate 
            // a slightly larger buffer:
            int nameLength = 128;
            System.Text.StringBuilder trackableName = new System.Text.StringBuilder(nameLength);
            QCARWrapper.Instance.DataSetGetTrackableName(mDataSetPtr, trackableData.id, trackableName, nameLength);

            MultiTarget multiTarget = new MultiTargetImpl(trackableName.ToString(), trackableData.id);

            // Add newly created Multi Target to dictionary.
            mTrackablesDict[trackableData.id] = multiTarget;
        }

        Marshal.FreeHGlobal(multiTargetDataPtr);
    }

    #endregion // PRIVATE_METHODS
}