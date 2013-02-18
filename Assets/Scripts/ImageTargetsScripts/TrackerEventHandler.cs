/*==============================================================================
            Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
            All Rights Reserved.
            Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

/// <summary>
///  A custom handler that implements the ITrackerEventHandler interface.
/// </summary>
public class TrackerEventHandler : MonoBehaviour,
                                   ITrackerEventHandler
{
    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        QCARBehaviour qcarBehaviour = GetComponent<QCARBehaviour>();
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region PUBLIC_METHODS

    /// <summary>
    /// Implementation of the ITrackerEventHandler function called after
    /// QCAR has been initialized completely
    /// </summary>
    public void OnInitialized()
    {
        //Debug.Log("Finished initializing");
    }

    /// <summary>
    /// Implementation of the ITrackerEventHandler function called after all
    /// trackables have changed.
    /// </summary>
    public void OnTrackablesUpdated()
    {
        //Debug.Log("trackables updated");
    }

    #endregion // PUBLIC_METHODS
}
