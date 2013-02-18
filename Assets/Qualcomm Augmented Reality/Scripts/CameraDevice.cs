/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// This class provides access to camera methods and properties
/// </summary>
public abstract class CameraDevice
{
    #region NESTED
    

    /// <summary>
    /// The mode used for camera capturing and video rendering.
    /// The camera device mode is set through the Unity inspector.
    /// </summary>
    public enum CameraDeviceMode
    {
        // Best compromise between speed and quality.
        MODE_DEFAULT = -1,
        // Optimize for speed. Quality of the video background could suffer.
        MODE_OPTIMIZE_SPEED = -2,
        // Optimize for quality. Application performance could go down.
        MODE_OPTIMIZE_QUALITY = -3
    }
    
    /// <summary>
    /// The different focus modes for the active camera
    /// </summary>
    public enum FocusMode
    {
        FOCUS_MODE_NORMAL,           // Default focus mode
        FOCUS_MODE_TRIGGERAUTO,      // Triggers a single autofocus operation
        FOCUS_MODE_CONTINUOUSAUTO,   // Continuous autofocus mode
        FOCUS_MODE_INFINITY,         // Focus set to infinity
        FOCUS_MODE_MACRO             // Macro mode for close-up focus
    }

    /// <summary>
    /// If the front, back or default camera on a device should be used
    /// Not all devices have both a back and a front camera.
    /// </summary>
    public enum CameraDirection
    {
        CAMERA_DEFAULT,              // Default camera device.  Usually BACK
        CAMERA_BACK,                 // Rear facing camera
        CAMERA_FRONT                 // Front facing camera
    };

    /// <summary>
    /// This struct stores video mode data. This includes the width and height of
    /// the frame and the framerate of the camera.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VideoModeData
    {
        public int width;
        public int height;
        public float frameRate;
    }

    #endregion // NESTED



    #region PROPERTIES

    /// <summary>
    /// Returns an instance of a CameraDevice (thread safe)
    /// </summary>
    public static CameraDevice Instance
    {
        get
        {
            // Make sure only one instance of CameraDevice is created.
            if (mInstance == null)
            {
                lock (typeof(CameraDevice))
                {
                    if (mInstance == null)
                    {
                        mInstance = new CameraDeviceImpl();
                    }
                }
            }
            return mInstance;
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBERS

    private static CameraDevice mInstance = null;

    #endregion // PRIVATE_MEMBERS



    #region PUBLIC_METHODS

    /// <summary>
    /// Initializes the camera.
    /// </summary>
    public abstract bool Init(CameraDirection cameraDirection);


    /// <summary>
    /// Deinitializes the camera.
    /// </summary>
    public abstract bool Deinit();


    /// <summary>
    /// Starts the camera. Frames are being delivered.
    /// </summary>
    public abstract bool Start();


    /// <summary>
    /// Stops the camera if video feed is not required
    /// (e.g. in non-AR mode of an application).
    /// </summary>
    public abstract bool Stop();


    /// <summary>
    ///  Get the video mode data that matches the given CameraDeviceMode.
    /// </summary>
    public abstract VideoModeData GetVideoMode(CameraDeviceMode mode);


    /// <summary>
    /// Chooses a video mode out of the list of modes.
    /// This function can be only called after the camera device has been
    /// initialized but not started yet. Once you have started the camera and
    /// you need the select another video mode, you need to Stop(), Deinit(),
    /// then Init() the camera before calling SelectVideoMode() again.
    /// </summary>
    public abstract bool SelectVideoMode(CameraDeviceMode mode);


    /// <summary>
    /// Activate or deactivate the camera device flash.
    /// Returns false if flash is not available or can't be activated.
    /// </summary>
    public abstract bool SetFlashTorchMode(bool on);


    /// <summary>
    /// Set the active focus mode.
    /// Returns false if this mode is not available or can't be activated.
    /// </summary>
    public abstract bool SetFocusMode(FocusMode mode);


    /// <summary>
    ///  Enables or disables the request of the camera image in the desired pixel
    /// format. Returns true on success, false otherwise. Note that this may
    /// result in processing overhead. Image are accessed using GetCameraImage.
    /// Note that there may be a delay of several frames until the camera image
    /// becomes availables.
    /// </summary>
    public abstract bool SetFrameFormat(Image.PIXEL_FORMAT format, bool enabled);


    /// <summary>
    /// Returns a camera images for the requested format. Returns null if
    /// this image is not available. You must call SetFrameFormat before
    /// accessing the corresponding camera image.
    /// </summary>
    public abstract Image GetCameraImage(Image.PIXEL_FORMAT format);

    #endregion // PUBLIC_METHODS
}
