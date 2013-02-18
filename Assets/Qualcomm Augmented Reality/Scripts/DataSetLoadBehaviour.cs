/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This behaviour allows to automatically load and activate one or more DataSet on startup
/// </summary>
public class DataSetLoadBehaviour : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    public List<string> mDataSetsToActivate = new List<string>();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    public List<string> mDataSetsToLoad = new List<string>();

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Awake()
    {
        if (!QCARRuntimeUtilities.IsQCAREnabled())
        {
            return;
        }

        if (QCARRuntimeUtilities.IsPlayMode())
        {
            // initialize QCAR 
            QCARUnity.CheckInitializationError();
        }

        if (TrackerManager.Instance.GetTracker(Tracker.Type.IMAGE_TRACKER) == null)
        {
            TrackerManager.Instance.InitTracker(Tracker.Type.IMAGE_TRACKER);
        }

        if (mDataSetsToLoad.Count <= 0)
        {
            Debug.LogWarning("No data sets defined. Not loading any data sets.");
            return;
        }

        foreach (string dataSetName in mDataSetsToLoad)
        {
            if (!DataSet.Exists(dataSetName))
            {
                Debug.LogError("Data set " + dataSetName + " does not exist.");
                continue;
            }

            ImageTracker imageTracker = (ImageTracker)TrackerManager.Instance.GetTracker(Tracker.Type.IMAGE_TRACKER);
            DataSet dataSet = imageTracker.CreateDataSet();

            if (!dataSet.Load(dataSetName))
            {
                Debug.LogError("Failed to load data set " + dataSetName + ".");
                continue;
            }

            // Activate the data set if it is the one specified in the editor.
            if (mDataSetsToActivate.Contains(dataSetName))
            {
                imageTracker.ActivateDataSet(dataSet);
            }
        }
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

}
