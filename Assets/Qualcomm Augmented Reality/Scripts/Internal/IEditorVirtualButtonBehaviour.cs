/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

// The editor interface for Virtual Buttons
// to be implemented explicitly by behaviours
public interface IEditorVirtualButtonBehaviour
{
    #region EDITOR_INTERFACE

    string VirtualButtonName { get; }
    bool SetVirtualButtonName(string virtualButtonName);
    
    VirtualButton.Sensitivity SensitivitySetting { get; }
    bool SetSensitivitySetting(VirtualButton.Sensitivity sensibility);

    Matrix4x4 PreviousTransform { get; }
    bool SetPreviousTransform(Matrix4x4 transform);

    GameObject PreviousParent { get; }
    bool SetPreviousParent(GameObject parent);

    void InitializeVirtualButton(VirtualButton virtualButton);
    bool SetPosAndScaleFromButtonArea(Vector2 topLeft, Vector2 bottomRight);

    bool UnregisterOnDestroy { get; set; }

    bool HasUpdatedPose { get; }
    bool UpdatePose();

    #endregion // EDITOR_INTERFACE



    #region UNITY_INTERFACE

    bool enabled { get; set; }
    Transform transform { get; }
    GameObject gameObject { get; }
    Renderer renderer { get; }

    #endregion // UNITY_INTERFACE
}