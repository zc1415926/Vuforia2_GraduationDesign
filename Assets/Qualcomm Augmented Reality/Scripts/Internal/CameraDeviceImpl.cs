/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class CameraDeviceImpl : CameraDevice
{
    #region PROPERTIES
    
    public WebCamImpl WebCam
    {
        get{ return mWebCam; }
    }

    public bool CameraReady
    {
        get
        {
            // the camera is available instantly after initialization on the device,
            // but in the emulator it takes some time to get the actual camera resolution
            if (QCARRuntimeUtilities.IsPlayMode())
            {
                if (mWebCam != null)
                    return mWebCam.IsTextureSizeAvailable;

                return false;
            }
            
            return mCameraReady;
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBERS

    private Dictionary<Image.PIXEL_FORMAT, Image> mCameraImages;
    
    private static WebCamImpl mWebCam = null;

    private bool mCameraReady = false;

    private bool mIsDirty = false;

    #endregion // PRIVATE_MEMBERS

    

    #region PUBLIC_METHODS

    // Initializes the camera.
    public override bool Init(CameraDirection cameraDirection)
    {
        // play mode emulates back facing camera mode only for now
        if (QCARRuntimeUtilities.IsPlayMode())
            cameraDirection = CameraDirection.CAMERA_BACK;
        
        if (InitCameraDevice((int)cameraDirection) == 0)
        {
            return false;
        }

        mCameraReady = true;

        // in play mode, CameraReady does not only evaluate the above flag, but also the web cam state
        if (this.CameraReady)
        {
            QCARBehaviour qcarBehaviour = (QCARBehaviour)Object.FindObjectOfType(typeof(QCARBehaviour));
            if (qcarBehaviour)
            {
                // resets the number of frames for which the color buffer needs to be cleared
                // to avoid rendering artifacts while the camera is being initialized
                qcarBehaviour.ResetClearBuffers();

                // configure the videobackground, set initial reflection setting
                qcarBehaviour.ConfigureVideoBackground(true);
            }
        }

        return true;
    }


    // Deinitializes the camera.
    public override bool Deinit()
    {
        if (DeinitCameraDevice() == 0)
        {
            return false;
        }
        
        
        mCameraReady = false;

        return true;
    }


    // Starts the camera. Frames are being delivered.
    public override bool Start()
    {
        mIsDirty = true;
        // clear bg buffer (previous camera image)
        GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 1.0f));

        if (StartCameraDevice() == 0)
        {
            return false;
        }

        return true;
    }


    // Stops the camera if video feed is not required
    // (e.g. in non-AR mode of an application).
    public override bool Stop()
    {
        if (StopCameraDevice() == 0)
        {
            return false;
        }

        return true;
    }

    // Get the video mode data that matches the given CameraDeviceMode.
    public override VideoModeData GetVideoMode(CameraDeviceMode mode)
    {
        if (QCARRuntimeUtilities.IsPlayMode())
            return WebCam.GetVideoMode();

        else
        {
            IntPtr videoModePtr = Marshal.AllocHGlobal(
                Marshal.SizeOf(typeof (VideoModeData)));
            QCARWrapper.Instance.CameraDeviceGetVideoMode((int)mode, videoModePtr);
            VideoModeData videoMode = (VideoModeData) Marshal.PtrToStructure
                                                          (videoModePtr, typeof (VideoModeData));
            Marshal.FreeHGlobal(videoModePtr);

            return videoMode;
        }
    }


    // Chooses a video mode out of the list of modes.
    // This function can be only called after the camera device has been
    // initialized but not started yet. Once you have started the camera and
    // you need the select another video mode, you need to Stop(), Deinit(),
    // then Init() the camera before calling SelectVideoMode() again.
    public override bool SelectVideoMode(CameraDeviceMode mode)
    {
        if (QCARWrapper.Instance.CameraDeviceSelectVideoMode((int)mode) == 0)
        {
            return false;
        }

        return true;
    }


    // Activate or deactivate the camera device flash.
    // Returns false if flash is not available or can't be activated.
    public override bool SetFlashTorchMode(bool on)
    {
        bool result = QCARWrapper.Instance.CameraDeviceSetFlashTorchMode(on ? 1 : 0) != 0;
        Debug.Log("Toggle flash " + (on ? "ON" : "OFF") + " " + (result ?
                  "WORKED" : "FAILED"));
        return result;
    }


    // Set the active focus mode.
    // Returns false if this mode is not available or can't be activated.
    public override bool SetFocusMode(FocusMode mode)
    {
        bool result = QCARWrapper.Instance.CameraDeviceSetFocusMode((int)mode) != 0;
        Debug.Log("Requested Focus mode " + mode + (result ?
                  " successfully." : ".  Not supported on this device."));
        return result;
    }


    // Enables or disables the request of the camera image in the desired pixel
    // format. Returns true on success, false otherwise. Note that this may
    // result in processing overhead. Image are accessed using GetCameraImage.
    // Note that there may be a delay of several frames until the camera image
    // becomes availables.
    public override bool SetFrameFormat(Image.PIXEL_FORMAT format, bool enabled)
    {
        if (enabled)
        {
            if (!mCameraImages.ContainsKey(format))
            {
                if (QCARWrapper.Instance.QcarSetFrameFormat((int)format, 1) == 0)
                {
                    Debug.LogError("Failed to set frame format");
                    return false;
                }

                Image newImage = new ImageImpl();
                newImage.PixelFormat = format;
                mCameraImages.Add(format, newImage);
                return true;
            }
        }
        else
        {
            if (mCameraImages.ContainsKey(format))
            {
                if (QCARWrapper.Instance.QcarSetFrameFormat((int)format, 0) == 0)
                {
                    Debug.LogError("Failed to set frame format");
                    return false;
                }

                return mCameraImages.Remove(format);
            }
        }

        return true;
    }


    // Returns a camera images for the requested format. Returns null if
    // this image is not available. You must call SetFrameFormat before
    // accessing the corresponding camera image.
    public override Image GetCameraImage(Image.PIXEL_FORMAT format)
    {
        // Has the format been requested:
        if (mCameraImages.ContainsKey(format))
        {
            // Check the image is valid:
            Image image = mCameraImages[format];
            if (image.IsValid())
            {
                return image;
            }
        }

        // No valid image of this format:
        return null;
    }


    // Returns the container of all requested images. The images may or may 
    // not be initialized. Please use GetCameraImage for a list of
    // available and valid images. Used only by the QCARBehaviour.
    public Dictionary<Image.PIXEL_FORMAT, Image> GetAllImages()
    {
        return mCameraImages;
    }

    public bool IsDirty()
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            return mIsDirty || WebCam.IsRendererDirty();
        }
        else
        {
            return mIsDirty;
        }
    }

    public void ResetDirtyFlag()
    {
        mIsDirty = false;
    }

    #endregion // PUBLIC_METHODS



    #region CONSTRUCTION

    public CameraDeviceImpl()
    {
        mCameraImages = new Dictionary<Image.PIXEL_FORMAT, Image>();
    }

    #endregion // CONSTRUCTION



    #region PRIVATE_METHODS

#if !UNITY_EDITOR

    private int InitCameraDevice(int camera)
    {
        return QCARWrapper.Instance.CameraDeviceInitCamera(camera);
    }


    private int DeinitCameraDevice()
    { 
        return QCARWrapper.Instance.CameraDeviceDeinitCamera();
    }


    private int StartCameraDevice()
    {
        return QCARWrapper.Instance.CameraDeviceStartCamera();
    }


    private int StopCameraDevice() 
    { 
        return QCARWrapper.Instance.CameraDeviceStopCamera();
    }

#else // !UNITY_EDITOR

    private int InitCameraDevice(int camera)
    { 
        int rslt = 0;
        
        try
        {

            WebCamBehaviour webCamBehaviour = (WebCamBehaviour) Object.FindObjectOfType(typeof (WebCamBehaviour));
            webCamBehaviour.InitCamera();
            mWebCam = webCamBehaviour.ImplementationClass;

            QCARWrapper.Instance.CameraDeviceSetCameraConfiguration(mWebCam.ResampledTextureSize.x, mWebCam.ResampledTextureSize.y);
            
            rslt = 1;
            
        }catch( NullReferenceException ex ){
            Debug.LogError( ex.Message );
        }

        QCARWrapper.Instance.CameraDeviceInitCamera(camera);
        
        return rslt; 
    }


    private int DeinitCameraDevice()
    {
        int rslt = 0;
        
        if( mWebCam != null ){
            mWebCam.StopCamera();// Stop() releases the Cam instance.
            rslt = 1;
        }

        QCARWrapper.Instance.CameraDeviceDeinitCamera();
        
        return rslt;
    }


    private int StartCameraDevice() 
    {
        int rslt = 0;
        
        if( mWebCam != null ){
            mWebCam.StartCamera();
            rslt = 1;
        }

        QCARWrapper.Instance.CameraDeviceStartCamera();
        
        return rslt;
    }


    private int StopCameraDevice()
    {
        int rslt = 0;
        
        if( mWebCam != null ){
            mWebCam.StopCamera();
            rslt = 1;
        }

        QCARWrapper.Instance.CameraDeviceStopCamera();
        
        return rslt;
    }


#endif // !UNITY_EDITOR

    #endregion // PRIVATE_METHODS
}
