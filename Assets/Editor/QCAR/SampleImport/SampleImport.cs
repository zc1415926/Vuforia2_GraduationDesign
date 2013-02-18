/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.IO;
using UnityEditor;
using UnityEngine;


public class SampleImport : AssetPostprocessor
{
    // This method is called by Unity whenever assets are updated (deleted,
    // moved or added)
    public static void OnPostprocessAllAssets(string[] importedAssets,
                                              string[] deletedAssets,
                                              string[] movedAssets,
                                              string[] movedFromAssetPaths)
    {
        // Set the Unity version for internal use
        string path = Path.Combine(Application.dataPath, "StreamingAssets/QCAR");
        QCARUnityImpl.SetUnityVersion(path);
    }
}