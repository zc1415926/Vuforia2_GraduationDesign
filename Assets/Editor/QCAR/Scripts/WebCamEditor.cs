/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WebCamBehaviour))]
public class WebCamEditor : Editor
{
    #region PRIVATE

    private const string NO_CAMERAS_TEXT = "NO CAMERAS FOUND";
    private static WebCamProfile sWebCamProfiles;
    private static string[] sWebCamDeviceNames = new string[0];
    private static bool sWebCamDevicesReadOnce = false;
    private static DateTime sLastDeviceListRefresh = DateTime.Now - TimeSpan.FromDays(1); // initialize to yesterday
    // on mac refreshing the list of webcams blocks the UI for quite some time, on windows it can be done more often.
#if UNITY_STANDALONE_OSX
    private static readonly TimeSpan sDeviceListRefreshInterval = TimeSpan.FromMilliseconds(5000d);
#else
    private static readonly TimeSpan sDeviceListRefreshInterval = TimeSpan.FromMilliseconds(500d);
#endif
    #endregion // PRIVATE



    #region UNITY_EDITOR_METHODS

    public void OnEnable()
    {
        WebCamBehaviour webCam = (WebCamBehaviour)target;
        
        // We don't want to initialize if this is a prefab.
        if (QCARUtilities.GetPrefabType(webCam) == PrefabType.Prefab)
        {
            return;
        }

        // Initialize scene manager
        if (!SceneManager.Instance.SceneInitialized)
        {
            SceneManager.Instance.InitScene();
        }
    }
    
    public override void OnInspectorGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            WebCamBehaviour webCam = (WebCamBehaviour)target;
            if (QCARUtilities.GetPrefabType(webCam) != PrefabType.Prefab)
            {
                webCam.TurnOffWebCam = EditorGUILayout.Toggle("Don't use for Play Mode", webCam.TurnOffWebCam);

                if (!webCam.TurnOffWebCam)
                {
                    // check if play mode is supported by this Unity version:
                    if (!webCam.CheckNativePluginSupport())
                    {
                        EditorGUILayout.HelpBox("Play Mode requires a Unity Pro license!", MessageType.Warning);
                    }

                    int currentDeviceIndex = 0;
                    string[] deviceNames = GetDeviceNames();
                    for (int i = 0; i < deviceNames.Length; i++)
                        if (deviceNames[i] != null) // sometimes this happens during Play Mode startup on Mac
                            if (deviceNames[i].Equals(webCam.DeviceName))
                                currentDeviceIndex = i;

                    // check if there is a device profile for the currently selected webcam
                    if (sWebCamProfiles == null) sWebCamProfiles = new WebCamProfile();
                    string deviceName = deviceNames[currentDeviceIndex];
                    if (deviceName.Equals(NO_CAMERAS_TEXT))
                    {
                        EditorGUILayout.HelpBox("No camera connected!\nTo run your application using Play Mode, please connect a webcam to your computer.", MessageType.Warning);
                    }
                    else
                    {
                        if (!sWebCamProfiles.ProfileAvailable(deviceNames[currentDeviceIndex]))
                        {
                            EditorGUILayout.HelpBox("Webcam profile not found!\n" +
                                                    "Unfortunately there is no profile for your webcam model: '" +
                                                    deviceNames[currentDeviceIndex] + "'.\n\n" +
                                                    "The default profile will been used. You can configure a profile yourself by editing '" +
                                                    Path.Combine(Application.dataPath, "Editor/QCAR/WebcamProfiles/profiles.xml") +
                                                    "'.", MessageType.Warning);
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("Camera Device");
                    int newDeviceIndex = EditorGUILayout.Popup(currentDeviceIndex, deviceNames);

                    if ((newDeviceIndex != currentDeviceIndex) && (!deviceNames[newDeviceIndex].Equals(NO_CAMERAS_TEXT)))
                        webCam.DeviceName = deviceNames[newDeviceIndex];

                    EditorGUILayout.EndHorizontal();

                    webCam.FlipHorizontally = EditorGUILayout.Toggle("Flip Horizontally", webCam.FlipHorizontally);

                    EditorGUILayout.Space();

                    EditorGUILayout.HelpBox("Here you can enter the index of the layer that will be used internally for our render to texture " +
                                            "functionality. the ARCamera will be configured to not draw this layer.", MessageType.None);
                    webCam.RenderTextureLayer = EditorGUILayout.IntField("Render Texture Layer", webCam.RenderTextureLayer);
                }

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(webCam);
                    SceneManager.Instance.SceneUpdated();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Webcam settings cannot be changed during Play Mode.", MessageType.Info);
            }
        }
     }
    
    #endregion // UNITY_EDITOR_METHODS



    #region PRIVATE_METHODS
    
    string[] GetDeviceNames()
    {
        // update the list of devices only if we are not currently in Play Mode
        // OR it has not been initialized once.
        if (!EditorApplication.isPlaying || !sWebCamDevicesReadOnce)
        {
            if ((DateTime.Now - sDeviceListRefreshInterval) > sLastDeviceListRefresh)
            {
                try
                {
                    WebCamDevice[] devices = WebCamTexture.devices;
                    int deviceCount = WebCamTexture.devices.Length;

                    if (deviceCount > 0)
                    {
                        sWebCamDeviceNames = new string[deviceCount];

                        for (int n = 0; n < deviceCount; n++)
                        {
                            sWebCamDeviceNames[n] = devices[n].name;
                        }
                    }
                    else
                    {
                        sWebCamDeviceNames = new string[1];
                        sWebCamDeviceNames[0] = NO_CAMERAS_TEXT;
                    }

                }
                catch (System.Exception ex)
                {
                    // catch when camera is in use by another app.
                    Debug.Log(ex.Message);
                }

                sWebCamDevicesReadOnce = true;
                sLastDeviceListRefresh = DateTime.Now;
            }
        }

        return sWebCamDeviceNames;
    }

    #endregion // PRIVATE_METHODS
}
