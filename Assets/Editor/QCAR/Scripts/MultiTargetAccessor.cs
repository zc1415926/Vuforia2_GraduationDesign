/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;
using UnityEditor;

public class MultiTargetAccessor : TrackableAccessor
{
    #region CONSTRUCTION

    // The one MultiTargetBehaviour instance this accessor belongs to is set in
    // the constructor.
    public MultiTargetAccessor(MultiTargetBehaviour target)
    {
        mTarget = target;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    // This method updates the respective Trackable properties (e.g. size)
    // with data set data.
    public override void ApplyDataSetProperties()
    {
        // Prefabs should not be editable
        if (QCARUtilities.GetPrefabType(mTarget) == PrefabType.Prefab)
        {
            return;
        }

        IEditorMultiTargetBehaviour mtb = (MultiTargetBehaviour)mTarget;

        ConfigData.MultiTargetData mtConfig;
        if (TrackableInDataSet(mtb.TrackableName, mtb.DataSetName))
        {
            ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(mtb.DataSetName);
            dataSetData.GetMultiTarget(mtb.TrackableName, out mtConfig);
        }
        else
        {
            // If the Trackable has been removed from the data set we reset it to default.
            ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME);
            dataSetData.GetMultiTarget(QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME, out mtConfig);
            mtb.SetDataSetPath(QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME);
            mtb.SetNameForTrackable(QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME);
        }

        List<ConfigData.MultiTargetPartData> prtConfigs = mtConfig.parts;

        MultiTargetEditor.UpdateParts(mtb, prtConfigs.ToArray());
    }


    // This method updates the respective Trackable appearance (e.g.
    // aspect ratio and texture) with data set data.
    public override void ApplyDataSetAppearance()
    {
        // MultiTarget reconfiguration in the editor is not yet supported.
        // We therefore always update all Multi Target values.
        ApplyDataSetProperties();
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private bool TrackableInDataSet(string trackableName, string dataSetName)
    {
        if (ConfigDataManager.Instance.ConfigDataExists(dataSetName))
        {
            ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(dataSetName);
            if (dataSetData.MultiTargetExists(trackableName))
            {
                return true;
            }
        }
        return false;
    }

    #endregion // PRIVATE_METHODS
}
