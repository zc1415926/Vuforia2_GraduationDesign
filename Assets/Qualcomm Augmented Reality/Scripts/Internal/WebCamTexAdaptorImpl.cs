/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


using UnityEngine;

/// <summary>
/// This class serves as a thin abstraction layer between Unity's WebCamTexture and Vuforia
/// </summary>
public class WebCamTexAdaptorImpl : WebCamTexAdaptor
{
    #region PRIVATE_MEMBER_VARIALBES

    private WebCamTexture mWebCamTexture;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    public override bool DidUpdateThisFrame
    {
        get { return mWebCamTexture.didUpdateThisFrame; }
    }

    public override bool IsPlaying
    {
        get { return mWebCamTexture.isPlaying; }
    }

    public override Texture Texture
    {
        get { return mWebCamTexture; }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    public WebCamTexAdaptorImpl(string deviceName, int requestedFPS, QCARRenderer.Vec2I requestedTextureSize)
    {
        mWebCamTexture = new WebCamTexture();
        mWebCamTexture.deviceName = deviceName;
        mWebCamTexture.requestedFPS = requestedFPS;
        mWebCamTexture.requestedWidth = requestedTextureSize.x;
        mWebCamTexture.requestedHeight = requestedTextureSize.y;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    public override void Play()
    {
        mWebCamTexture.Play();
    }

    public override void Stop()
    {
        mWebCamTexture.Stop();
    }

    #endregion // PUBLIC_METHDOS
}