/*==============================================================================
        Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
        All Rights Reserved.
        Qualcomm Confidential and Proprietary
==============================================================================*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Splash screen manager.
/// 
/// Draws a SplashScreen with AutoRotation enabled
/// using a GUI Texture for different devices.
/// After 2 seconds of visibility it calls the
/// AboutScreen Scene.
/// </summary>
public class SplashScreenManager : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    public Texture PortraitTextureAndroid;
    public Texture LandscapeTextureAndroid;
    
    public Texture PortraitTextureIPad;
    public Texture LandscapeTextureIPad;
    
    public Texture PortraitTextureIPhone;
    public Texture LandscapeTextureIPhone;

    public float SecondsVisible = 2.0f;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region UNITY_MONOBEHAVIOUR_METHODS
    
    void Start () 
    {
        // Loads the About Scene after N seconds
        Invoke("LoadAboutScene", SecondsVisible);
    }

    void OnGUI()
    {

#if UNITY_IPHONE

        if(QCARRuntimeUtilities.IsLandscapeOrientation)
        {
            if(iPhone.generation == iPhoneGeneration.iPhone)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), LandscapeTextureIPhone);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), LandscapeTextureIPad);
            }
        }
        else
        {
            if(iPhone.generation == iPhoneGeneration.iPhone)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), PortraitTextureIPhone);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), PortraitTextureIPad);
            }
        }

#else

        if (QCARRuntimeUtilities.IsLandscapeOrientation)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), LandscapeTextureAndroid);
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), PortraitTextureAndroid);
        }

#endif

    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    /// <summary>
    /// Loads the about scene.
    /// </summary>
    private void LoadAboutScene()
    {
        Application.LoadLevel("Vuforia-2-AboutScreen");
    }

    #endregion // PRIVATE_METHODS
}
