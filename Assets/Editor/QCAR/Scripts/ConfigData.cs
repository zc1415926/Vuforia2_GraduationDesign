/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

// This class is used to store and access data that is read from a config.xml
// file.
public class ConfigData
{
    #region NESTED

    // Representation of a Virtual Button node in the config.xml file.
    public struct VirtualButtonData
    {
        public string name;
        public bool enabled;
        public Vector4 rectangle;
        public VirtualButton.Sensitivity sensitivity;
    }

    // Representation of an Image Target node in the config.xml file.
    public struct ImageTargetData
    {
        public Vector2 size;
        public List<VirtualButtonData> virtualButtons;
    }

    // Representation of a Multi Target Part node in the config.xml file.
    public struct MultiTargetPartData
    {
        public string name;
        public Vector3 translation;
        public Quaternion rotation;
    }

    // Representation of a Multi Target node in the config.xml file.
    public struct MultiTargetData
    {
        public List<MultiTargetPartData> parts;
    }

    // Representation of a Frame Marker node in the config.xml file.
    public struct FrameMarkerData
    {
        public string name;
        public Vector2 size;
    }

    #endregion // NESTED



    #region PROPERTIES

    // Returns the number of Image Targets currently present in the config data.
    public int NumImageTargets
    {
        get
        {
            return imageTargets.Count;
        }
    }

    // Returns the number of Multi Targets currently present in the config data.
    public int NumMultiTargets
    {
        get
        {
            return multiTargets.Count;
        }
    }

    //Returns the overall number of Trackables currently present in the config data.
    public int NumTrackables
    {
        get
        {
            return (NumImageTargets + NumMultiTargets);
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    private Dictionary<string, ImageTargetData> imageTargets;
    private Dictionary<string, MultiTargetData> multiTargets;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    // Constructor of ConfigData class.
    // Creates initializes internal collections.
    public ConfigData()
    {
        imageTargets = new Dictionary<string, ImageTargetData>();
        multiTargets = new Dictionary<string, MultiTargetData>();
    }

    // Copy constructor of ConfigData class.
    public ConfigData(ConfigData original)
    {
        // Create Image Target dictionary from original.
        imageTargets =
            new Dictionary<string, ImageTargetData>(original.imageTargets);

        // Create Multi Target dictionary from original.
        multiTargets =
            new Dictionary<string, MultiTargetData>(original.multiTargets);
    }

    #endregion //CONSTRUCTION



    #region PUBLIC_METHODS

    // Set attributes of the Image Target with the given name.
    // If the Image Target does not yet exist it is created automatically.
    public void SetImageTarget(ImageTargetData item, string name)
    {
        imageTargets[name] = item;
    }


    // Set attributes of the Multi Target with the given name.
    // If the Multi Target does not yet exist it is created automatically.
    public void SetMultiTarget(MultiTargetData item, string name)
    {
        multiTargets[name] = item;
    }


    // Add Virtual Button to the Image Target with the given imageTargetName.
    public void AddVirtualButton(VirtualButtonData item, string imageTargetName)
    {
        try
        {
            ImageTargetData it = imageTargets[imageTargetName];
            it.virtualButtons.Add(item);
        }
        catch
        {
            throw;
        }
    }


    // Add Multi Target Part to the Multi Target with the given multiTargetName.
    public void AddMultiTargetPart(MultiTargetPartData item, string multiTargetName)
    {
        try
        {
            MultiTargetData mt = multiTargets[multiTargetName];
            mt.parts.Add(item);
        }
        catch
        {
            throw;
        }
    }


    // Clear all data.
    public void ClearAll()
    {
        ClearImageTargets();
        ClearMultiTargets();
    }


    // Clear all Image Target data.
    public void ClearImageTargets()
    {
        imageTargets.Clear();
    }


    // Clear all Multi Target data.
    public void ClearMultiTargets()
    {
        multiTargets.Clear();
    }


    // Clear all Virtual Button data.
    public void ClearVirtualButtons()
    {
        foreach (ImageTargetData it in imageTargets.Values)
        {
            it.virtualButtons.Clear();
        }
    }


    // Remove Image Target with the given name.
    // Returns false if Image Target does not exist.
    public bool RemoveImageTarget(string name)
    {
        return imageTargets.Remove(name);
    }


    // Remove Multi Target with the given name.
    // Returns false if Multi Target does not exist.
    public bool RemoveMultiTarget(string name)
    {
        return multiTargets.Remove(name);
    }


    // Creates a new Image Target with the data of the Image Target with the
    // given name.
    // Returns false if Image Target does not exist.
    public void GetImageTarget(string name, out ImageTargetData it)
    {
        try
        {
            it = imageTargets[name];
        }
        catch
        {
            throw;
        }
    }


    // Creates a new Multi Target with the data of the Multi Target with the
    // given name.
    // Returns false if Multi Target does not exist.
    public void GetMultiTarget(string name, out MultiTargetData mt)
    {
        try
        {
            mt = multiTargets[name];
        }
        catch
        {
            throw;
        }
    }


    // Creates a new Virtual Button with the data of the Virtual Button with the
    // given name that is a child of the Image Target with the name
    // imageTargetName.
    // Returns false if Virtual Button does not exist.
    public void GetVirtualButton(string name,
                                 string imageTargetName,
                                 out VirtualButtonData vb)
    {
        vb = new VirtualButtonData();

        try
        {
            ImageTargetData it;
            GetImageTarget(imageTargetName, out it);

            List<VirtualButtonData> vbs = it.virtualButtons;
            for (int i = 0; i < vbs.Count; ++i)
            {
                if (vbs[i].name == name)
                {
                    vb = vbs[i];
                }
            }
        }
        catch
        {
            throw;
        }
    }


    // Checks if the Image Target with the given name is part of the data set.
    public bool ImageTargetExists(string name)
    {
        return imageTargets.ContainsKey(name);
    }


    // Checks if the Multi Target with the given name is part of the data set.
    public bool MultiTargetExists(string name)
    {
        return multiTargets.ContainsKey(name);
    }


    // Copy all Image Target names into the given string array.
    // The index defines at which location to start copying.
    public void CopyImageTargetNames(string[] arrayToFill, int index)
    {
        try
        {
            imageTargets.Keys.CopyTo(arrayToFill, index);
        }
        catch
        {
            throw;
        }
    }


    // Copy all Multi Target names into the given string array.
    // The index defines at which location to start copying.
    public void CopyMultiTargetNames(string[] arrayToFill, int index)
    {
        try
        {
            multiTargets.Keys.CopyTo(arrayToFill, index);
        }
        catch
        {
            throw;
        }
    }

    #endregion // PUBLIC_METHODS
}
