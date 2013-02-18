/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

// The editor interface for all DataSetTrackableBehaviours
// to be implemented explicitly by behaviours

using UnityEngine;

public interface IEditorDataSetTrackableBehaviour:IEditorTrackableBehaviour
{
    #region EDITOR_INTERFACE

    string DataSetName { get; }
    string DataSetPath { get; }
    bool SetDataSetPath(string dataSetPath);

    #endregion // EDITOR_INTERFACE
}