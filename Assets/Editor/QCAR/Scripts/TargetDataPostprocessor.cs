/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// The target data postprocessor class generates callbacks on file import of
// QCAR related files (tracking dataset, configuration file,
// Image Target textures)
public class TargetDataPostprocessor : AssetPostprocessor
{
    #region NESTED

    // The import state defines how a file has been modified on import.
    public enum ImportState
    {
        NONE,    // Default state. File was not imported.
        ADDED,   // File has not existed before and was therefore added.
        RENAMED, // File has existed before and was automatically renamed.
        DELETED  // File was not imported and an existing copy was kept.
    }

    #endregion // NESTED



    #region UNITY_EDITOR_METHODS

    // This method is called by Unity whenever assets are updated (deleted,
    // moved or added).
    public static void OnPostprocessAllAssets(string[] importedAssets,
                                              string[] deletedAssets,
                                              string[] movedAssets,
                                              string[] movedFromAssetPaths)
    {
        bool filesUpdated = false;

        // Check if there are relevant files that have been imported.
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset.IndexOf(
                QCARUtilities.GlobalVars.DATA_SET_PATH,
                System.StringComparison.OrdinalIgnoreCase) != -1)
            {
                filesUpdated = true;

                // No more checks needed.
                break;
            }
        }

        // Check if there are relevant files that have been deleted.
        if (!filesUpdated)
        {
            foreach (string deletedAsset in deletedAssets)
            {
                if (deletedAsset.IndexOf(
                    QCARUtilities.GlobalVars.DATA_SET_PATH,
                    System.StringComparison.OrdinalIgnoreCase) != -1)
                {
                    filesUpdated = true;

                    // No more checks needed.
                    break;
                }
            }
        }

        // Check if there are relevant files that have been renamed/moved.
        if (!filesUpdated)
        {
            foreach (string movedAsset in movedAssets)
            {
                if (movedAsset.IndexOf(
                    QCARUtilities.GlobalVars.DATA_SET_PATH,
                    System.StringComparison.OrdinalIgnoreCase) != -1)
                {
                    filesUpdated = true;

                    // No more checks needed.
                    break;
                }
            }
        }

        // We only alert the SceneManager if files have actually been
        // changed.
        if (filesUpdated)
        {
            SceneManager.Instance.FilesUpdated();
        }
    }

    #endregion // UNITY_EDITOR_METHODS
}