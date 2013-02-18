/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// The QCARManager manages updating trackables, their state and position to the camera
/// It is called from the QCARBehaviour.
/// </summary>
public abstract class QCARManager
{
    #region PROPERTIES

    /// <summary>
    /// Returns an instance of a QCARManager (thread safe)
    /// </summary>
    public static QCARManager Instance
    {
        get
        {
            // Make sure only one instance of QCARManager is created.
            if (sInstance == null)
            {
                lock (typeof(QCARManager))
                {
                    if (sInstance == null)
                    {
                        sInstance = new QCARManagerImpl();
                    }
                }
            }
            return sInstance;
        }
    }

    /// <summary>
    /// World Center Mode setting on the ARCamera
    /// </summary>
    public abstract QCARBehaviour.WorldCenterMode WorldCenterMode { get; set; }

    /// <summary>
    /// World Center setting on the ARCamera
    /// </summary>
    public abstract TrackableBehaviour WorldCenter { get; set; }

    /// <summary>
    /// A handle to the ARCamera object
    /// </summary>
    public abstract Camera ARCamera { get; set; }

    /// <summary>
    /// True to have QCAR render the video background image natively
    /// False to bind the video background to the texture set in
    /// QCARRenderer.SetVideoBackgroundTexture
    /// </summary>
    public abstract bool DrawVideoBackground { get; set; }

    /// <summary>
    /// returns true once the QCARManager has been initialized
    /// </summary>
    public abstract bool Initialized { get; }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    private static QCARManager sInstance = null;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    /// Initialization
    /// </summary>
    public abstract bool Init();


    /// <summary>
    /// Free globally allocated containers
    /// </summary>
    public abstract void Deinit();

    #endregion // PUBLIC_METHODS
}
