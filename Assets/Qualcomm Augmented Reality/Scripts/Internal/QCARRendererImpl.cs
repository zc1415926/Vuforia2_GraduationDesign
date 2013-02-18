/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class QCARRendererImpl : QCARRenderer
{
    #region NESTED

    #endregion // NESTED



    #region PROPERTIES

    // True to have QCAR render the video background image natively
    // False to bind the video background to the texture set in
    // QCARRenderer.SetVideoBackgroundTextureID
    public override bool DrawVideoBackground
    {
        // Forward to QCAR Manager:
        set { QCARManager.Instance.DrawVideoBackground = value; }
        get { return QCARManager.Instance.DrawVideoBackground; }
    }

    // texture to render the video background into if not, only for internal usage, only in emulator
    public Texture2D VideoBackgroundForEmulator { private set; get; }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBERS

    private VideoBGCfgData mVideoBGConfig;
    private bool mVideoBGConfigSet = false;

    #endregion // PRIVATE_MEMBERS



    #region PUBLIC_METHODS


    // Retrieves the current layout configuration of the video background.
    public override VideoBGCfgData GetVideoBackgroundConfig()
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            return mVideoBGConfig;
        }
        else
        {
            IntPtr configPtr = Marshal.AllocHGlobal(
                                Marshal.SizeOf(typeof(VideoBGCfgData)));
            QCARWrapper.Instance.RendererGetVideoBackgroundCfg(configPtr);
            VideoBGCfgData config = (VideoBGCfgData) Marshal.PtrToStructure
                                (configPtr, typeof(VideoBGCfgData));
            Marshal.FreeHGlobal(configPtr);

            return config;
        }
    }


    // clears the config of the videobackground when QCAR is stopped.
    public override void ClearVideoBackgroundConfig()
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            // remember the config has been cleared (checking for null not possible for structs)
            mVideoBGConfigSet = false;
        }
    }


    // Configures the layout of the video background (location on the screen
    // and size).
    public override void SetVideoBackgroundConfig(VideoBGCfgData config)
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            mVideoBGConfig = config;
            // remember the config has been set (checking for null not possible for structs)
            mVideoBGConfigSet = true;
        }
        else
        {
            IntPtr configPtr = Marshal.AllocHGlobal(
                                Marshal.SizeOf(typeof(VideoBGCfgData)));
            Marshal.StructureToPtr(config, configPtr, true);
            QCARWrapper.Instance.RendererSetVideoBackgroundCfg(configPtr);
            Marshal.FreeHGlobal(configPtr);
        }
    }


    // Tells QCAR where the texture id to use for updating video
    // background data
    public override bool SetVideoBackgroundTexture(Texture2D texture)
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            VideoBackgroundForEmulator = texture;
            return true;
        }
        else
        {
            if (texture != null)
            {
                return QCARWrapper.Instance.RendererSetVideoBackgroundTextureID(texture.GetNativeTextureID()) != 0;
            }
            return true;
        }
    }


    // Check if video background info is available
    public override bool IsVideoBackgroundInfoAvailable()
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            // info is available if the config has been set
            return mVideoBGConfigSet;
        }
        else
        {
            return QCARWrapper.Instance.RendererIsVideoBackgroundTextureInfoAvailable() != 0;
        }
    }


    // Returns the texture info associated with the current video background
    public override VideoTextureInfo GetVideoTextureInfo()
    {
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
            return cameraDeviceImpl.WebCam.GetVideoTextureInfo();
        }
        else
        {
            IntPtr ptr = Marshal.AllocHGlobal(
                Marshal.SizeOf(typeof (VideoTextureInfo)));

            QCARWrapper.Instance.RendererGetVideoBackgroundTextureInfo(ptr);

            VideoTextureInfo info = (VideoTextureInfo) Marshal.PtrToStructure
                                                           (ptr, typeof (VideoTextureInfo));

            Marshal.FreeHGlobal(ptr);

            return info;
        }
    }

    #endregion // PUBLIC_METHODS
}
