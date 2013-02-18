/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This class represents a service that retrieves targets using cloud-based recognition
/// </summary>
public abstract class TargetFinder
{
    #region NESTED
    
    /// <summary>
    /// Status codes returned by the init() function
    /// </summary>
    public enum InitState
    {
        INIT_DEFAULT = 0,                        ///< Initialization has not started
        INIT_RUNNING = 1,                        ///< Initialiation is running
        INIT_SUCCESS = 2,                        ///< Initialization completed successfully
        INIT_ERROR_NO_NETWORK_CONNECTION = -1,   ///< No network connection
        INIT_ERROR_SERVICE_NOT_AVAILABLE = -2    ///< Service is not available   
    }

    /// <summary>
    /// Status codes returned by the updateSearchResults() function
    /// </summary>
    public enum UpdateState
    {
        UPDATE_NO_MATCH = 0,                     ///< No matches since the last update
        UPDATE_NO_REQUEST = 1,                   ///< No recognition request since the last update
        UPDATE_RESULTS_AVAILABLE = 2,            ///< New search results have been found
        UPDATE_ERROR_AUTHORIZATION_FAILED = -1,  ///< Credentials are wrong or outdated
        UPDATE_ERROR_PROJECT_SUSPENDED = -2,     ///< The specified project was suspended.
        UPDATE_ERROR_NO_NETWORK_CONNECTION = -3, ///< Device has no network connection
        UPDATE_ERROR_SERVICE_NOT_AVAILABLE = -4, ///< Server not found, down or overloaded.
        UPDATE_ERROR_BAD_FRAME_QUALITY = -5,     ///< Low-frame quality has been continuously observed
        UPDATE_ERROR_UPDATE_SDK = -6,            ///< SDK Version outdated.
        UPDATE_ERROR_TIMESTAMP_OUT_OF_RANGE = -7,///< Client/Server clocks too far away.
        UPDATE_ERROR_REQUEST_TIMEOUT = -8        ///< No response to network request after timeout.
    };

    /// <summary>
    /// This struct contains all known data of a specific search result
    /// </summary>
    public struct TargetSearchResult
    {
        // name of the target
        public string TargetName;
        // system-wide unique id of the target.
        public string UniqueTargetId;
        /// width of the target (in 3D scene units)
        public float TargetSize;
        // metadata associated with this target
        public string MetaData;
        //tracking rating for this target
        /**
         *  The tracking rating represents a 5-star rating describing the
         *  suitability of this target for tracking on a scale from 0 to 5. A low
         *  tracking rating may result in poor tracking or unstable augmentation.
         */
        public byte TrackingRating;
        // pointer to native search result
        public IntPtr TargetSearchResultPtr;
    }

    #endregion // NESTED


    
    #region PUBLIC_METHODS

    /// <summary>
    /// Starts initialization of the cloud-based recognition system.
    ///
    /// Initialization of the cloud-based recognition system may take significant
    /// time and is thus handled in a background process. Use GetInitState() to
    /// query the initialization progress and result. Pass in the user/password
    /// for authenticating with the cloud reco server.
    /// </summary>
    public abstract bool StartInit(string userAuth, string secretAuth);


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
    public abstract InitState GetInitState();


    /// <summary>
    /// Deinitializes the cloud-based recognition system
    /// </summary>
    public abstract bool Deinit();


    /// <summary>
    /// Starts cloud recognition
    ///
    /// Starts continuous recognition of Targets from the cloud.
    /// Use updateSearchResult() and getResult() to retrieve search matches.
    /// </summary>
    public abstract bool StartRecognition();


    /// <summary>
    /// Stops cloud recognition
    /// </summary>
    public abstract bool Stop();


    /// <summary>
    /// Sets the base color of the scanline in the scanning UI
    /// </summary>
    public abstract void SetUIScanlineColor(Color color); 


    /// <summary>
    /// Sets the base color of the points in the scanning UI
    /// </summary>
    public abstract void SetUIPointColor(Color color); 


    /// <summary>
    /// Returns true if the TargetFinder is in 'requesting' mode
    ///
    /// When in 'requesting' mode the TargetFinder has issued a search 
    /// query to the recognition server and is waiting for the results.
    /// </summary>
    public abstract bool IsRequesting();


    /// <summary>
    /// Update cloud reco results
    ///
    /// Clears and rebuilds the list of target search results with results found
    /// since the last call to updateSearchResults().
    /// Also refreshes the IsRequesting flag. 
    /// Returns the status code  UPDATE_RESULTS_AVAILABLE if new search results have been found.
    /// </summary>
    public abstract UpdateState Update();


    /// <summary>
    /// Returns new search results
    ///
    /// Earlier search result instances are destroyed when UpdateSearchResults
    /// is called. 
    /// </summary>
    public abstract IEnumerable<TargetSearchResult> GetResults();


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
    public abstract ImageTargetBehaviour EnableTracking(TargetSearchResult result, string gameObjectName);


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
    public abstract ImageTargetBehaviour EnableTracking(TargetSearchResult result, GameObject gameObject);
    
    
    /// <summary>
    /// Clears all targets enabled for tracking
    ///
    /// Destroys all ImageTargets that have been created via EnableTracking().
    /// </summary>
    public abstract void ClearTrackables(bool destroyGameObjects = true);
    

    /// <summary>
    /// Returns the ImageTargets currently enabled for tracking.
    /// </summary>
    public abstract IEnumerable<ImageTarget> GetImageTargets();

    #endregion // PUBLIC_METHODS
}
