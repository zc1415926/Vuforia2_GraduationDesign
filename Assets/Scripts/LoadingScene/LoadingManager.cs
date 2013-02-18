/*==============================================================================
        Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// Loading manager.
/// 
/// This Script handles the loading of the Main scene in background
/// displaying a loading animation while the scene is being loaded
/// </summary>
public class LoadingManager : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// The texture that will be rotated in the center of the screen 
    /// </summary>
    public Texture Spinner;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private bool mChangeLevel = true;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Awake()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }


    void Start()
    {
        Resources.UnloadUnusedAssets();

        System.GC.Collect();

        Application.backgroundLoadingPriority = ThreadPriority.Low;

        mChangeLevel = true;
    }


    void Update()
    {
        if (mChangeLevel)
        {
            LoadImageTargetsScene();
            mChangeLevel = false;
        }
    }


    void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        float thisAngle = Time.frameCount*4;

        Rect thisRect = new Rect(Screen.width/2.0f - Spinner.width/2f, Screen.height/2.0f - Spinner.height/2f,
                                 Spinner.width, Spinner.height);

        GUIUtility.RotateAroundPivot(thisAngle, thisRect.center);
        GUI.DrawTexture(thisRect, Spinner);
        GUI.matrix = oldMatrix;
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    /// <summary>
    /// Loads the ImageTargets scene.
    /// </summary>
    private void LoadImageTargetsScene()
    {
        Application.LoadLevelAsync("Vuforia-4-ImageTargets");
    }

    #endregion // PRIVATE_METHODS
}
