/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


using UnityEngine;

/// <summary>
/// This class serves as a thin abstraction layer between Unity's WebCamTexture and Vuforia
/// It is also Null-implemented for the case where no webcam is connected.
/// </summary>
public abstract class WebCamTexAdaptor
{
    #region PROPERTIES

    public abstract bool DidUpdateThisFrame { get; }
    public abstract bool IsPlaying { get; }
    public abstract Texture Texture { get; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    public abstract void Play();
    public abstract void Stop();

    #endregion // PUBLIC_METHDOS
}