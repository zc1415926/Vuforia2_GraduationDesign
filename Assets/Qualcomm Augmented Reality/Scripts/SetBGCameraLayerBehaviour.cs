/*==============================================================================
            Copyright (c) 2012 Qualcomm Austria Research Center GmbH.
            All Rights Reserved.
            Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

/// <summary>
/// This Behaviour allows to set a layer ID that is applied to all child gameobjects
/// and the camera is set to cull away everything except this layer.
/// This is necessary because unity packages won't export the layers set up in a project
/// </summary>
[RequireComponent(typeof(Camera))]
public class SetBGCameraLayerBehaviour : MonoBehaviour
{

    #region PRIVATE_MEMBER_VARIABLES
    
    /// <summary>
    /// the layer that sould be used for all objects rendered by this camera
    /// </summary>
    public int CameraLayer;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Awake()
    {
        // put all gameobjects below this one into the given layer
        ApplyCameraLayerRecursive(gameObject);

        // switch on the given layer for the camera
        Camera camera = GetComponent<Camera>();
        camera.cullingMask |= (1 << CameraLayer);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    private void ApplyCameraLayerRecursive(GameObject go)
    {
        go.layer = CameraLayer;

        // recursive
        for (int i = 0; i < go.transform.GetChildCount(); i++)
            ApplyCameraLayerRecursive(go.transform.GetChild(i).gameObject);
    }
    
    #endregion // PRIVATE_METHODS
}