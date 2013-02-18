/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// This class takes care about rendering the video background in the right size and orientation
/// </summary>
public abstract class QCARRenderer
{
    #region NESTED

    /// <summary>
    /// If the video background image is mirrored horizontally (useful when the front camera is used)
    /// </summary>
    public enum VideoBackgroundReflection
    {
        DEFAULT,  // Allows the SDK to set the recommended reflection settings for the current camera
        ON,       // Overrides the SDK recommendation to force a reflection
        OFF       // Overrides the SDK recommendation to disable reflection
    }

    /// <summary>
    /// This struct stores Video Background configuration data. It stores if
    /// background rendering is enabled, if it happens synchronously and it
    /// stores position and size of the video background on the screen.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VideoBGCfgData
    {
        public int enabled;
        public int synchronous;
        public Vec2I position;
        public Vec2I size;
        [MarshalAs(UnmanagedType.SysInt)] 
        public VideoBackgroundReflection reflection;
    }

    /// <summary>
    /// This struct stores 2D integer vectors.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vec2I
    {
        public int x;
        public int y;
        
        public Vec2I(int v1, int v2)
        {
            x = v1;
            y = v2;
        }
    }

    /// <summary>
    /// Describes the size of the texture in the graphics unit as well as
    /// the size of the image inside the texture. The latter corresponds
    /// to the size of the image delivered by the camera
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VideoTextureInfo
    {
        public Vec2I textureSize;
        public Vec2I imageSize;
    }

    #endregion // NESTED



    #region PROPERTIES

    /// <summary>
    /// Returns an instance of a QCARRenderer (thread safe)
    /// </summary>
    public static QCARRenderer Instance
    {
        get
        {
            // Make sure only one instance of CameraDevice is created.
            if (sInstance == null)
            {
                lock (typeof(QCARRenderer))
                {
                    if (sInstance == null)
                    {
                        sInstance = new QCARRendererImpl();
                    }
                }
            }
            return sInstance;
        }
    }


    /// <summary>
    /// True to have Vuforia render the video background image natively
    /// False to bind the video background to the texture set in
    /// QCARRenderer.SetVideoBackgroundTextureID
    /// </summary>
    public abstract bool DrawVideoBackground { get; set; }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBERS

    private static QCARRenderer sInstance = null;

    #endregion // PRIVATE_MEMBERS



    #region PUBLIC_METHODS

    /// <summary>
    /// Retrieves the current layout configuration of the video background.
    /// </summary>
    public abstract VideoBGCfgData GetVideoBackgroundConfig();


    /// <summary>
    /// clears the config of the videobackground when QCAR is stopped.
    /// </summary>
    public abstract void ClearVideoBackgroundConfig();
    

    /// <summary>
    /// Configures the layout of the video background (location on the screen
    /// and size).
    /// </summary>
    public abstract void SetVideoBackgroundConfig(VideoBGCfgData config);


    /// <summary>
    /// Tells QCAR where the texture id to use for updating video
    /// background data
    /// </summary>
    public abstract bool SetVideoBackgroundTexture(Texture2D texture);


    /// <summary>
    /// Check if video background info is available
    /// </summary>
    public abstract bool IsVideoBackgroundInfoAvailable();


    /// <summary>
    /// Returns the texture info associated with the current video background
    /// </summary>
    public abstract VideoTextureInfo GetVideoTextureInfo();

    #endregion // PUBLIC_METHODS
}
