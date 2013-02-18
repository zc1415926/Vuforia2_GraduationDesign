/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// The scene manager is responsible for synchronizing scene and config.xml file
public class SceneManager
{
    #region PROPERTIES

    // Returns an instance of a SceneManager (thread safe)
    public static SceneManager Instance
    {
        get
        {
            // Make sure only one instance of SceneManager is created.
            if (mInstance == null)
            {
                lock (typeof(SceneManager))
                {
                    if (mInstance == null)
                    {
                        mInstance = new SceneManager();
                    }
                }
            }
            return mInstance;
        }
    }

    // Returns if the scene has already been initialized. This initialization
    // happens only once in a SceneManager lifetime.
    public bool SceneInitialized
    {
        get
        {
            return mSceneInitialized;
        }
    }    

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    // Delegate that is called by Unity in editor.
    private EditorApplication.CallbackFunction mUpdateCallback = null;
    // These Booleans are used to tell the SceneManager that it is time to write
    // or read values to or from the config.xml file.
    private bool mDoSerialization = false;
    private bool mDoDeserialization = false;
    // This Boolean is used to apply properties (e.g. size) from loaded data
    // sets to the current scene.
    private bool mApplyProperties = false;
    // This Boolean is used to change the appearance (e.g. aspect ratio and
    // texture)of Trackables in the scene with data from the data sets.
    private bool mApplyAppearance = false;
    // This variable is used to check if the scene has been initialized.
    private bool mSceneInitialized = false;
    // Needs scene to be checked for issues (e.g. duplicates)?
    private bool mValidateScene = false;
    // Since it is not possible to open up the QCAR web page from a button press
    // in the Unity editor directly this is handled in the EditorUpdate
    // callback.
    private bool mGoToARPage = false;
    // The path used for exporting the data set.
    private string mDataSetExportPath = "";

    // Singleton: Still uses lazy initialization:
    // Private static variables initialized on first reference to class.
    private static SceneManager mInstance;
    private static TimeSpan UPDATE_INTERVAL = TimeSpan.FromSeconds(60);
    private DateTime mLastUpdate = DateTime.Now;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    private SceneManager()
    {
        mUpdateCallback = new EditorApplication.CallbackFunction(EditorUpdate);
        if (EditorApplication.update == null)
        {
            EditorApplication.update += mUpdateCallback;
        }
        else if (!EditorApplication.update.Equals(mUpdateCallback))
        {
            EditorApplication.update += mUpdateCallback;
        }

        // We force a config.xml read operation before the SceneManager is used
        // to avoid inconsistencies on Unity startup.
        mDoDeserialization = true;

        // Make sure that the scene is initialized whenever a new instance of
        // the SceneManager is created.
        mSceneInitialized = false;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    // Initializes scene with content of the config.xml file (ensures that 
    // changes to the file that happened while Unity was not running are
    // applied).
    public void InitScene()
    {
        DataSetTrackableBehaviour[] trackables =
            (DataSetTrackableBehaviour[])UnityEngine.Object.FindObjectsOfType(
                typeof(DataSetTrackableBehaviour));

        // Read all avaiable data set files.
        ConfigDataManager.Instance.DoRead();

        // We apply the newly read values to the scene.
        UpdateTrackableAppearance(trackables);

        mValidateScene = true;

        mSceneInitialized = true;

        SetUnityVersion();
    }

    
    // Update method triggered by Unity.
    // Used to check scene content changes.
    // Be aware that this update callback only is called once a SceneManager
    // instance has been created.
    // Be aware that the instance is destroyed if the SceneManager file is
    // recompiled.
    public void EditorUpdate()
    {
        TrackableBehaviour[] trackables =
            (TrackableBehaviour[])UnityEngine.Object.FindObjectsOfType(
                typeof(TrackableBehaviour));
        VirtualButtonBehaviour[] virtualButtons =
            (VirtualButtonBehaviour[])UnityEngine.Object.FindObjectsOfType(
                typeof(VirtualButtonBehaviour));

        // Correct scales of Trackables.
        CorrectTrackableScales(trackables);

        // Correct Virtual Button poses.
        VirtualButtonEditor.CorrectPoses(virtualButtons);

        // We do deserialization and serialization in this order to avoid
        // overwriting the config.xml file.
        if (mDoDeserialization)
        {
            ConfigDataManager.Instance.DoRead();

            DataSetLoadEditor.OnConfigDataChanged();

            ApplyDataSetAppearance();

            mDoDeserialization = false;
        }

        if (mApplyAppearance)
        {
            // We apply the newly read values to the scene.
            UpdateTrackableAppearance(trackables);

            mApplyAppearance = false;
        }

        if (mApplyProperties)
        {
            // We apply the newly read values to the scene.
            UpdateTrackableProperties(trackables);

            mApplyProperties = false;
        }

        if (mDoSerialization)
        {
            ConfigData sceneData = CreateDataSetFromTrackables(trackables);
            if (!ConfigParser.Instance.structToFile(mDataSetExportPath, sceneData))
            {
                Debug.LogError("Export of scene file failed.");
            }

            mDoSerialization = false;
        }

        if (mValidateScene)
        {
            // Check if there are duplicate trackables in the scene.
            CheckForDuplicates(trackables);

            // Validate all Virtual Buttons in the scene.
            VirtualButtonEditor.Validate();

            mValidateScene = false;
        }

        if (mGoToARPage)
        {
            mGoToARPage = false;
            System.Diagnostics.Process.Start(
                "https://developer.vuforia.com/target-manager");
        }

        if (mLastUpdate.Add(UPDATE_INTERVAL) < DateTime.Now)
        {
            SetUnityVersion();
            mLastUpdate = DateTime.Now;
        }
    }

    private static void SetUnityVersion()
    {
        // Set the Unity version for internal use
        string path = Path.Combine(Application.dataPath, "StreamingAssets/QCAR");
        QCARUnityImpl.SetUnityVersion(path);
    }
    
    public string[] GetImageTargetNames(string dataSetName)
    {
        ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(dataSetName);
        string[] itNames = new string[dataSetData.NumImageTargets + 1];
        itNames[0] = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        dataSetData.CopyImageTargetNames(itNames, 1);
        return itNames;
    }


    public string[] GetMultiTargetNames(string dataSetName)
    {
        ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(dataSetName);
        string[] itNames = new string[dataSetData.NumMultiTargets + 1];
        itNames[0] = QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME;
        dataSetData.CopyMultiTargetNames(itNames, 1);
        return itNames;
    }


    public int GetNextFrameMarkerID()
    {
        IEditorMarkerBehaviour[] markers =
            (MarkerBehaviour[])UnityEngine.Object.FindObjectsOfType(
                typeof(MarkerBehaviour));

        if (markers.Length <= 0)
        {
            return 0;
        }
        if (markers.Length >= QCARUtilities.GlobalVars.MAX_NUM_FRAME_MARKERS)
        {
            Debug.LogWarning("Too many frame markers in scene.");
            return (QCARUtilities.GlobalVars.MAX_NUM_FRAME_MARKERS - 1);
        }

        int freeMarkerID = 0;
        bool idIsFree = false;
        while (!idIsFree)
        {
            idIsFree = true;
            for (int i = 0; i < markers.Length; ++i)
            {
                // If marker is in scene it is not free.
                if (markers[i].MarkerID == freeMarkerID)
                {
                    idIsFree = false;
                    ++freeMarkerID;
                    break;
                }
            }
        }
        return freeMarkerID;
    }


    // Export scene to dataSet.xml file.
    public void ExportScene(string path)
    {
        mDataSetExportPath = path;

        mDoSerialization = true;
    }


    // Tell scene manager that scene has been updated.
    public void SceneUpdated()
    {
        mValidateScene = true;
    }


    // Method to be called by data postprocessor on import of new QCAR
    // related files.
    public void FilesUpdated()
    {
        mDoDeserialization = true;
    }


    // Change the properties (e.g. size)of Trackables in the scene with
    // data from the data sets.
    public void ApplyDataSetProperties()
    {
        mApplyProperties = true;
    }


    // Change the appearance (e.g. aspect ratio and texture)of Trackables in 
    // the scene with data from the data sets.
    public void ApplyDataSetAppearance()
    {
        mApplyAppearance = true;
    }


    // This is function enables an asynchronous call to open the QCAR help page.
    public void GoToARPage()
    {
        mGoToARPage = true;
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Updates trackables in scene from config data.
    private void UpdateTrackableAppearance(TrackableBehaviour[] trackables)
    {
        // do not set appearance in play mode
        if (!Application.isPlaying)
        {
            foreach (TrackableBehaviour tb in trackables)
            {
                // Ignore non-data set trackables.
                if (!(tb is DataSetTrackableBehaviour))
                {
                    continue;
                }

                DataSetTrackableBehaviour trackable = (DataSetTrackableBehaviour) tb;
                TrackableAccessor configApplier = AccessorFactory.Create(trackable);
                if (configApplier != null)
                    configApplier.ApplyDataSetAppearance();
            }
        }
    }


    // Updates trackables in scene from config data.
    private void UpdateTrackableProperties(TrackableBehaviour[] trackables)
    {
        foreach (TrackableBehaviour tb in trackables)
        {
            // Ignore non-data set trackables.
            if (!(tb is DataSetTrackableBehaviour))
            {
                continue;
            }

            DataSetTrackableBehaviour trackable = (DataSetTrackableBehaviour)tb;
            TrackableAccessor configApplier = AccessorFactory.Create(trackable);
            if (configApplier != null)
                configApplier.ApplyDataSetProperties();
        }
    }


    // This method creates a single data set from the trackables provided.
    // The method ignores the data set property in TrackableBehaviour and
    // adds all Trackables to a single file.
    // Default Trackables are not added to the data set.
    private ConfigData CreateDataSetFromTrackables(TrackableBehaviour[] trackables)
    {
        // Sanity check.
        if (trackables == null)
        {
            return null;
        }

        ConfigData sceneData = new ConfigData();
        
        foreach (TrackableBehaviour tb in trackables)
        {
            // Ignore non-data set trackables.
            if (!(tb is DataSetTrackableBehaviour))
            {
                continue;
            }

            IEditorDataSetTrackableBehaviour trackable = (DataSetTrackableBehaviour)tb;

            string dataSetName = trackable.DataSetName;
            string trackableName = trackable.TrackableName;

            // We ignore default Trackables or undefined Trackables.
            if (dataSetName == QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME ||
                dataSetName == "" ||
                trackableName == QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME ||
                trackableName == "")
            {
                Debug.LogWarning("Ignoring default Trackable for export");
                continue;
            }

            if (trackable.GetType() == typeof(ImageTargetBehaviour))
            {
                ImageTargetBehaviour it = (ImageTargetBehaviour)trackable;
                IEditorImageTargetBehaviour editorIt = it;

                ConfigData.ImageTargetData itConfig = new ConfigData.ImageTargetData();

                itConfig.size = editorIt.GetSize();

                // Process Virtual Button list.
                VirtualButtonBehaviour[] vbs =
                    it.GetComponentsInChildren<VirtualButtonBehaviour>();
                itConfig.virtualButtons = new List<ConfigData.VirtualButtonData>(vbs.Length);
                foreach (VirtualButtonBehaviour vb in vbs)
                {
                    Vector2 leftTop;
                    Vector2 rightBottom;
                    if (!vb.CalculateButtonArea(out leftTop,
                                                out rightBottom))
                    {
                        // Invalid Button
                        continue;
                    }

                    ConfigData.VirtualButtonData vbConfig =
                        new ConfigData.VirtualButtonData();

                    IEditorVirtualButtonBehaviour editorVB = vb;
                    vbConfig.name = editorVB.VirtualButtonName;
                    vbConfig.enabled = editorVB.enabled;
                    vbConfig.rectangle = new Vector4(leftTop.x,
                                                        leftTop.y,
                                                        rightBottom.x,
                                                        rightBottom.y);
                    vbConfig.sensitivity = editorVB.SensitivitySetting;

                    itConfig.virtualButtons.Add(vbConfig);
                }

                sceneData.SetImageTarget(itConfig, editorIt.TrackableName);
            }
            else if (trackable.GetType() == typeof(MultiTargetBehaviour))
            {
                Debug.Log("Multi Targets not exported.");
            }
        }

        return sceneData;
    }


    // Check for duplicate Trackables in the scene.
    private void CheckForDuplicates(IEditorTrackableBehaviour[] trackables)
    {
        //Before we serialize we check for duplicates and provide a warning.
        for (int i = 0; i < trackables.Length; ++i)
        {
            string tNameA = trackables[i].TrackableName;

            // Ignore default names...
            if (tNameA == QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME)
                continue;

            for (int j = i + 1; j < trackables.Length; ++j)
            {
                string tNameB = trackables[j].TrackableName;

                // Ignore default names...
                if (tNameB == QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME)
                    continue;
                
                // We need to handle data set Trackables differently than markers.
                if (((trackables[i] is DataSetTrackableBehaviour) &&
                    (trackables[j] is DataSetTrackableBehaviour)))
                {
                    IEditorDataSetTrackableBehaviour castedTrackableA = (DataSetTrackableBehaviour)trackables[i];
                    IEditorDataSetTrackableBehaviour castedTrackableB = (DataSetTrackableBehaviour)trackables[j];

                    string tDataA = castedTrackableA.DataSetName;
                    string tDataB = castedTrackableB.DataSetName;

                    // Ignore Data Set Trackables that don't belong to the same data set.
                    if (tDataA.IndexOf(tDataB) != 0)
                    {
                        continue;
                    }
                }
                else if (!((trackables[i] is MarkerBehaviour) &&
                         (trackables[j] is MarkerBehaviour)))
                {
                    // Ignore trackables that are of different type.
                    // Note: Multi Targets and Image Targets still need to have
                    //       different names.
                    continue;
                }

                if (tNameA == tNameB)
                {
                    Debug.LogWarning("Duplicate Trackables detected: \"" +
                                     tNameA +
                                     "\". Only one of the Trackables and its respective Augmentation " +
                                     "will be selected for use at runtime - " +
                                     "that selection is indeterminate here.");
                }
            }
        }
    }


    // Correct scales of Trackables (make them uniform).
    private bool CorrectTrackableScales(TrackableBehaviour[] trackables)
    {
        bool scaleCorrected = false;
        foreach (IEditorTrackableBehaviour trackable in trackables)
        {
            if (trackable.CorrectScale())
            {
                scaleCorrected = true;
            }
        }
        return scaleCorrected;
    }

    #endregion // PRIVATE_METHODS
}