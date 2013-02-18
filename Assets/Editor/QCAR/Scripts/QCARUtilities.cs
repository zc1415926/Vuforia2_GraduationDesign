/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class QCARUtilities
{
    #region NESTED

    // This struct contains a collection of constant variables and makes them
    // available to all editor classes.
    public struct GlobalVars
    {
        // Paths to QCAR specific assets.
        public const string DATA_SET_PATH =
            "Assets/StreamingAssets/QCAR/";
        public const string DAT_PATH =
            "Assets/StreamingAssets/QCAR/qcar-resources.dat";
        public const string CONFIG_XML_PATH =
            "Assets/StreamingAssets/QCAR/config.xml";
        // Paths to materials and textures that are used for Trackables and
        // Virtual Buttons.
        public const string TARGET_TEXTURES_PATH =
            "Assets/Editor/QCAR/ImageTargetTextures/";
        public const string REFERENCE_MATERIAL_PATH =
            "Assets/Qualcomm Augmented Reality/Materials/DefaultTarget.mat";
        public const string MASK_MATERIAL_PATH =
            "Assets/Qualcomm Augmented Reality/Materials/DepthMask.mat";
        public const string VIRTUAL_BUTTON_MATERIAL_PATH =
            "Assets/Editor/QCAR/VirtualButtonTextures/" +
            "VirtualButtonPreviewMaterial.mat";
        public const string UDT_MATERIAL_PATH =
            "Assets/Qualcomm Augmented Reality/Materials/UserDefinedTarget.mat";
        public const string CL_MATERIAL_PATH =
            "Assets/Qualcomm Augmented Reality/Materials/CloudRecoTarget.mat";

        // Default name used for Trackables that are not part of the config.xml
        // file yet.
        public const string DEFAULT_TRACKABLE_NAME = "--- EMPTY ---";
        // Default name used for data sets.
        public const string DEFAULT_DATA_SET_NAME = "--- EMPTY ---";
        // separation line in dropdown menus:
        public const string PREDEFINED_TARGET_DROPDOWN_TEXT = "Predefined";
        // string entry used to select user created targets
        public const string USER_CREATED_TARGET_DROPDOWN_TEXT = "User Defined";
        // string entry used to select cloud reco targets
        public const string CLOUD_RECO_DROPDOWN_TEXT = "Cloud Reco";
        // The theoretical maximum of Frame Markers that can be used in an
        // application.
        public const int MAX_NUM_FRAME_MARKERS = 512;
    }

    #endregion // NESTED



    #region PUBLIC_METHODS

    // Parses well formed strings to a Size vector.
    public static bool SizeFromStringArray(out Vector2 result,
                                           string[] valuesToParse)
    {
        result = Vector2.zero;

        // Check if we have the same number of elements for the Vector type and
        // the string array.
        bool areParamsOk = false;
        if (valuesToParse != null)
        {
            if (valuesToParse.Length == 2)
            {
                areParamsOk = true;
            }
        }

        if (!areParamsOk)
        {
            return false;
        }

        try
        {
            result = new Vector2(float.Parse(valuesToParse[0]),
                                 float.Parse(valuesToParse[1]));
        }
        catch
        {
            return false;
        }

        return true;
    }


    // Parses well formed strings to a Transform vector.
    public static bool TransformFromStringArray(out Vector3 result,
                                                string[] valuesToParse)
    {
        result = Vector3.zero;

        // Check if we have the same number of elements for the Vector type and
        // the string array.
        bool areParamsOk = false;
        if (valuesToParse != null)
        {
            if (valuesToParse.Length == 3)
            {
                areParamsOk = true;
            }
        }

        if (!areParamsOk)
        {
            return false;
        }

        try
        {
            result = new Vector3(float.Parse(valuesToParse[0]),
                                 float.Parse(valuesToParse[2]),
                                 float.Parse(valuesToParse[1]));
        }
        catch
        {
            return false;
        }

        return true;
    }


    // Parses well formed strings to a Rectangle vector.
    public static bool RectangleFromStringArray(out Vector4 result,
                                                string[] valuesToParse)
    {
        result = Vector4.zero;

        // Check if we have the same number of elements for the Vector type and
        // the string array.
        bool areParamsOk = false;
        if (valuesToParse != null)
        {
            if (valuesToParse.Length == 4)
            {
                areParamsOk = true;
            }
        }

        if (!areParamsOk)
        {
            return false;
        }

        try
        {
            result = new Vector4(float.Parse(valuesToParse[0]),
                                 float.Parse(valuesToParse[1]),
                                 float.Parse(valuesToParse[2]),
                                 float.Parse(valuesToParse[3]));
        }
        catch
        {
            return false;
        }

        return true;
    }


    // Parses well formed strings to a Orientation Quaternion.
    // This function is QCAR specific. It changes some of the number signs when
    // parsing.
    public static bool OrientationFromStringArray(out Quaternion result,
                                                  string[] valuesToParse)
    {
        result = Quaternion.identity;

        bool areParamsOk = false;
        if (valuesToParse != null)
        {
            if (valuesToParse.Length == 5)
            {
                areParamsOk = true;
            }
            else if (valuesToParse.Length == 4)
            {
                Debug.LogError("Direct parsing of Quaternions is not " +
                               "supported. Use Axis-Angle Degrees (AD:) or " +
                               "Axis-Angle Radians (AR:) instead.");
            }
        }

        if (!areParamsOk)
        {
            return false;
        }

        try
        {
            float angle = float.Parse(valuesToParse[4]);
            Vector3 axis = new Vector3(-float.Parse(valuesToParse[1]),
                                       float.Parse(valuesToParse[3]),
                                       -float.Parse(valuesToParse[2]));

            if (string.Compare(valuesToParse[0], "ad:", true) == 0)
            {
                result = Quaternion.AngleAxis(angle, axis);
            }
            else if (string.Compare(valuesToParse[0], "ar:", true) == 0)
            {
                result = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, axis);
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }


    // Returns the index of a string in a string array.
    // Returns -1 if the string is not part of the array.
    public static int GetIndexFromString(string stringToFind, string[] availableStrings)
    {
        int index = -1;
        for (int i = 0; i < availableStrings.Length; ++i)
        {
            if (string.Compare(availableStrings[i], stringToFind, true) == 0)
            {
                index = i;
            }
        }
        return index;
    }


    // Returns the PrefabType and suppresses warnings
    // EditorUtility.GetPrefabType is deprecated as of Unity 3.5
    public static PrefabType GetPrefabType(UnityEngine.Object target)
    {
        // Disable the obsolete method warning
        #pragma warning disable 618
        return EditorUtility.GetPrefabType(target);
        #pragma warning restore 618
    }


    // Create a default ImageTarget that can be added to the default dataset.
    public static ConfigData.ImageTargetData CreateDefaultImageTarget()
    {
        ConfigData.ImageTargetData it = new ConfigData.ImageTargetData();

        // Apply default values
        it.size = new Vector2(200.0f, 200.0f);
        it.virtualButtons = new List<ConfigData.VirtualButtonData>();

        return it;
    }

    #endregion // PUBLIC_METHODS
}