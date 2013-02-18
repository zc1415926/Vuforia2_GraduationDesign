/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;

public class MarkerAccessor : TrackableAccessor
{
    #region PROPERTIES

    // A property that returns the constant marker name prefix.
    public static string MarkerNamePrefix
    {
        get
        {
            return MARKER_NAME_PREFIX;
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    private const string MARKER_NAME_PREFIX = "mymarker";

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    // The one MarkerBehaviour instance this accessor belongs to is set in the
    // constructor.
    public MarkerAccessor(MarkerBehaviour target)
    {
        mTarget = target;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    // This method is called when new configuration values are available and
    // need to be applied to Marker objects in the scene.
    public override void ApplyDataSetProperties()
    {
        // Prefabs should not be editable
        if (QCARUtilities.GetPrefabType(mTarget) == PrefabType.Prefab)
        {
            return;
        }

        //MarkerBehaviour markerBehaviour = (MarkerBehaviour)mTarget;

        //ConfigData.FrameMarker fmConfig;
        
        //SceneManager.Instance.GetFrameMarker(markerBehaviour.MarkerID, out fmConfig);

        //markerBehaviour.TrackableName = fmConfig.name;
        //MarkerEditor.CreateMesh(markerBehaviour);
        //MarkerEditor.UpdateScale(markerBehaviour, fmConfig.size);
    }


    public override void ApplyDataSetAppearance()
    {
        throw new System.NotImplementedException();
    }

    #endregion // PUBLIC_METHODS
}
