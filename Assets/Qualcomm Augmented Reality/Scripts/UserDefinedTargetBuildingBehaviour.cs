/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This Component can be used to create new ImageTargets at runtime. It can be configured to start scanning automatically
/// or via a call from an external script.
/// Registered event handlers will be informed of changes in the frame quality as well as new TrackableSources
/// </summary>
public class UserDefinedTargetBuildingBehaviour : MonoBehaviour, ITrackerEventHandler
{
    #region PRIVATE_MEMBER_VARIABLES

    // ImageTracker reference to avoid lookups
    private ImageTracker mImageTracker;
    // last frame quality
    private ImageTargetBuilder.FrameQuality mLastFrameQuality = ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE;
    // if we are currently in scanning mode
    private bool mCurrentlyScanning = false;
    private bool mWasScanningBeforeDisable = false;
    // if we are currently building a new target internally
    private bool mCurrentlyBuilding = false;
    private bool mWasBuildingBeforeDisable = false;
    // if the OnInitialized callback has been called
    private bool mOnInitializedCalled = false;
    // a list of registered handlers that will be notified of new events
    private readonly List<IUserDefinedTargetEventHandler> mHandlers = new List<IUserDefinedTargetEventHandler>();

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region EXPOSED_PUBLIC_VARIABLES

    /// <summary>
    /// if the tracker should be stopped when scanning is started and resumed when scanning is stopped
    /// </summary>
    public bool StopTrackerWhileScanning = false;

    /// <summary>
    /// if this is set to true, scanning will start automatically when this component is activated
    /// </summary>
    public bool StartScanningAutomatically = false;

    /// <summary>
    /// if scanning should be stopped automatically when a new target is built
    /// </summary>
    public bool StopScanningWhenFinshedBuilding = false;

    #endregion // EXPOSED_PUBLIC_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    /// Registers an event handler with this UserDefinedTargetBuildingBehaviour which will be called on events
    /// </summary>
    public void RegisterEventHandler(IUserDefinedTargetEventHandler eventHandler)
    {
        mHandlers.Add(eventHandler);

        // in case initialization has already happened:
        if (mOnInitializedCalled)
            eventHandler.OnInitialized();
    }


    /// <summary>
    /// Unregisters an event handler
    /// </summary>
    public bool UnregisterEventHandler(IUserDefinedTargetEventHandler eventHandler)
    {
        return mHandlers.Remove(eventHandler);
    }

    /// <summary>
    /// Starts scanning the current camera image for features. 
    /// This will trigger callbacks to the registered event handlers whenever the
    /// frame quality changes.
    /// If 'StopTrackerWhileScanning' is set, this will stop the ImageTracker
    /// </summary>
    public void StartScanning()
    {
        if (mImageTracker != null)
        {
            if (StopTrackerWhileScanning)
                mImageTracker.Stop();

            mImageTracker.ImageTargetBuilder.StartScan();

            mCurrentlyScanning = true;
        }

        SetFrameQuality(ImageTargetBuilder.FrameQuality.FRAME_QUALITY_LOW);
    }

    /// <summary>
    /// This will start building a new target and report back to the event handlers as soon
    /// as a new TrackableSource is available.
    /// </summary>
    public void BuildNewTarget(string targetName, float sceenSizeWidth)
    {
        mCurrentlyBuilding = true;
        mImageTracker.ImageTargetBuilder.Build(targetName, sceenSizeWidth);
    }

    /// <summary>
    /// Stops scanning, event handlers will be informed.
    /// If 'StopTrackerWhileScanning' is set, this will resume the ImageTracker
    /// </summary>
    public void StopScanning()
    {
        mCurrentlyScanning = false;

        mImageTracker.ImageTargetBuilder.StopScan();

        if (StopTrackerWhileScanning)
            mImageTracker.Start();

        SetFrameQuality(ImageTargetBuilder.FrameQuality.FRAME_QUALITY_NONE);
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private void SetFrameQuality(ImageTargetBuilder.FrameQuality frameQuality)
    {
        if (frameQuality != mLastFrameQuality)
        {
            // notify the event handlers:
            foreach (IUserDefinedTargetEventHandler eventHandler in mHandlers)
                eventHandler.OnFrameQualityChanged(frameQuality);

            mLastFrameQuality = frameQuality;
        }
    }

    #endregion // PRIVATE_METHODS



    #region UNITY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        // register for initialized callback at QCARBehaviour
        QCARBehaviour qcarBehaviour = (QCARBehaviour)FindObjectOfType(typeof(QCARBehaviour));
        if (qcarBehaviour)
        {
            qcarBehaviour.RegisterTrackerEventHandler(this);
        }
    }

    void Update()
    {
        if (mOnInitializedCalled)
        {
            // if we are currently scanning, update the current frame quality
            if (mCurrentlyScanning)
                SetFrameQuality(mImageTracker.ImageTargetBuilder.GetFrameQuality());

            // when building a new target, check if the trackablesource is already available
            if (mCurrentlyBuilding)
            {
                TrackableSource trackableSource = mImageTracker.ImageTargetBuilder.GetTrackableSource();
                if (trackableSource != null)
                {
                    mCurrentlyBuilding = false;

                    // notify the event handlers:
                    foreach (IUserDefinedTargetEventHandler eventHandler in mHandlers)
                        eventHandler.OnNewTrackableSource(trackableSource);
                    
                    if (StopScanningWhenFinshedBuilding)
                        StopScanning();
                }
            }
        }
    }
    
// OnApplicaitonPause does not work reliably on desktop OS's - on windows it never gets called,
// on Mac only if the window focus is lost and Play mode was paused (or resumed!) before.
#if !UNITY_EDITOR

    // call disable/enable also on pause to recover previous state
    void OnApplicationPause(bool pause)
    {
        if (pause)
            OnDisable();
        else
            OnEnable();
    }

#endif

    void OnEnable()
    {
        if (mOnInitializedCalled)
        {
            // reload previous state
            mCurrentlyScanning = mWasScanningBeforeDisable;
            mCurrentlyBuilding = mWasBuildingBeforeDisable;

            // resume scanning
            if (mWasScanningBeforeDisable)
                StartScanning();
        }
    }

    void OnDisable()
    {
        if (mOnInitializedCalled)
        {
            // remember previous state
            mWasScanningBeforeDisable = mCurrentlyScanning;
            mWasBuildingBeforeDisable = mCurrentlyBuilding;

            // stop scanning
            if (mCurrentlyScanning)
                StopScanning();
        }
    }


    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region ITrackerEventHandler_IMPLEMENTATION

    /// <summary>
    /// Initialize after QCAR
    /// </summary>
    public void OnInitialized()
    {
        mOnInitializedCalled = true;

        // look up the ImageTracker once and store a reference
        mImageTracker = (ImageTracker)TrackerManager.Instance.GetTracker(
                                        Tracker.Type.IMAGE_TRACKER);

        // notify the event handlers:
        foreach (IUserDefinedTargetEventHandler eventHandler in mHandlers)
            eventHandler.OnInitialized();

        // start scanning automatically if set from inspector:
        if (StartScanningAutomatically)
            StartScanning();
    }

    public void OnTrackablesUpdated()
    {
        // not used here
    }

    #endregion // ITrackerEventHandler_IMPLEMENTATION
}
