/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


using UnityEngine;

/// <summary>
/// A static utility class that exposes helper functions from QCAR
/// </summary>
public static class QCARUnity
{
    #region NESTED

    /// <summary>
    /// InitError is an error value that is returned by QCAR if something goes
    /// wrong at initialization.
    /// </summary>
    public enum InitError
    {
        // Device is not supported by QCAR.
        INIT_DEVICE_NOT_SUPPORTED = -2,
        // Another (unknown) initialization error has occured.
        INIT_ERROR = -1,
        // Everything is fine
        INIT_SUCCESS = 0
    }

    /// <summary>
    /// Hints are used to push the tracker into certain behavior. Hints (as the
    /// name suggests) are not always guaranteed to work.
    /// </summary>
    public enum QCARHint
    {
        // Specify the number of Image Targets that are handled by the tracker
        // at once.
        HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS = 0,
    }

    #endregion // NESTED



    #region PUBLIC_METHODS

    /// <summary>
    /// Retrieves initialization error code or success
    /// </summary>
    public static InitError CheckInitializationError()
    {
        return QCARUnityImpl.CheckInitializationError();
    }


    /// <summary>
    /// Checks if the GL surface has changed
    /// </summary>
    public static bool IsRendererDirty()
    {
        return QCARUnityImpl.IsRendererDirty();
    }


    /// <summary>
    /// Sets a hint for the QCAR SDK
    /// Hints help the SDK to understand the developer's needs.
    /// However, depending on the device or SDK version the hints
    /// might not be taken into consideration.
    /// Returns false if the hint is unknown or deprecated.
    /// </summary>
    public static bool SetHint(QCARHint hint, int value)
    {
        return QCARUnityImpl.SetHint(hint, value);
    }


    /// <summary>
    /// Indicates whether the rendering surface needs to support an alpha channel
    // for transparency
    /// </summary>
    public static bool RequiresAlpha()
    {
        return QCARUnityImpl.RequiresAlpha();
    }


    /// <summary>
    /// Returns the QCAR projection matrix
    /// </summary>
    public static Matrix4x4 GetProjectionGL(float nearPlane, float farPlane, ScreenOrientation screenOrientation)
    {
        return QCARUnityImpl.GetProjectionGL(nearPlane, farPlane, screenOrientation);
    }

    #endregion // PUBLIC_METHODS
}
