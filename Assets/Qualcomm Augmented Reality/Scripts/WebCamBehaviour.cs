/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This MonoBehaviour manages the usage of a webcam for Play Mode in Windows or Mac.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebCamBehaviour : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES
    
    /// <summary>
    /// The camera that will be used to render the background in Play Mode
    /// </summary>
    public Camera BackgroundCameraPrefab;

    /// <summary>
    /// The layer that will be used to render the background image
    /// </summary>
    public int RenderTextureLayer;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    private string mDeviceNameSetInEditor;

    [SerializeField]
    [HideInInspector]
    private bool mFlipHorizontally = false;

    [SerializeField]
    [HideInInspector]
    private bool mTurnOffWebCam = false;

    // pointer to the implementation class encapsulating all the functionality
    private WebCamImpl mWebCamImpl;
    // pointer to hold the instanciated background rendering camera
    private Camera mBackgroundCameraInstance;

    #endregion // PRIVATE_MEMBER_VARIABLES
    


    #region PROPERTIES
    
    /// <summary>
    /// The name of the web cam device that is used
    /// </summary>
    public string DeviceName
    {
        get{ return mDeviceNameSetInEditor;  }
        set
        {
            mDeviceNameSetInEditor = value;
        }
    }
    
    /// <summary>
    /// Reference to the internal implementation class
    /// </summary>
    public WebCamImpl ImplementationClass
    {
        get { return mWebCamImpl; }    
    }

    /// <summary>
    /// If the image from the webcam should be flipped horizontally.
    /// Some webcams will deliver a mirrored image, in that case 
    /// </summary>
    public bool FlipHorizontally
    {
        get { return mFlipHorizontally; }
        set { mFlipHorizontally = value; }
    }

    /// <summary>
    /// The webcam can also be turned off if wanted.
    /// In this case, all Trackalbes will be considered tracked at their
    /// initial position.
    /// </summary>
    public bool TurnOffWebCam
    {
        get { return mTurnOffWebCam; }
        set { mTurnOffWebCam = value; }
    }

    /// <summary>
    /// If the web cam is currently capturing video
    /// </summary>
    public bool IsPlaying
    {
        get { return mWebCamImpl.IsPlaying; }
    }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Initialized the camera
    /// </summary>
    public void InitCamera ()
    {
        if (mWebCamImpl == null)
        {
            // in play mode, do not pause when focus is lost
            Application.runInBackground = true;

            Camera arCamera = gameObject.GetComponent<Camera>();
            mBackgroundCameraInstance = Instantiate(BackgroundCameraPrefab) as Camera;
            mWebCamImpl = new WebCamImpl(arCamera, mBackgroundCameraInstance, RenderTextureLayer, mDeviceNameSetInEditor, mFlipHorizontally);
        }
    }

    /// <summary>
    /// Starts the camera
    /// </summary>
    public void StartCamera()
    {
        mWebCamImpl.StartCamera();
    }
    
    /// <summary>
    /// Stops the camera
    /// </summary>
    public void StopCamera()
    {
        mWebCamImpl.StopCamera();
    }

    /// <summary>
    /// Checks for native plugin support. Vuforia relies on native plugins, but they are not
    /// supported in the free Unity version on desktops.
    /// </summary>
    public bool CheckNativePluginSupport()
    {
#if UNITY_EDITOR
            int nativePluginSupport = 0;
            try
            {
                nativePluginSupport = qcarCheckNativePluginSupport();
            }
            catch (Exception)
            {
                // an exception occurred while calling into native, reset flag:
                nativePluginSupport = 0;
            }

            return (nativePluginSupport == 1);
#else
        return true;
#endif
    }

    /// <summary>
    /// If the web cam is actually used.
    /// </summary>
    public bool IsWebCamUsed()
    {
        // if webcam support is not turned off and native plugins are supported and a webcam is connected
        return ((!mTurnOffWebCam) && CheckNativePluginSupport() && (WebCamTexture.devices.Length > 0)) ;
    }

    #endregion // PUBLIC_METHODS



    #region UNITY_MONOBEHAVIOUR_METHODS
    
#if UNITY_EDITOR

    void OnDestroy()
    {
        if (mWebCamImpl != null)
        {
            mWebCamImpl.OnDestroy();

            // destroy BackgroundCamera instance
            Destroy(mBackgroundCameraInstance);
        }
    }

    void Update()
    {
        if (mWebCamImpl != null) mWebCamImpl.Update();
    }

#endif

    #endregion // UNITY_MONOBEHAVIOUR_METHODS
    

#if UNITY_EDITOR

    #region NATIVE_FUNCTIONS

    [DllImport(QCARMacros.PLATFORM_DLL)]
    private static extern int qcarCheckNativePluginSupport();

    #endregion // NATIVE_FUNCTIONS

#endif
}
