/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


using UnityEngine;

// The editor interface for MarkerBehaviours
// to be implemented explicitly by behaviours
public interface IEditorImageTargetBehaviour : IEditorDataSetTrackableBehaviour
{
    #region EDITOR_INTERFACE

    float AspectRatio { get; }
    ImageTargetType ImageTargetType { get; }
    bool SetAspectRatio(float aspectRatio);
    bool SetImageTargetType(ImageTargetType imageTargetType);
    Vector2 GetSize();
    void InitializeImageTarget(ImageTarget imageTarget);
    void CreateMissingVirtualButtonBehaviours();
    bool TryGetVirtualButtonBehaviourByID(int id, out VirtualButtonBehaviour virtualButtonBehaviour);
    void AssociateExistingVirtualButtonBehaviour(VirtualButtonBehaviour virtualButtonBehaviour);

    #endregion // EDITOR_INTERFACE
}