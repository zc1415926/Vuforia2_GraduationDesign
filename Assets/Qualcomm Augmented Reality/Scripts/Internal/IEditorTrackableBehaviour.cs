/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

// The editor interface for TrackableBehaviours
// to be implemented explicitly by behaviours

using UnityEngine;

public interface IEditorTrackableBehaviour
{
    #region EDITOR_INTERFACE

    string TrackableName { get; }
    bool CorrectScale();
    bool SetNameForTrackable(string name);
    Trackable Trackable { get; }

    Vector3 PreviousScale { get; }
    bool SetPreviousScale(Vector3 previousScale);
    bool PreserveChildSize { get; }
    bool SetPreserveChildSize(bool preserveChildSize);
    bool InitializedInEditor { get; }
    bool SetInitializedInEditor(bool initializedInEditor);
    void UnregisterTrackable();

    #endregion // EDITOR_INTERFACE



    #region UNITY_INTERFACE

    bool enabled { get; set; }
    Transform transform { get; }
    GameObject gameObject { get; }
    Renderer renderer { get; }

    #endregion // UNITY_INTERFACE
}