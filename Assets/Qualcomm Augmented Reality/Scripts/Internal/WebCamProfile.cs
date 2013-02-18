/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

#if UNITY_EDITOR
using System;
using System.IO;
using System.Xml;
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class loads a xml file from the streamingassets folder that contains
/// information about the webcamera that is used in the emulator
/// Most of the code is excluded via ifdefs outside of the editor to avoid
/// importing larger Mono libraries for XML parsing when build for a mobile device
/// </summary>
public class WebCamProfile
{
    #region NESTED

    public struct ProfileData
    {
        public QCARRenderer.Vec2I RequestedTextureSize;
        public QCARRenderer.Vec2I ResampledTextureSize;
        public int RequestedFPS;
    }

    #endregion // NESTED



    #region PRIVATE_MEMBER_VARIABLES
    
    private ProfileData mDefaultProfile = new ProfileData();
    private readonly Dictionary<string,ProfileData> mProfiles = new Dictionary<string,ProfileData>(); 

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    public ProfileData Default
    {
        get { return mDefaultProfile; }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    public WebCamProfile()
    {
        LoadAndParseProfiles();
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the profile data for the given webcam model.
    /// If none is found, the default profile will be returned.
    /// </summary>
    public ProfileData GetProfile(string webcamName)
    {
        ProfileData profileData;
        if (mProfiles.TryGetValue(webcamName.ToLower(), out profileData))
        {
            return profileData;
        }

        // if none found, return default profile
        return mDefaultProfile;
    }

    /// <summary>
    /// Returns true if a profile for the given webcam exists
    /// </summary>
    public bool ProfileAvailable(string webcamName)
    {
        return mProfiles.ContainsKey(webcamName.ToLower());
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private void LoadAndParseProfiles()
    {

//make sure this code and necessary libraries are not included in native builds to avoid increasing the app size
#if UNITY_EDITOR
        try
        {
            string filePath = Path.Combine(Application.dataPath, "Editor/QCAR/WebcamProfiles/profiles.xml");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            // 
            XmlNodeList cameraNodes = xmlDoc.GetElementsByTagName("webcam");
            foreach (XmlNode cameraNode in cameraNodes)
            {
                // parse this entry and store it in dictionary
                mProfiles[cameraNode.Attributes["deviceName"].Value.ToLower()] = ParseConfigurationEntry(cameraNode);
            }

            // parse default configuration
            mDefaultProfile = ParseConfigurationEntry(xmlDoc.GetElementsByTagName("default")[0]);
        }
        catch (Exception e)
        {
            string errorMsg = "Exception occurred when trying to parse web cam profile file: " + e.Message;
            EditorUtility.DisplayDialog("Error occurred!", errorMsg, "Ok");
            Debug.LogError(errorMsg);
        }

#endif

    }


    //make sure this code and necessary libraries are not included in native builds to avoid increasing the app size
#if UNITY_EDITOR

    private ProfileData ParseConfigurationEntry(XmlNode cameraNode)
    {
        foreach (XmlNode platformNode in cameraNode.ChildNodes)
        {
            string platformIdentifier = "undefined";
            if (Application.platform == RuntimePlatform.WindowsEditor) platformIdentifier = "windows";
            if (Application.platform == RuntimePlatform.OSXEditor) platformIdentifier = "osx";
            if (platformNode.Name.Equals(platformIdentifier))
            {
                return new ProfileData()
                           {
                               RequestedTextureSize =
                                   new QCARRenderer.Vec2I(
                                   int.Parse(GetValueOfChildNodeByName(platformNode, "requestedTextureWidth")),
                                   int.Parse(GetValueOfChildNodeByName(platformNode, "requestedTextureHeight"))),
                               ResampledTextureSize =
                                   new QCARRenderer.Vec2I(
                                   int.Parse(GetValueOfChildNodeByName(platformNode, "resampledTextureWidth")),
                                   int.Parse(GetValueOfChildNodeByName(platformNode, "resampledTextureHeight"))),
                               RequestedFPS = 30
                           };
            }
        }

        // nothing returned yet -> throw an exception
        throw new Exception("Could not parse webcam profile: " + cameraNode.InnerXml);
    }

    private static string GetValueOfChildNodeByName(XmlNode parentNode, string name)
    {
        foreach (XmlNode childNode in parentNode.ChildNodes)
        {
            if (childNode.Name.Equals(name)) return childNode.InnerXml;
        }
        return "";
    }

#endif

    #endregion // PRIVATE_METHODS

}