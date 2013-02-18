/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

/// <summary>
/// Null implementation for a WebCamTexture
/// Used in case no webcam is connected
/// </summary>
public class NullWebCamTexAdaptor : WebCamTexAdaptor
{
    #region PRIVATE_MEMBER_VARIALBES

    private readonly Texture2D mTexture;
    private bool mPseudoPlaying = true;
    
    // used to fake fps
    private readonly double mMsBetweenFrames;
    private DateTime mLastFrame;
    private const string ERROR_MSG = "No camera connected!\nTo run your application using Play Mode, please connect a webcam to your computer.";

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// fake the requested fps - report back a new frame every x milliseconds
    /// if we reported back a new frame every time, this would cause a performance issue.
    /// </summary>
    public override bool DidUpdateThisFrame
    {
        get 
        {
            if ((DateTime.Now - mLastFrame).TotalMilliseconds > mMsBetweenFrames)
            {
                mLastFrame = DateTime.Now;
                return true;
            }
            return false;
        }
    }

    public override bool IsPlaying
    {
        get { return mPseudoPlaying; }
    }

    public override Texture Texture
    {
        get { return mTexture; }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    public NullWebCamTexAdaptor(int requestedFPS, QCARRenderer.Vec2I requestedTextureSize)
    {
        mTexture = new Texture2D(requestedTextureSize.x, requestedTextureSize.y);
        mMsBetweenFrames = 1000d/requestedFPS;
        // initialize last frame way back
        mLastFrame = DateTime.Now - TimeSpan.FromDays(1);

        if (QCARRuntimeUtilities.IsQCAREnabled())
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Error occurred!", ERROR_MSG, "Ok");
#endif
            Debug.LogError(ERROR_MSG);
        }
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    public override void Play()
    {
        mPseudoPlaying = true;
    }

    public override void Stop()
    {
        mPseudoPlaying = false;
    }

    #endregion // PUBLIC_METHDOS
}