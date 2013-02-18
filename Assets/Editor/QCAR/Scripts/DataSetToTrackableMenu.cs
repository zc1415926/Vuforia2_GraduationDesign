/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;

public class DataSetToTrackableMenu : Editor
{
    #region PUBLIC_METHODS

    [MenuItem("Vuforia/Apply Data Set Properties", false, 2)]
    public static void ApplyDataSetProperties()
    {
        SceneManager.Instance.ApplyDataSetProperties();
    }

    #endregion // PUBLIC_METHODS
}