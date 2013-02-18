/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataSetLoadBehaviour))]
public class DataSetLoadEditor : Editor
{
    #region UNITY_EDITOR_METHODS

    public void OnEnable()
    {
        if (!SceneManager.Instance.SceneInitialized)
        {
            SceneManager.Instance.InitScene();
        }
    }


    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();

        DrawDefaultInspector();

        DataSetLoadBehaviour dslb = (DataSetLoadBehaviour)target;

        // If this instance is a prefab don't show anything in the inspector.
        if (QCARUtilities.GetPrefabType(dslb) == PrefabType.Prefab)
        {
            return;
        }

        // We know that the data set manager also has a default data set that we don't want to use => "num - 1".
        string[] dataSetList = new string[ConfigDataManager.Instance.NumConfigDataObjects - 1];
        // Fill list with available data sets.
        ConfigDataManager.Instance.GetConfigDataNames(dataSetList, false);

        DrawDataSets(dslb, dataSetList);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dslb);
        }
    }

    #endregion // UNITY_EDITOR_METHODS


    #region PUBLIC_METHODS

    // Called by the Scene Manager to notify that the list of data sets may have changed
    public static void OnConfigDataChanged()
    {
        // List of config data objects minus the default object:
        string[] dataSetList = new string[ConfigDataManager.Instance.NumConfigDataObjects - 1];
        ConfigDataManager.Instance.GetConfigDataNames(dataSetList, false);

        DataSetLoadBehaviour[] dslbs = (DataSetLoadBehaviour[])UnityEngine.Object.FindObjectsOfType(
                                            typeof(DataSetLoadBehaviour));

        foreach (DataSetLoadBehaviour dslb in dslbs)
        {
            // Clear any datasets to activate if they no longer exists:
            dslb.mDataSetsToActivate.RemoveAll(s => (System.Array.Find(
                dataSetList, str => str.Equals(s)) == null));

            // Clear any datasets to load if they no longer exists:
            dslb.mDataSetsToLoad.RemoveAll(s => (System.Array.Find(
                dataSetList, str => str.Equals(s)) == null));
        }
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

    // Draws check boxes for all data sets to choose to load them.
    private void DrawDataSets(DataSetLoadBehaviour dslb, string[] dataSetList)
    {
        foreach (string dataSet in dataSetList)
        {
            bool prevLoadDataSet = dslb.mDataSetsToLoad.Contains(dataSet);
            bool prevActivateDataSet = dslb.mDataSetsToActivate.Contains(dataSet);

            bool nowLoadDataSet = EditorGUILayout.Toggle("Load Data Set " + dataSet, prevLoadDataSet);
            bool nowActivateDataSet = false;
            if (nowLoadDataSet)
                nowActivateDataSet = EditorGUILayout.Toggle("                     Activate", prevActivateDataSet);

            if (dataSet != dataSetList[dataSetList.Length - 1])
            {
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
            }

            // LOAD
            // Remove data sets that are being unchecked.
            if (prevLoadDataSet && (!nowLoadDataSet))
            {
                dslb.mDataSetsToLoad.Remove(dataSet);
            }
            // Add data sets that are being checked.
            else if ((!prevLoadDataSet) && nowLoadDataSet)
            {
                dslb.mDataSetsToLoad.Add(dataSet);
            }

            // ACTIVATE
            // Remove data sets that are being unchecked.
            if (prevActivateDataSet && (!nowActivateDataSet))
            {
                dslb.mDataSetsToActivate.Remove(dataSet);
            }
            // Add data sets that are being checked.
            else if ((!prevActivateDataSet) && nowActivateDataSet)
            {
                dslb.mDataSetsToActivate.Add(dataSet);
            }
        }
    }

    #endregion // PRIVATE_METHODS
}
