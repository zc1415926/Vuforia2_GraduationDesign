/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
#if UNITY_EDITOR
        using UnityEditor;
#endif
using Object = UnityEngine.Object;

/// <summary>
/// A utility class containing various helper methods
/// </summary>
public class QCARRuntimeUtilities
{
    #region NESTED

    private enum WebCamUsed
    {
        UNKNOWN,
        TRUE,
        FALSE
    }

    #endregion // NESTED



    #region PRIVATE_STATIC_VARIABLES
    
#if UNITY_EDITOR
    private static WebCamUsed sWebCamUsed = WebCamUsed.UNKNOWN;
#endif
    private static ScreenOrientation sScreenOrientation = ScreenOrientation.Unknown;

    #endregion // PRIVATE_STATIC_VARIABLES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the file name without the path.
    /// </summary>
    public static string StripFileNameFromPath(string fullPath)
    {
        string[] pathParts = fullPath.Split(new char[] { '/' });
        string fileName = pathParts[pathParts.Length - 1];

        return fileName;
    }


    /// <summary>
    /// Returns the extension without the path.
    /// </summary>
    public static string StripExtensionFromPath(string fullPath)
    {
        string[] pathParts = fullPath.Split(new char[] { '.' });

        // Return empty string if there is no extension.
        if (pathParts.Length <= 1)
        {
            return "";
        }

        string extension = pathParts[pathParts.Length - 1];

        return extension;
    }

    /// <summary>
    /// Wrapper for Screen.orientation to enable correct handling in Editor
    /// and on devices that do not support all orientations.
    /// </summary>
    public static ScreenOrientation ScreenOrientation
    {
        get
        {
            if (QCARRuntimeUtilities.IsPlayMode())
                return ScreenOrientation.LandscapeLeft;
            else
            {
                // if this is queried before it has been initialized, ask native:
                if (sScreenOrientation == ScreenOrientation.Unknown)
                    sScreenOrientation = (ScreenOrientation)QCARWrapper.Instance.GetSurfaceOrientation();

                return sScreenOrientation;
            }
        }
    }

    /// <summary>
    /// This method is called from QCARBehaviour whenever the surface orientation changes in native.
    /// The last orientation is remembered here so that it is not queried all the time.
    /// </summary>
    public static void CacheSurfaceOrientation(ScreenOrientation surfaceOrientation)
    {
        sScreenOrientation = surfaceOrientation;
    }

    /// <summary>
    /// returns true of ScreenOrientation is in ANY landscape mode
    /// </summary>
    public static bool IsLandscapeOrientation
    {
        get
        {
            ScreenOrientation screenOrientation = ScreenOrientation;
            return (screenOrientation == ScreenOrientation.Landscape) ||
                   (screenOrientation == ScreenOrientation.LandscapeLeft) ||
                   (screenOrientation == ScreenOrientation.LandscapeRight);
        }
    }

    /// <summary>
    /// returns true if ScreenOrientation is in ANY portrait mode
    /// </summary>
    public static bool IsPortraitOrientation
    {
        get { return !IsLandscapeOrientation; }
    }

    /// <summary>
    /// Disables all TrackableBehaviours. Used when an GL or orientation error is detected in a sample.
    /// </summary>
    public static void ForceDisableTrackables()
    {
        TrackableBehaviour[] tbs = (TrackableBehaviour[])Object.FindObjectsOfType(typeof(TrackableBehaviour));
        if (tbs != null)
        {
            for (int i = 0; i < tbs.Length; ++i)
            {
                tbs[i].enabled = false;
            }
        }
    }


    /// <summary>
    /// returns ONLY true if we are running in Play Mode
    /// </summary>
    public static bool IsPlayMode()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// returns true if we have access to QCAR (on a mobile device OR in the emulator in Unity Pro with a webcam connected)
    /// </summary>
    public static bool IsQCAREnabled()
    {
#if UNITY_EDITOR
        if (sWebCamUsed == WebCamUsed.UNKNOWN)
        {
            // query the webcam if it should be used
            WebCamBehaviour webcam = (WebCamBehaviour)Object.FindObjectOfType(typeof(WebCamBehaviour));
            sWebCamUsed = webcam.IsWebCamUsed() ? WebCamUsed.TRUE : WebCamUsed.FALSE;
        }

        return sWebCamUsed == WebCamUsed.TRUE;
#else
        return true;
#endif
    }

    /// <summary>
    /// This method forces Play Mode to restart.
    /// It is called when Unity re-compiles the scripts shortly after starting play mode.
    /// </summary>
    public static void RestartPlayMode()
    {
#if UNITY_EDITOR
        // register for the next update call to start play mode again
        EditorApplication.update += CheckToStartPlayMode;
        // stop play mode - will not be executed immediately, but after scripts have been executed.
        EditorApplication.isPlaying = false;
#endif
    }


#if UNITY_EDITOR
    /// <summary>
    /// This restarts Play Mode and unregisters from the editor callback.
    /// </summary>
    private static void CheckToStartPlayMode()
    {
        // if play mode has stopped
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= CheckToStartPlayMode;
            EditorApplication.isPlaying = true;
            Debug.LogWarning("Restarted Play Mode because scripts have been recompiled.");
        }
    }
#endif

    #endregion PUBLIC_METHODS
}