/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The ConfigData Manager handles operations on the ConfigData (e.g. sync with
// config.xml file, sync with scene).
public class ConfigDataManager
{
    #region PROPERTIES

    // Returns an instance of a ConfigDataManager (thread safe)
    public static ConfigDataManager Instance
    {
        get
        {
            // Make sure only one instance of ConfigDataManager is created.
            if (mInstance == null)
            {
                lock (typeof(ConfigDataManager))
                {
                    if (mInstance == null)
                    {
                        mInstance = new ConfigDataManager();
                    }
                }
            }
            return mInstance;
        }
    }

    // Returns the number of config data objects in the dictionary.
    // Please be aware that the dictionary usually also contains a default dataset.
    public int NumConfigDataObjects
    {
        get
        {
            return mConfigData.Count;
        }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    // The config data object contains the data that is part of the config.xml
    // file.
    private Dictionary<string, ConfigData> mConfigData = null;

    // Singleton: Still uses lazy initialization:
    // Private static variables initialized on first reference to class.
    private static ConfigDataManager mInstance;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    // Private constructor. Class is implemented as a singleton.
    private ConfigDataManager()
    {
        mConfigData = new Dictionary<string, ConfigData>();
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS
    
    // Read config file.
    public void DoRead()
    {
        List<string> xmlFiles = GetFilePaths(QCARUtilities.GlobalVars.DATA_SET_PATH, "xml");

        List<string> correctedXMLFiles = CorrectXMLFileList(xmlFiles);

        // List is completely replaced on read.
        mConfigData.Clear();

        // Add default data set.
        mConfigData.Add(QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME,
                        CreateDefaultDataSet());

        foreach (string xmlFile in correctedXMLFiles)
        {
            ReadConfigData(xmlFile);
        }
    }


    // Get the config data object with the specified name.
    public ConfigData GetConfigData(string dataSetName)
    {
        return mConfigData[dataSetName];
    }


    // Returns all data set names that are currently available.
    // The array also includes the default data set name.
    public void GetConfigDataNames(string[] configDataNames)
    {
        try
        {
            GetConfigDataNames(configDataNames, true);
        }
        catch
        {
            throw;
        }
    }


    // Returns all data set names that are currently available.
    // Optionally include the default data set which can't be used at runtime.
    public void GetConfigDataNames(string[] configDataNames, bool includeDefault)
    {
        try
        {
            if (includeDefault)
            {
                mConfigData.Keys.CopyTo(configDataNames, 0);
            }
            else
            {
                Dictionary<string, ConfigData>.KeyCollection.Enumerator dicEnum =
                    mConfigData.Keys.GetEnumerator();

                int i = 0;
                // Ignore the first element:
                dicEnum.MoveNext();
                while(dicEnum.MoveNext())
                {
                    configDataNames[i] = dicEnum.Current;
                    ++i;
                }
            }
        }
        catch
        {
            throw;
        }
    }


    // Check if data set with given name is part of the dictionary.
    public bool ConfigDataExists(string configDataName)
    {
        return mConfigData.ContainsKey(configDataName);
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Compares the list of XML files with a list of DAT files and makes
    // sure that for every XML there is also a DAT file.
    private List<string> CorrectXMLFileList(List<string> xmlFileList)
    {
        List<string> correctedList = new List<string>(xmlFileList.Count);
        List<string> datFileList = GetFilePaths(QCARUtilities.GlobalVars.DATA_SET_PATH, "dat");
        foreach (string xmlFile in xmlFileList)
        {
            bool correspondingFileFound = false;
            string noExtension = xmlFile.Remove(xmlFile.Length - 4);
            foreach (string datFile in datFileList)
            {
                if (datFile.IndexOf(noExtension) == 0)
                {
                    correctedList.Add(xmlFile);
                    correspondingFileFound = true;
                }
            }

            // Warn user about file inconsistencies.
            if (!correspondingFileFound)
            {
                Debug.LogWarning(xmlFile + " ignored. No corresponding DAT file found.");
            }
        }
        return correctedList;
    }


    // Returns all files with the given extension from the given path.
    private List<string> GetFilePaths(string directoryPath, string fileType)
    {
        List<string> files = new List<string>();
        if (Directory.Exists(directoryPath))
        {
            string[] allFilePaths = Directory.GetFiles(directoryPath);

            foreach (string dataSetFilePath in allFilePaths)
            {
                string extension = QCARRuntimeUtilities.StripExtensionFromPath(dataSetFilePath);
                if (extension.IndexOf(fileType,
                    System.StringComparison.OrdinalIgnoreCase) == 0)
                {
                    files.Add(dataSetFilePath);
                }
            }
        }
        return files;
    }


    // Method reads config data from config.xml file.
    private void ReadConfigData(string dataSetFilePath)
    {
        ConfigData dataSetData = new ConfigData();

        // Parse config.xml file data.
        ConfigParser.Instance.fileToStruct(dataSetFilePath, dataSetData);

        string dataSetName = QCARRuntimeUtilities.StripFileNameFromPath(dataSetFilePath);
        string dataSetNameNoExt = dataSetName.Remove(dataSetName.Length - 4);
        mConfigData[dataSetNameNoExt] = dataSetData;
    }


    // Create a default dataset for use in the editor.
    private ConfigData CreateDefaultDataSet()
    {
        ConfigData defaultDataSetData = new ConfigData();

        defaultDataSetData.SetImageTarget(QCARUtilities.CreateDefaultImageTarget(), QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME);
        defaultDataSetData.SetMultiTarget(CreateDefaultMultiTarget(), QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME);

        return defaultDataSetData;
    }


    // Create a default MultiTarget that can be added to the default dataset.
    private ConfigData.MultiTargetData CreateDefaultMultiTarget()
    {
        ConfigData.MultiTargetData mt = new ConfigData.MultiTargetData();

        // Apply default values
        mt.parts = CreateDefaultParts();

        return mt;
    }


    // Create Multi Target from default Image Targets
    private List<ConfigData.MultiTargetPartData> CreateDefaultParts()
    {
        List<ConfigData.MultiTargetPartData> prts =
            new List<ConfigData.MultiTargetPartData>(6);

        // Get default Image Target and use it as template for MT parts.
        ConfigData.ImageTargetData it = QCARUtilities.CreateDefaultImageTarget();

        // We assume a square default target.
        float offset = it.size.x * 0.5f;

        // Front
        ConfigData.MultiTargetPartData frontPart = new ConfigData.MultiTargetPartData();
        frontPart.translation = new Vector3(0, offset, 0);
        frontPart.rotation = Quaternion.AngleAxis(0, new Vector3(1, 0, 0));
        frontPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(frontPart);

        // Back
        ConfigData.MultiTargetPartData backPart = new ConfigData.MultiTargetPartData();
        backPart.translation = new Vector3(0, -offset, 0);
        backPart.rotation = Quaternion.AngleAxis(180, new Vector3(1, 0, 0));
        backPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(backPart);

        // Left
        ConfigData.MultiTargetPartData leftPart = new ConfigData.MultiTargetPartData();
        leftPart.translation = new Vector3(-offset, 0, 0);
        leftPart.rotation = Quaternion.AngleAxis(90, new Vector3(0, 0, 1));
        leftPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(leftPart);

        // Right
        ConfigData.MultiTargetPartData rightPart = new ConfigData.MultiTargetPartData();
        rightPart.translation = new Vector3(offset, 0, 0);
        rightPart.rotation = Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));
        rightPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(rightPart);

        // Top
        ConfigData.MultiTargetPartData topPart = new ConfigData.MultiTargetPartData();
        topPart.translation = new Vector3(0, 0, offset);
        topPart.rotation = Quaternion.AngleAxis(90, new Vector3(1, 0, 0));
        topPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(topPart);

        // Bottom
        ConfigData.MultiTargetPartData btmPart = new ConfigData.MultiTargetPartData();
        btmPart.translation = new Vector3(0, 0, -offset);
        btmPart.rotation = Quaternion.AngleAxis(-90, new Vector3(1, 0, 0));
        btmPart.name = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        prts.Add(btmPart);

        return prts;
    }

    #endregion // PRIVATE_METHODS
}