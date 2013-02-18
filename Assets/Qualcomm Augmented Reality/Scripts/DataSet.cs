/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
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

/// <summary>
/// This class represents a dataset that can be loaded and holds a collection of trackables.
/// Trackables can also be created and destroyed at runtime.
/// </summary>
public abstract class DataSet
{
    #region NESTED

    /// <summary>
    /// Storage type is used to interpret a given path string.
    /// </summary>
    public enum StorageType
    {
        STORAGE_APP, 
        STORAGE_APPRESOURCE,
        STORAGE_ABSOLUTE
    }

    #endregion // NESTED



    #region PROPERTIES

    /// <summary>
    /// Returns the path to the data set.
    /// </summary>
    public abstract string Path { get; }

    /// <summary>
    /// Returns the storage type of the data set.
    /// </summary>
    public abstract StorageType FileStorageType { get; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Checks if a data set exists at the default "StreamingAssets/QCAR" directory.
    /// </summary>
    public static bool Exists(String name)
    {
        String path = "QCAR/" + name + ".xml";
        return Exists(path, StorageType.STORAGE_APPRESOURCE);
    }


    /// <summary>
    /// Checks if a data set exists at the given path.
    /// Storage type is used to correctly interpret the given path.
    /// </summary>
    public static bool Exists(String path, StorageType storageType)
    {
        return DataSetImpl.ExistsImpl(path, storageType);
    }

    
    /// <summary>
    /// Loads a data set from the default "StreamingAssets/QCAR" directory.
    /// </summary>
    public abstract bool Load(String name);


    /// <summary>
    /// Loads data set from the given path.
    /// Storage type is used to correctly interpret the given path.
    /// </summary>
    public abstract bool Load(String path, StorageType storageType);


    /// <summary>
    /// Returns the trackables that are defined in the data set.
    /// </summary>
    public abstract IEnumerable<Trackable> GetTrackables();


    /// <summary>
    /// Creates a new trackable behaviour attached to a new gameobject with the given name and adds it to this dataset
    /// </summary>
    public abstract DataSetTrackableBehaviour CreateTrackable(TrackableSource trackableSource, string gameObjectName);


    /// <summary>
    /// Adds a single trackable from a source and a given gameobject to this dataset
    /// </summary>
    public abstract DataSetTrackableBehaviour CreateTrackable(TrackableSource trackableSource, GameObject gameObject);
    

    /// <summary>
    /// This method must not be called while the dataset is active or it will return false.
    /// </summary>
    public abstract bool Destroy(Trackable trackable, bool destroyGameObject);


    /// <summary>
    /// Checks if this DataSet's Trackable capacity is reached.
    /// Returns true if the number of Trackables created in this DataSet
    /// has reached the maximum capacity, false otherwise.
    /// </summary>
    public abstract bool HasReachedTrackableLimit();


    /// <summary>
    /// Checks if the given trackable is contained in the DataSet
    /// </summary>
    public abstract bool Contains(Trackable trackable);


    /// <summary>
    /// Destroys all existing trackables
    /// </summary>
    public abstract void DestroyAllTrackables(bool destroyGameObject);

    #endregion // PUBLIC_METHODS
}