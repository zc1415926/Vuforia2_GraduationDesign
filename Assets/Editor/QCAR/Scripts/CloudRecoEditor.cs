/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CloudRecoBehaviour))]
public class CloudRecoEditor : Editor
{
    #region UNITY_EDITOR_METHODS
    
    // Draws a custom UI for the cloud reco behaviour inspector
    public override void OnInspectorGUI()
    {
        CloudRecoBehaviour crb = (CloudRecoBehaviour)target;

        EditorGUILayout.HelpBox("Credentials for authenticating with the CloudReco service.\n" + 
            "These are read-only access keys for accessing the image database specific to this sample application - the keys should be replaced " +
            "by your own access keys. You should be very careful how you share your credentials, especially with untrusted third parties, and should " +
            "take the appropriate steps to protect them within your application code.", MessageType.Info);
        crb.AccessKey = EditorGUILayout.TextField("Access Key", crb.AccessKey);
        crb.SecretKey = EditorGUILayout.TextField("Secret Key", crb.SecretKey);

        EditorGUILayout.HelpBox("You can use these color fields to configure the scanline UI to match the color scheme of your app.", MessageType.None);
        crb.ScanlineColor = EditorGUILayout.ColorField("Scanline", crb.ScanlineColor);
        crb.FeaturePointColor = EditorGUILayout.ColorField("Feature Points", crb.FeaturePointColor);

        if (GUI.changed)
            EditorUtility.SetDirty(crb);
    }

    // Renders a label to visualize the CloudRecoBehaviour
    public void OnSceneGUI()
    {
        CloudRecoBehaviour crb = (CloudRecoBehaviour)target;
        GUIStyle guiStyle = new GUIStyle{alignment = TextAnchor.LowerRight, fontSize = 18, normal = {textColor = Color.white}};
        Handles.Label(crb.transform.position, "Cloud\nRecognition", guiStyle);
    }

    #endregion // UNITY_EDITOR_METHODS
}