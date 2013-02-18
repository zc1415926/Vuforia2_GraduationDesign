/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UserDefinedTargetBuildingBehaviour))]
public class UserDefinedTargetBuilderEditor : Editor
{
    #region UNITY_EDITOR_METHODS

    // Draws a custom UI for the UserDefinedTargetBehaviour inspector
    public override void OnInspectorGUI()
    {
        UserDefinedTargetBuildingBehaviour udtb = (UserDefinedTargetBuildingBehaviour)target;

        EditorGUIUtility.LookLikeControls();
        EditorGUILayout.HelpBox("If this is enabled, the Target Builder will begin to automatically scan the " +
                                "frame for feature points on startup.", MessageType.None);
        EditorGUIUtility.LookLikeInspector();
        udtb.StartScanningAutomatically = EditorGUILayout.Toggle("Start scanning automatically", udtb.StartScanningAutomatically);

        EditorGUIUtility.LookLikeControls();
        EditorGUILayout.HelpBox("Check this if you want to automatically disable the ImageTracker while the Target Builder is scanning. " +
                                "Once scanning mode is stopped, the ImageTracker will be enabled again.", MessageType.None);
        EditorGUIUtility.LookLikeInspector();
        udtb.StopTrackerWhileScanning = EditorGUILayout.Toggle("Stop tracker while scanning", udtb.StopTrackerWhileScanning);

        EditorGUIUtility.LookLikeControls();
        EditorGUILayout.HelpBox("If this is enabled, scanning will be automatically stopped when a new target has been created.", MessageType.None);
        EditorGUIUtility.LookLikeInspector();
        udtb.StopScanningWhenFinshedBuilding = EditorGUILayout.Toggle("Stop scanning after creating target", udtb.StopScanningWhenFinshedBuilding);

        if (GUI.changed)
            EditorUtility.SetDirty(udtb);
    }

    // Renders a label to visualize the UserDefinedTargetBehaviour
    public void OnSceneGUI()
    {
        UserDefinedTargetBuildingBehaviour udtb = (UserDefinedTargetBuildingBehaviour)target;
        GUIStyle guiStyle = new GUIStyle { alignment = TextAnchor.LowerRight, fontSize = 18, normal = { textColor = Color.white } };
        Handles.Label(udtb.transform.position, "User Defined\n      Target Builder", guiStyle);
    }

    #endregion // UNITY_EDITOR_METHODS
}