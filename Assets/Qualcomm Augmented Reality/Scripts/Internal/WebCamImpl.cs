/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

public class WebCamImpl
{
    #region NESTED

     struct BufferedFrame
     {
         public int frameIndex;
         public RenderTexture frame;
     }

    #endregion // NESTED



    #region PRIVATE_MEMBER_VARIABLES

    // pointer to ARCamera object
    private readonly Camera mARCamera;

// these cause compile-time warnings on mobile platforms if not excluded
#if UNITY_EDITOR
    private readonly CameraClearFlags mOriginalCameraClearFlags;
    private readonly int mOriginalCameraCullMask;

    // instantiated Background Camera for BG texture rendering
    private readonly Camera mBackgroundCameraInstance;
#endif

    // The BGRenderingBehaviour component on the background camera
    private readonly BGRenderingBehaviour mBgRenderingTexBehaviour = null;
    
    private readonly WebCamTexAdaptor mWebCamTexture = null;

    // config structs used to describe video texture and modes
    private CameraDevice.VideoModeData mVideoModeData = new CameraDevice.VideoModeData();
    private QCARRenderer.VideoTextureInfo mVideoTextureInfo = new QCARRenderer.VideoTextureInfo();

    // helper class encapsulating renderToTexture functionality
    private TextureRenderer mTextureRenderer = null;

    // this texture is used to read pixels from the RenderTextures buffered by the TextureRenderer
    private Texture2D mBufferReadTexture = null;
    private Rect mReadPixelsRect = new Rect();

    // contains config values for the web camera
    private readonly WebCamProfile.ProfileData mWebCamProfile = new WebCamProfile.ProfileData();

    // if the camera image should be mirrored
    private readonly bool mFlipHorizontally = false;

    // stores the last screen size to allow triggering video bg updates on changes:
    private int mLastScreenWidth;
    private int mLastScreenHeight;

    // Queue of Buffered Frames for synchronization
    private readonly Queue<BufferedFrame> mBufferedFrames = new Queue<BufferedFrame>();
    private int mLastFrameIdx = -1;

    // the layer for rendering the buffered frames in
    private readonly int mRenderTextureLayer;

    #endregion // PRIVATE_MEMBER_VARIABLES
    


    #region PROPERTIES
    
    public bool DidUpdateThisFrame
    {
        get
        {
            return IsTextureSizeAvailable && mWebCamTexture.DidUpdateThisFrame;
        }    
    }

    public bool IsPlaying
    {
        get { return mWebCamTexture.IsPlaying; }
    }

    public int ActualWidth
    {
        get { return mTextureRenderer.Width; }
    }

    public int ActualHeight
    {
        get { return mTextureRenderer.Height; }
    }

    public bool IsTextureSizeAvailable 
    { get; private set; }

    public bool FlipHorizontally
    {
        get { return mFlipHorizontally; }
    }

    public QCARRenderer.Vec2I ResampledTextureSize
    {
        get { return mWebCamProfile.ResampledTextureSize; }
    }

    #endregion // PROPERTIES



    #region PRIVATE_METHODS

    /// <summary>
    /// renders the given texture in the background, either itself or into a texture that was set to receive it
    /// </summary>
    private void RenderFrame(RenderTexture frameToDraw)
    {
        if (QCARRenderer.Instance.DrawVideoBackground)
        {
            // set it for the background renderer
            mBgRenderingTexBehaviour.SetTexture(frameToDraw);
        }
        else
        {
            // we do not render the background ourselves, so render it to the designated texture
            Texture2D targetTexture = ((QCARRendererImpl)QCARRenderer.Instance).VideoBackgroundForEmulator;
            if (targetTexture != null)
            {
                if (targetTexture.width != frameToDraw.width ||
                    targetTexture.height != frameToDraw.height ||
                    targetTexture.format != TextureFormat.ARGB32)
                    targetTexture.Resize(frameToDraw.width, frameToDraw.height, TextureFormat.ARGB32, false);

                RenderTexture.active = frameToDraw;
                targetTexture.ReadPixels(new Rect(0, 0, frameToDraw.width, frameToDraw.height), 0, 0);
                targetTexture.Apply();
            }
        }
    }

    #endregion // PRIVATE_METHODS



    #region CONSTRUCTION

    public WebCamImpl(Camera arCamera, Camera backgroundCamera, int renderTextureLayer, string webcamDeviceName, bool flipHorizontally)
    {
#if UNITY_EDITOR
        mRenderTextureLayer = renderTextureLayer;

        // get main camera, set correct clear flags:
        mARCamera = arCamera;
        mOriginalCameraClearFlags = mARCamera.clearFlags;
        mARCamera.clearFlags = CameraClearFlags.Depth;

        // instanciate BackgroundCamera
        mBackgroundCameraInstance = backgroundCamera;

        // make sure the correct prefab has been attached:
        mBgRenderingTexBehaviour =
            mBackgroundCameraInstance.GetComponentInChildren<BGRenderingBehaviour>();
        if (mBgRenderingTexBehaviour == null)
        {
            Debug.LogError("Instanciated Prefab does not contain VideoTextureBehaviour!");
        }
        else
        {
            mOriginalCameraCullMask = mARCamera.cullingMask;
            // set mask to cull away bg rendering behaviour and texture buffer layer
            mARCamera.cullingMask &= ~(1 << mBgRenderingTexBehaviour.gameObject.layer);
            mARCamera.cullingMask &= ~(1 << mRenderTextureLayer);

            // load the webcam profiles
            WebCamProfile profiles = new WebCamProfile();

            if (QCARRuntimeUtilities.IsQCAREnabled() && (WebCamTexture.devices.Length > 0))
            {
                // check if selected web cam shows up in list of available devices:
                bool selectedWebCamAvailable = false;
                foreach (WebCamDevice webCamDevice in WebCamTexture.devices)
                    if (webCamDevice.name.Equals(webcamDeviceName))
                        selectedWebCamAvailable = true;

                // if it was not found, default to first available camera
                if (!selectedWebCamAvailable) webcamDeviceName = WebCamTexture.devices[0].name;

                mWebCamProfile = profiles.GetProfile(webcamDeviceName);

                // create webcam texture adaptor
                mWebCamTexture = new WebCamTexAdaptorImpl(webcamDeviceName, mWebCamProfile.RequestedFPS,
                                                            mWebCamProfile.RequestedTextureSize);
            }
            else
            {
                // no webcam connected, use default profile and null implementation for webcam
                mWebCamProfile = profiles.Default;

                // create null webcam implementation
                mWebCamTexture = new NullWebCamTexAdaptor(mWebCamProfile.RequestedFPS, mWebCamProfile.RequestedTextureSize);
            }

            // override the texture created by the the VideoTextureBehaviour
            mBgRenderingTexBehaviour.SetFlipHorizontally(flipHorizontally);

            mFlipHorizontally = flipHorizontally;
        }
#endif
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    public void StartCamera()
    {
        if (!mWebCamTexture.IsPlaying)
        {
            mWebCamTexture.Play();
        }
    }
    
    public void StopCamera()
    {
        mWebCamTexture.Stop();
    }

    public Color32[] GetPixels32AndBufferFrame(int frameIndex)
    {
        // get a buffered image of this frame:
        RenderTexture bufferedFrame = mTextureRenderer.Render();
        mBufferedFrames.Enqueue(new BufferedFrame(){frame = bufferedFrame, frameIndex = frameIndex});

        RenderTexture.active = bufferedFrame;
        mBufferReadTexture.ReadPixels(mReadPixelsRect, 0, 0, false);

        return mBufferReadTexture.GetPixels32();
    }

    public void SetFrameIndex(int frameIndex)
    {
        int currentFrameIndex = mLastFrameIdx;
        if (currentFrameIndex != frameIndex)
        {
            while(currentFrameIndex != frameIndex)
            {
                if (mBufferedFrames.Count == 0) break;
                BufferedFrame nextFrame = mBufferedFrames.Peek();
                currentFrameIndex = nextFrame.frameIndex;
                if (currentFrameIndex == frameIndex)
                {
                    // we found the correct frame
                    RenderFrame(nextFrame.frame);
                    break;
                }
                else
                {
                    mBufferedFrames.Dequeue();
                    RenderTexture.ReleaseTemporary(nextFrame.frame);
                }
            }
            mLastFrameIdx = frameIndex;
        }
    }


    public CameraDevice.VideoModeData GetVideoMode()
    {
        return mVideoModeData;
    }

    
    public QCARRenderer.VideoTextureInfo GetVideoTextureInfo()
    {
        return mVideoTextureInfo;
    }

    // checks if the screen size has chabned in the editor after bg info is available
    public bool IsRendererDirty()
    {
        bool isDirty = IsTextureSizeAvailable &&
                       (mLastScreenWidth != Screen.width || mLastScreenHeight != Screen.height);
        if (isDirty)
        {
            mLastScreenWidth = Screen.width;
            mLastScreenHeight = Screen.height;
        }

        return isDirty;
    }
    
#if UNITY_EDITOR

    public void OnDestroy()
    {
        // set the original camera clear flags again:
        mARCamera.clearFlags = mOriginalCameraClearFlags;
        mARCamera.cullingMask = mOriginalCameraCullMask;

        // texture will no longer be available
        IsTextureSizeAvailable = false;

        // destroy textureBuffer gameobjects
        if (mTextureRenderer != null)
            mTextureRenderer.Destroy();
    }

    // wait for the first web cam frame to set config structs with correct size values
    // (mWebCamTexture.width and height are set to default values before the first frame is captured)
    public void Update()
    {
        if (!IsTextureSizeAvailable && mWebCamTexture.DidUpdateThisFrame)
        {
            QCARRenderer.Vec2I resampledSize = mWebCamProfile.ResampledTextureSize;

                mVideoModeData = new CameraDevice.VideoModeData
                                     {
                                         width = resampledSize.x,
                                         height = resampledSize.y,
                                         frameRate = mWebCamProfile.RequestedFPS // real fps not known, but not used in Unity any way...
                                     };

                mVideoTextureInfo = new QCARRenderer.VideoTextureInfo
                                        {
                                            imageSize = resampledSize,
                                            textureSize = resampledSize
                                        };

                mTextureRenderer = new TextureRenderer(mWebCamTexture.Texture, mRenderTextureLayer, resampledSize);
                mBufferReadTexture = new Texture2D(resampledSize.x, resampledSize.y);
                mReadPixelsRect = new Rect(0, 0, resampledSize.x, resampledSize.y);

            IsTextureSizeAvailable = true;
        }

        // turn on and off BGRenderingBehaviour depending if Background Rendering has been enabled or not:
        mBgRenderingTexBehaviour.CheckAndSetActive(QCARRenderer.Instance.DrawVideoBackground);
    }

#endif

    #endregion // PUBLIC_METHODS
    }
