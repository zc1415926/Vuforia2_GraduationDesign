/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This class represents a service that retrieves targets using cloud-based recognition
/// </summary>
public class TargetFinderImpl : TargetFinder
{
    #region NESTED

    /// <summary>
    /// This struct stores the state of the ImageTargetBuilder, including the mode, build progress and frame quality in search mode.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TargetFinderState
    {
        public int IsRequesting;
        [MarshalAs(UnmanagedType.SysInt)]
        public UpdateState UpdateState;
        public int ResultCount;
    }

    /// <summary>
    /// This struct describes a new search result. If the new target cannot be tracked, TrackableSource will be null.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct InternalTargetSearchResult
    {
        public IntPtr TargetNamePtr;
        public IntPtr UniqueTargetIdPtr;
        public float TargetSize;
        public IntPtr MetaDataPtr;
        public IntPtr TargetSearchResultPtr;
        public byte TrackingRating;
    }

    #endregion // NESTED



    #region PRIVATE_MEMBER_VARIABLES

    private IntPtr mTargetFinderStatePtr;
    private TargetFinderState mTargetFinderState;
    private List<TargetSearchResult> mNewResults;
    private Dictionary<int, ImageTarget> mImageTargets;

    #endregion // PRIVATE_MEMBER_VARIABLES


        

    #region CONSTRUCTION

    public TargetFinderImpl()
    {
        mTargetFinderState = new TargetFinderState();

        // create the state pointer with the correct size
        mTargetFinderStatePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TargetFinderState)));
        Marshal.StructureToPtr(mTargetFinderState, mTargetFinderStatePtr, false);
        mImageTargets = new Dictionary<int, ImageTarget>();
    }

    ~TargetFinderImpl()
    {
        Marshal.FreeHGlobal(mTargetFinderStatePtr);
        mTargetFinderStatePtr = IntPtr.Zero;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS


    /// <summary>
    /// Starts initialization of the cloud-based recognition system.
    ///
    /// Initialization of the cloud-based recognition system may take significant
    /// time and is thus handled in a background process. Use GetInitState() to
    /// query the initialization progress and result. Pass in the user/password
    /// for authenticating with the cloud reco server.
    /// </summary>
    public override bool StartInit(string userAuth, string secretAuth)
    {
        return QCARWrapper.Instance.TargetFinderStartInit(userAuth, secretAuth) == 1;
    }

    /// <summary>
    /// Returns the current state of the initialization process
    ///
    /// Returns INIT_SUCCESS if the cloud-based recognition system was
    /// initialized successfully. Initialization requires a network connection
    /// to be available on the device, otherwise INIT_ERROR_NO_NETWORK_CONNECTION
    /// is returned. If the cloud-based recognition service is not available this
    /// function will return INIT_ERROR_SERVICE_NOT_AVAILABLE. Returns
    /// INIT_DEFAULT if initialization has not been started. Returns INIT_RUNNING
    /// if the initialization process has not completed.
    /// </summary>
    public override InitState GetInitState()
    {
        return (InitState)QCARWrapper.Instance.TargetFinderGetInitState();
    }

    /// <summary>
    /// Deinitializes the cloud-based recognition system
    /// </summary>
    public override bool Deinit()
    {
        return QCARWrapper.Instance.TargetFinderDeinit() == 1;
    }

    /// <summary>
    /// Starts cloud recognition
    ///
    /// Starts continuous recognition of Targets from the cloud.
    /// Use updateSearchResult() and getResult() to retrieve search matches.
    /// </summary>
    public override bool StartRecognition()
    {
        return QCARWrapper.Instance.TargetFinderStartRecognition() == 1;
    }

    /// <summary>
    /// Stops cloud recognition
    /// </summary>
    public override bool Stop()
    {
        return QCARWrapper.Instance.TargetFinderStop() == 1;
    }

    /// <summary>
    /// Sets the base color of the scanline in the scanning UI
    /// </summary>
    public override void SetUIScanlineColor(Color color)
    {
        QCARWrapper.Instance.TargetFinderSetUIScanlineColor(color.r, color.g, color.b);
    }

    /// <summary>
    /// Sets the base color of the points in the scanning UI
    /// </summary>
    public override void SetUIPointColor(Color color)
    {
        QCARWrapper.Instance.TargetFinderSetUIPointColor(color.r, color.g, color.b);
    }

    /// <summary>
    /// Returns true if the TargetFinder is in 'requesting' mode
    ///
    /// When in 'requesting' mode the TargetFinder has issued a search 
    /// query to the recognition server and is waiting for the results.
    /// </summary>
    public override bool IsRequesting()
    {
        return mTargetFinderState.IsRequesting == 1;
    }

    /// <summary>
    /// Update cloud reco results
    ///
    /// Clears and rebuilds the list of target search results with results found
    /// since the last call to updateSearchResults().
    /// Also refreshes the IsRequesting flag. 
    /// Returns the status code  UPDATE_RESULTS_AVAILABLE if new search results have been found.
    /// </summary>
    public override UpdateState Update()
    {
        // update the TargetFinder internally to find new results.
        QCARWrapper.Instance.TargetFinderUpdate(mTargetFinderStatePtr);
        mTargetFinderState = (TargetFinderState)Marshal.PtrToStructure(mTargetFinderStatePtr, typeof(TargetFinderState));

        if (mTargetFinderState.ResultCount>0)
        {
            // there are new results, poll them from native and parse them.
            IntPtr newResultsPtr  = Marshal.AllocHGlobal(Marshal.SizeOf(
                    typeof(InternalTargetSearchResult)) * mTargetFinderState.ResultCount);

            if (QCARWrapper.Instance.TargetFinderGetResults(newResultsPtr, mTargetFinderState.ResultCount) != 1)
            {
                Debug.LogError("TargetFinder: Could not retrieve new results!");
                return UpdateState.UPDATE_NO_MATCH;
            }

            mNewResults = new List<TargetSearchResult>();
            for (int i = 0; i < mTargetFinderState.ResultCount; i++)
            {
                IntPtr resultPtr = new IntPtr(newResultsPtr.ToInt32() + i *
                        Marshal.SizeOf(typeof(QCARManagerImpl.TrackableResultData)));
                InternalTargetSearchResult internalTargetSearchResult = (InternalTargetSearchResult)
                        Marshal.PtrToStructure(resultPtr, typeof(InternalTargetSearchResult));
                
                // store the results in a nice format
                mNewResults.Add(new TargetSearchResult
                {
                                        TargetName = Marshal.PtrToStringAnsi(internalTargetSearchResult.TargetNamePtr),
                                        UniqueTargetId = Marshal.PtrToStringAnsi(internalTargetSearchResult.UniqueTargetIdPtr),
                                        TargetSize = internalTargetSearchResult.TargetSize,
                                        MetaData = Marshal.PtrToStringAnsi(internalTargetSearchResult.MetaDataPtr),
                                        TrackingRating = internalTargetSearchResult.TrackingRating,
                                        TargetSearchResultPtr = internalTargetSearchResult.TargetSearchResultPtr
                                    });
            }

            Marshal.FreeHGlobal(newResultsPtr);
        }

        return mTargetFinderState.UpdateState;
    }

    /// <summary>
    /// Returns new search results
    ///
    /// Earlier search result instances are destroyed when UpdateSearchResults
    /// is called. 
    /// </summary>
    public override IEnumerable<TargetSearchResult> GetResults()
    {
        return mNewResults;
    }


    /// <summary>
    /// Enable this search result for tracking
    ///
    /// Creates an ImageTarget for local detection and tracking of this target
    /// and returns a new ImageTargetBehaviour attached to a new game object with the given name.
    /// Note that this call may result in an earlier ImageTarget that was enabled for
    /// tracking to be destroyed, including its ImageTargetBehaviour. 
    /// Thus it is not advised to hold a pointer to an
    /// ealier created ImageTarget after calling enableTracking again. Returns
    /// NULL if the target failed to be enabled for tracking.
    /// </summary>
    public override ImageTargetBehaviour EnableTracking(TargetSearchResult result, string gameObjectName)
    {
        GameObject gameObject = new GameObject(gameObjectName);
        return EnableTracking(result, gameObject);
    }


    /// <summary>
    /// Enable this search result for tracking
    ///
    /// Creates an ImageTarget for local detection and tracking of this target.
    /// If the given game object has no ImageTargetBehaviour, a new one will be created.
    /// Note that this call may result in an earlier ImageTarget that was enabled for
    /// tracking to be destroyed, including its ImageTargetBehaviour. 
    /// Thus it is not advised to hold a pointer to an
    /// ealier created ImageTarget after calling enableTracking again. Returns
    /// NULL if the target failed to be enabled for tracking.
    /// </summary>
    public override ImageTargetBehaviour EnableTracking(TargetSearchResult result, GameObject gameObject)
    {
        // create a new trackable in native
        IntPtr imageTargetPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ImageTargetData)));
        int newTrackableCount = QCARWrapper.Instance.TargetFinderEnableTracking(result.TargetSearchResultPtr, imageTargetPtr);

        ImageTargetData imageTargetData = (ImageTargetData)
                Marshal.PtrToStructure(imageTargetPtr, typeof(ImageTargetData));
        Marshal.FreeHGlobal(imageTargetPtr);

        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();

        // if successful, create a new trackable here and add it to internal collection
        ImageTargetBehaviour imageTargetBehaviour = null;
        if (imageTargetData.id == -1)
        {
            Debug.LogError("TargetSearchResult " + result.TargetName + " could not be enabled for tracking.");
        }
        else
        {
            ImageTarget newImageTarget = new CloudRecoImageTargetImpl(result.TargetName, imageTargetData.id, imageTargetData.size);

            // Add newly created Image Target to dictionary.
            mImageTargets[imageTargetData.id] = newImageTarget;

            // Find or create ImageTargetBehaviour for this ImageTarget:
            imageTargetBehaviour = stateManager.FindOrCreateImageTargetBehaviourForTrackable(newImageTarget, gameObject);
        }

        // get a list of currently existing trackables
        IntPtr imageTargetIdArrayPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * newTrackableCount);
        QCARWrapper.Instance.TargetFinderGetImageTargets(imageTargetIdArrayPtr, newTrackableCount);

        List<int> targetIds = new List<int>();
        for (int i = 0; i < newTrackableCount; i++)
        {
            IntPtr targetIdPtr = new IntPtr(imageTargetIdArrayPtr.ToInt32() + i * Marshal.SizeOf(typeof(int)));
            int targetID = Marshal.ReadInt32(targetIdPtr);
            targetIds.Add(targetID);
        }

        Marshal.FreeHGlobal(imageTargetIdArrayPtr);

        // delete those targets that no longer exist
        foreach (ImageTarget imageTarget in mImageTargets.Values.ToArray())
        {
            if (!targetIds.Contains(imageTarget.ID))
            {
                stateManager.DestroyTrackableBehavioursForTrackable(imageTarget);
                mImageTargets.Remove(imageTarget.ID);
            }
        }


        return imageTargetBehaviour;
    }


    /// <summary>
    /// Clears all targets enabled for tracking
    ///
    /// Destroys all ImageTargets that have been created via EnableTracking().
    /// </summary>
    public override void ClearTrackables(bool destroyGameObjects = true)
    {
        // call native function
        QCARWrapper.Instance.TargetFinderClearTrackables();

        // destroy ImageTargetBehaviours in StateManager
        StateManagerImpl stateManager = (StateManagerImpl)TrackerManager.Instance.GetStateManager();
        foreach (ImageTarget imageTarget in mImageTargets.Values)
        {
            stateManager.DestroyTrackableBehavioursForTrackable(imageTarget, destroyGameObjects);
        }

        // clear internal collection
        mImageTargets.Clear();
    }


    /// <summary>
    /// Returns the ImageTargets currently enabled for tracking.
    /// </summary>
    public override IEnumerable<ImageTarget> GetImageTargets()
    {
        return mImageTargets.Values;
    }

    #endregion // PUBLIC_METHODS
}
