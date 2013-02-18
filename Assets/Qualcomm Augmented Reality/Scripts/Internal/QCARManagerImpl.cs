/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class QCARManagerImpl : QCARManager
{
    #region NESTED

    // This struct stores 3D pose information as a position-vector,
    // orientation-Quaternion pair. The pose is given relatively to the camera.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PoseData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Vector3 position;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Quaternion orientation;
    }

    // This struct stores general data about a trackable result like its 3D pose, its status
    // and its unique id.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TrackableResultData
    {
        public PoseData pose;
        public TrackableBehaviour.Status status;
        public int id;
    }

    // This struct stores Virtual Button data like its current status (pressed
    // or not pressed) and its unique id.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VirtualButtonData
    {
        public int id;
        public int isPressed;
    }

    // This struct stores data of an image header. It includes the width and
    // height of the image, the byte stride in the buffer, the buffer size
    // (which can differ from the image size e.g. when image is converted to a
    // power of two size) and the format of the image
    // (e.g. RGB565, grayscale, etc.).
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ImageHeaderData
    {
        public int width;
        public int height;
        public int stride;
        public int bufferWidth;
        public int bufferHeight;
        public int format;
        public int reallocate;
        public int updated;
        public IntPtr data;
    }

    // This struct stores information about the state of the frame that was last processed by QCAR
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct FrameState
    {
        public int numTrackableResults;
        public int numVirtualButtonResults;
        public int frameIndex;
        public IntPtr trackableDataArray;
        public IntPtr vbDataArray;
    }

    #endregion // NESTED



    #region PROPERTIES

    // World Center Mode setting on the ARCamera
    public override QCARBehaviour.WorldCenterMode WorldCenterMode
    {
        set { mWorldCenterMode = value; }
        get { return mWorldCenterMode; }
    }

    // World Center setting on the ARCamera
    public override TrackableBehaviour WorldCenter
    {
        set { mWorldCenter = value; }
        get { return mWorldCenter; }
    }

    // A handle to the ARCamera object
    public override Camera ARCamera
    {
        set { mARCamera = value; }
        get { return mARCamera; }
    }

    // True to have QCAR render the video background image natively
    // False to bind the video background to the texture set in
    // QCARRenderer.SetVideoBackgroundTextureID
    public override bool DrawVideoBackground
    {
        set { mDrawVideobackground = value; }
        get { return mDrawVideobackground; }
    }

    /// <summary>
    /// returns true once the QCARManager has been initialized
    /// </summary>
    public override bool Initialized
    {
        get { return mInitialized; }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    private QCARBehaviour.WorldCenterMode mWorldCenterMode;
    private TrackableBehaviour mWorldCenter = null;
    private Camera mARCamera = null;
    private TrackableResultData[] mTrackableResultDataArray = null;

    private LinkedList<int> mTrackableFoundQueue = new LinkedList<int>();

    private IntPtr mImageHeaderData = IntPtr.Zero;
    private int mNumImageHeaders = 0;

    private bool mDrawVideobackground = true;

    // frame index of the next injected frame when in emulator mode
    private int mInjectedFrameIdx = 0;

    // ptr to index of frame last processed by Vuforia
    private IntPtr mLastProcessedFrameStatePtr = IntPtr.Zero;

    private bool mInitialized = false;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    // Initialization
    public override bool Init()
    {
        mTrackableResultDataArray = new TrackableResultData[0];

        mTrackableFoundQueue = new LinkedList<int>();

        mImageHeaderData = IntPtr.Zero;
        mNumImageHeaders = 0;

        mLastProcessedFrameStatePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FrameState)));
        QCARWrapper.Instance.InitFrameState(mLastProcessedFrameStatePtr);

        InitializeTrackableContainer(0);

        mInitialized = true;

        return true;
    }


    // Process the camera image and tracking data for this frame
    public void Update(ScreenOrientation counterRotation)
    {
        // enable "fake tracking" if running in the free editor version 
        // that does not support native plugins
        if (!QCARRuntimeUtilities.IsQCAREnabled())
        {
            UpdateTrackablesEditor();
            return;
        }

        // Prepare the camera image container
        UpdateImageContainer();

        if (QCARRuntimeUtilities.IsPlayMode())
        {
            CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
            if (cameraDeviceImpl.WebCam.DidUpdateThisFrame)
            {
                InjectCameraFrame();
            }
        }

        // Draw the video background or update the video texture
        // Also retrieve registered camera images for this frame
        QCARWrapper.Instance.UpdateQCAR(mImageHeaderData, mNumImageHeaders,
            mDrawVideobackground ? 0 : 1,
            mLastProcessedFrameStatePtr, (int)counterRotation);

        FrameState frameState = (FrameState)Marshal.PtrToStructure(mLastProcessedFrameStatePtr, typeof(FrameState));
        // Reinitialize the trackable data container if required:
        InitializeTrackableContainer(frameState.numTrackableResults);

        // Handle the camera image data
        UpdateCameraFrame();

        // Handle the trackable data
        UpdateTrackers(frameState);

        if (QCARRuntimeUtilities.IsPlayMode())
        {
            // read out the index of the last processed frame
            CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
            cameraDeviceImpl.WebCam.SetFrameIndex(frameState.frameIndex);
        }
    }


    // Free globally allocated containers
    public override void Deinit()
    {
        if (mInitialized)
        {
            Marshal.FreeHGlobal(mImageHeaderData);
            QCARWrapper.Instance.DeinitFrameState(mLastProcessedFrameStatePtr);
            Marshal.FreeHGlobal(mLastProcessedFrameStatePtr);

            mInitialized = false;
        }
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Initialize the container for retrieving tracking data from native
    private void InitializeTrackableContainer(int numTrackableResults)
    {
        if (mTrackableResultDataArray.Length != numTrackableResults)
        {
            mTrackableResultDataArray = new TrackableResultData[numTrackableResults];

            Debug.Log("Num trackables detected: " + numTrackableResults);
        }
    }


    // Unmarshal and process the tracking data
    private void UpdateTrackers(FrameState frameState)
    {
        // Unmarshal the trackable data
        // Take our array of unmanaged data from native and create an array of
        // TrackableResultData structures to work with (one per trackable, regardless
        // of whether or not that trackable is visible this frame).
        for (int i = 0; i < frameState.numTrackableResults; i++)
        {
            IntPtr trackablePtr = new IntPtr(frameState.trackableDataArray.ToInt32() + i *
                    Marshal.SizeOf(typeof(TrackableResultData)));
            TrackableResultData trackableResultData = (TrackableResultData)
                    Marshal.PtrToStructure(trackablePtr, typeof(TrackableResultData));
            mTrackableResultDataArray[i] = trackableResultData;
        }

        // Add newly found Trackables to the queue, remove lost ones
        // We keep track of the order in which Trackables become visible for the
        // AUTO World Center mode. This keeps the camera from jumping around in the
        // scene too much.
        foreach (TrackableResultData trackableData in mTrackableResultDataArray)
        {
            // We found a trackable (or set of Trackables) that match this id
            if ((trackableData.status == TrackableBehaviour.Status.DETECTED
                 || trackableData.status ==
                 TrackableBehaviour.Status.TRACKED))
            {
                if (!mTrackableFoundQueue.Contains(trackableData.id))
                {
                    // The trackable just became visible, add it to the queue
                    mTrackableFoundQueue.AddLast(trackableData.id);
                }
            }
            else
            {
                if (mTrackableFoundQueue.Contains(trackableData.id))
                {
                    // The trackable just disappeared, remove it from the queue
                    mTrackableFoundQueue.Remove(trackableData.id);
                }
            }
        }
        
        // now remove those from the TrackableFoundQueue that were not in this frame's results:
        List<int> trackableFoundQueueCopy = new List<int>(mTrackableFoundQueue);
        foreach (int id in trackableFoundQueueCopy)
        {
            // check if the trackable is in this frame's results:
            if (Array.Exists(mTrackableResultDataArray, tr => tr.id == id))
                break;

            // not in the results, remove it from the queue
            mTrackableFoundQueue.Remove(id);
        }

        StateManagerImpl stateManager = (StateManagerImpl)
            TrackerManager.Instance.GetStateManager();

        // The "scene origin" is only used in world center mode auto or user.
        int originTrackableID = -1;

        if (mWorldCenterMode == QCARBehaviour.WorldCenterMode.USER &&
            mWorldCenter != null)
        {
            originTrackableID = mWorldCenter.Trackable.ID;
        }
        else if (mWorldCenterMode == QCARBehaviour.WorldCenterMode.AUTO)
        {
            stateManager.RemoveDisabledTrackablesFromQueue(ref mTrackableFoundQueue);
            if (mTrackableFoundQueue.Count > 0)
            {
                originTrackableID = mTrackableFoundQueue.First.Value;
            }
        }

        // Update the Camera pose before Trackable poses are updated.
        stateManager.UpdateCameraPose(mARCamera, mTrackableResultDataArray, originTrackableID);

        // Update the Trackable poses.
        stateManager.UpdateTrackablePoses(mARCamera, mTrackableResultDataArray, originTrackableID);

        // Update Virtual Button states.
        stateManager.UpdateVirtualButtons(frameState.numVirtualButtonResults, frameState.vbDataArray);
    }


    // Simulate tracking in the editor
    private void UpdateTrackablesEditor()
    {
        // When running within the Unity editor without emulation mode:
        TrackableBehaviour[] trackableBehaviours = (TrackableBehaviour[])
                UnityEngine.Object.FindObjectsOfType(typeof(TrackableBehaviour));

        // Simulate all Trackables were tracked successfully:    
        foreach (TrackableBehaviour trackable in trackableBehaviours)
        {
            if (trackable.enabled)
            {
                trackable.OnTrackerUpdate(TrackableBehaviour.Status.TRACKED);
            }
        }
    }

    // Update the image container for the currently registered formats
    private void UpdateImageContainer()
    {
        CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;

        // Reallocate the data container if the number of requested images has
        // changed, or if the container is not allocated
        if (mNumImageHeaders != cameraDeviceImpl.GetAllImages().Count ||
           (cameraDeviceImpl.GetAllImages().Count > 0 && mImageHeaderData == IntPtr.Zero))
        {

            mNumImageHeaders = cameraDeviceImpl.GetAllImages().Count;

            Marshal.FreeHGlobal(mImageHeaderData);
            mImageHeaderData = Marshal.AllocHGlobal(Marshal.SizeOf(
                                typeof(ImageHeaderData)) * mNumImageHeaders);
        }

        // Update the image info:
        int i = 0;
        foreach (ImageImpl image in cameraDeviceImpl.GetAllImages().Values)
        {
            IntPtr imagePtr = new IntPtr(mImageHeaderData.ToInt32() + i *
                   Marshal.SizeOf(typeof(ImageHeaderData)));

            ImageHeaderData imageHeader = new ImageHeaderData();
            imageHeader.width = image.Width;
            imageHeader.height = image.Height;
            imageHeader.stride = image.Stride;
            imageHeader.bufferWidth = image.BufferWidth;
            imageHeader.bufferHeight = image.BufferHeight;
            imageHeader.format = (int)image.PixelFormat;
            imageHeader.reallocate = 0;
            imageHeader.updated = 0;
            imageHeader.data = image.UnmanagedData;

            Marshal.StructureToPtr(imageHeader, imagePtr, false);
            ++i;
        }
    }


    // Unmarshal the camera images for this frame
    private void UpdateCameraFrame()
    {
        // Unmarshal the image data:
        int i = 0;
        CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
        foreach (ImageImpl image in cameraDeviceImpl.GetAllImages().Values)
        {
            IntPtr imagePtr = new IntPtr(mImageHeaderData.ToInt32() + i *
                   Marshal.SizeOf(typeof(ImageHeaderData)));
            ImageHeaderData imageHeader = (ImageHeaderData)
                Marshal.PtrToStructure(imagePtr, typeof(ImageHeaderData));

            // Copy info back to managed Image instance:
            image.Width = imageHeader.width;
            image.Height = imageHeader.height;
            image.Stride = imageHeader.stride;
            image.BufferWidth = imageHeader.bufferWidth;
            image.BufferHeight = imageHeader.bufferHeight;
            image.PixelFormat = (Image.PIXEL_FORMAT) imageHeader.format;

            // Reallocate if required:
            if (imageHeader.reallocate == 1)
            {
                image.Pixels = new byte[QCARWrapper.Instance.QcarGetBufferSize(image.BufferWidth,
                                                    image.BufferHeight,
                                                    (int)image.PixelFormat)];

                Marshal.FreeHGlobal(image.UnmanagedData);

                image.UnmanagedData = Marshal.AllocHGlobal(QCARWrapper.Instance.QcarGetBufferSize(image.BufferWidth,
                                    image.BufferHeight,
                                    (int)image.PixelFormat));

                // Note we don't copy the data this frame as the unmanagedVirtualButtonBehaviour
                // buffer was not filled.
            }
            else if (imageHeader.updated == 1)
            {
                // Copy data:
                image.CopyPixelsFromUnmanagedBuffer();
            }

            ++i;
        }
    }

    // gets a snapshot from the 
    private void InjectCameraFrame()
    {
        CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
        Color32[] pixels = cameraDeviceImpl.WebCam.GetPixels32AndBufferFrame(mInjectedFrameIdx);
        GCHandle pixelHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        IntPtr pixelPointer = pixelHandle.AddrOfPinnedObject();
        int width = cameraDeviceImpl.WebCam.ActualWidth;
        int height = cameraDeviceImpl.WebCam.ActualHeight;

        // add a camera frame - it always has to be rotated and flipped by default
        QCARWrapper.Instance.QcarAddCameraFrame(pixelPointer, width, height, (int)Image.PIXEL_FORMAT.RGBA8888, 4 * width, mInjectedFrameIdx, cameraDeviceImpl.WebCam.FlipHorizontally ? 1 : 0);
        mInjectedFrameIdx++;
        pixelPointer = IntPtr.Zero;
        pixelHandle.Free();
    }

    #endregion // PRIVATE_METHODS
}
