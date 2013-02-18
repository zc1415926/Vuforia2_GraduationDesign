/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;

/// <summary>
/// The ImageTracker encapsulates methods to manage DataSets and provides access to
/// the ImageTargetBuilder and TargetFinder classes
/// </summary>
public abstract class ImageTracker : Tracker
{
    #region PROPERTIES

    /// <summary>
    /// exposes the ImageTargetBuilder member to other classes
    /// </summary>
    public abstract ImageTargetBuilder ImageTargetBuilder { get; }

    /// <summary>
    /// exposes the ImageTargetBuilder member to other classes
    /// </summary>
    public abstract TargetFinder TargetFinder { get; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Creates a new empty dataset.
    /// </summary>
    public abstract DataSet CreateDataSet();


    /// <summary>
    /// Destroy the given dataset.
    /// Returns false if the given dataset is active.
    /// </summary>
    public abstract bool DestroyDataSet(DataSet dataSet, bool destroyTrackables);


    /// <summary>
    /// Activates the given dataset.
    /// Datasets can only be activated when the tracker is not running.
    /// </summary>
    public abstract bool ActivateDataSet(DataSet dataSet);


    /// <summary>
    /// Deactivates the given dataset.
    /// This can only be done when the tracker is not running.
    /// </summary>
    public abstract bool DeactivateDataSet(DataSet dataSet);


    /// <summary>
    /// Returns the currently activated datasets. 
    /// </summary>
    public abstract IEnumerable<DataSet> GetActiveDataSets();


    /// <summary>
    /// Returns all datasets.
    /// </summary>
    public abstract IEnumerable<DataSet> GetDataSets();


    /// <summary>
    /// Deactivates the currently active dataset and
    /// destroys all datasets
    /// </summary>
    public abstract void DestroyAllDataSets(bool destroyTrackables);

    #endregion // PUBLIC_METHODS
}