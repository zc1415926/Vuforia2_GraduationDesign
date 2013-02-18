/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (SetBGCameraLayerBehaviour))]
public class SetBGCameraLayerEditor : Editor
{
    #region UNITY_EDITOR_METHODS

    public override void OnInspectorGUI()
    {
        SetBGCameraLayerBehaviour bgLayerBehaviour = (SetBGCameraLayerBehaviour)target;

        EditorGUILayout.HelpBox("Here you can enter the index of the layer that will be used as the layer for background " +
                                "that is rendered for this camera. \nThe ARCamera will also be configured to not draw this layer.", 
                                MessageType.None);

        bgLayerBehaviour.CameraLayer = EditorGUILayout.IntField("Render Texture Layer", bgLayerBehaviour.CameraLayer);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(bgLayerBehaviour);

            if (QCARUtilities.GetPrefabType(bgLayerBehaviour) != PrefabType.Prefab)
                SceneManager.Instance.SceneUpdated();
        }
    }

    #endregion // UNITY_EDITOR_METHODS
}