/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImageTargetBehaviour))]
public class ImageTargetEditor : Editor
{

    #region PUBLIC_METHODS

    // Recalculates the aspect ratio of the Image Target from a size vector.
    // Automatically updates mesh as well.
    public static void UpdateAspectRatio(IEditorImageTargetBehaviour it, Vector2 size)
    {
        it.SetAspectRatio(size[1] / size[0]);
        UpdateMesh(it);
    }


    // Updates the scale values in the transform component from a given size.
    public static void UpdateScale(IEditorImageTargetBehaviour it, Vector2 size)
    {
        // Update the scale:

        float childScaleFactor = it.GetSize()[0] / size[0];

        if (it.AspectRatio <= 1.0f)
        {
            it.transform.localScale = new Vector3(size[0], size[0], size[0]);
        }
        else
        {
            it.transform.localScale = new Vector3(size[1], size[1], size[1]);
        }

        // Check if 3D content should keep its size or if it should be scaled
        // with the target.
        if (it.PreserveChildSize)
        {
            foreach (Transform child in it.transform)
            {
                child.localPosition =
                    new Vector3(child.localPosition.x * childScaleFactor,
                                child.localPosition.y * childScaleFactor,
                                child.localPosition.z * childScaleFactor);

                child.localScale =
                    new Vector3(child.localScale.x * childScaleFactor,
                                child.localScale.y * childScaleFactor,
                                child.localScale.z * childScaleFactor);
            }
        }
    }


    // Assign material and texture to Image Target.
    public static void UpdateMaterial(IEditorImageTargetBehaviour it)
    {
        // Load reference material.
        string referenceMaterialPath =
            QCARUtilities.GlobalVars.REFERENCE_MATERIAL_PATH;
        Material referenceMaterial =
            (Material)AssetDatabase.LoadAssetAtPath(referenceMaterialPath,
                                                    typeof(Material));
        if (referenceMaterial == null)
        {
            Debug.LogError("Could not find reference material at " +
                           referenceMaterialPath +
                           " please reimport Unity package.");
            return;
        }

        // Load texture from texture folder. Textures have per convention the
        // same name as Image Targets + "_scaled" as postfix.
        string pathToTexture = QCARUtilities.GlobalVars.TARGET_TEXTURES_PATH;
        string textureFile = pathToTexture + it.DataSetName + "/" + it.TrackableName + "_scaled";

        if (File.Exists(textureFile + ".png"))
            textureFile += ".png";
        else if (File.Exists(textureFile + ".jpg"))
            textureFile += ".jpg";

        Texture2D targetTexture =
            (Texture2D)AssetDatabase.LoadAssetAtPath(textureFile,
                                                     typeof(Texture2D));

        if (targetTexture == null)
        {
            if (it.ImageTargetType == ImageTargetType.USER_DEFINED)
                // show the UserDefinedTarget default texture in case it's a user defined target
                it.renderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath(QCARUtilities.GlobalVars.UDT_MATERIAL_PATH, typeof(Material));
            else if (it.ImageTargetType == ImageTargetType.CLOUD_RECO)
                // show the cloud reco default texture in case it's a cloud reco target
                it.renderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath(QCARUtilities.GlobalVars.CL_MATERIAL_PATH, typeof(Material));
            else
                // If the texture is null we simply assign a default material.
                it.renderer.sharedMaterial = referenceMaterial;

            return;
        }

        // We create a new material that is based on the reference material but
        // also contains a texture.
        Material materialForTargetTexture = new Material(referenceMaterial);
        materialForTargetTexture.mainTexture = targetTexture;
        materialForTargetTexture.name = targetTexture.name + "Material";
        materialForTargetTexture.mainTextureScale = new Vector2(1, 1);

        it.renderer.sharedMaterial = materialForTargetTexture;

        // Cleanup assets that have been created temporarily.
        EditorUtility.UnloadUnusedAssets();
    }


    // Update Virtual Buttons from configuration data.
    public static void UpdateVirtualButtons(ImageTargetBehaviour it,
                                            ConfigData.VirtualButtonData[] vbs)
    {
        for (int i = 0; i < vbs.Length; ++i)
        {
            // Developer is responsible for deleting Virtual Buttons that
            // are not specified in newly imported config.xml files.
            IEditorVirtualButtonBehaviour[] vbBehaviours =
                it.GetComponentsInChildren<VirtualButtonBehaviour>();

            bool vbInScene = false;
            for (int j = 0; j < vbBehaviours.Length; ++j)
            {
                // If the Virtual Button specified in the config.xml file
                // is already part of the scene we fill it with new values.
                if (vbBehaviours[j].VirtualButtonName == vbs[i].name)
                {
                    vbBehaviours[j].enabled = vbs[i].enabled;
                    vbBehaviours[j].SetSensitivitySetting(vbs[i].sensitivity);
                    vbBehaviours[j].SetPosAndScaleFromButtonArea(
                        new Vector2(vbs[i].rectangle[0], vbs[i].rectangle[1]),
                        new Vector2(vbs[i].rectangle[2], vbs[i].rectangle[3]));
                    vbInScene = true;
                }
            }
            if (vbInScene)
            {
                continue;
            }

            // If Virtual Button is not part of the scene we create
            // a new one and add it as a direct child of the ImageTarget.
            ImageTargetEditor.AddVirtualButton(it, vbs[i]);
        }
    }


    // Add Virtual Buttons that are specified in the configuration data.
    public static void AddVirtualButtons(ImageTargetBehaviour it,
                                         ConfigData.VirtualButtonData[] vbs)
    {
        for (int i = 0; i < vbs.Length; ++i)
        {
            AddVirtualButton(it, vbs[i]);
        }
    }

    #endregion // PUBLIC_METHODS



    #region UNITY_EDITOR_METHODS

    // Initializes the Image Target when it is drag-dropped into the scene.
    public void OnEnable()
    {
        ImageTargetBehaviour itb = (ImageTargetBehaviour) target;

        // We don't want to initialize if this is a prefab.
        if (QCARUtilities.GetPrefabType(itb) == PrefabType.Prefab)
        {
            return;
        }

        // Make sure the scene and config.xml file are synchronized.
        if (!SceneManager.Instance.SceneInitialized)
        {
            SceneManager.Instance.InitScene();
        }

        IEditorImageTargetBehaviour editorItb = itb;

        // Only setup target if it has not been set up previously.
        if (!editorItb.InitializedInEditor)
        {
            ConfigData.ImageTargetData itConfig;

            ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME);
            dataSetData.GetImageTarget(QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME, out itConfig);

            UpdateAspectRatio(itb, itConfig.size);
            UpdateScale(itb, itConfig.size);
            UpdateMaterial(itb);
            editorItb.SetDataSetPath(QCARUtilities.GlobalVars.DEFAULT_DATA_SET_NAME);
            editorItb.SetNameForTrackable(QCARUtilities.GlobalVars.DEFAULT_TRACKABLE_NAME);
            editorItb.SetInitializedInEditor(true);
        }

        // Cache the current scale of the target:
        editorItb.SetPreviousScale(itb.transform.localScale);
    }


    // Lets the user choose a Image Target from a drop down list. Image Target
    // must be defined in the "config.xml" file.
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();

        DrawDefaultInspector();

        ImageTargetBehaviour itb = (ImageTargetBehaviour)target;
        IEditorImageTargetBehaviour editorItb = itb;

        if (QCARUtilities.GetPrefabType(itb) == PrefabType.Prefab)
        {
            GUILayout.Label("You can't choose a target for a prefab.");
        }
        else
        {
            ImageTargetType oldType = editorItb.ImageTargetType;

            // show dropdown for type selection (predefined TMS target vs. user defined target)
            string[] typeNames =
                new[] { QCARUtilities.GlobalVars.PREDEFINED_TARGET_DROPDOWN_TEXT, 
                        QCARUtilities.GlobalVars.USER_CREATED_TARGET_DROPDOWN_TEXT, 
                        QCARUtilities.GlobalVars.CLOUD_RECO_DROPDOWN_TEXT };
            ImageTargetType[] typeValues = 
                new [] { ImageTargetType.PREDEFINED, 
                         ImageTargetType.USER_DEFINED,
                         ImageTargetType.CLOUD_RECO};
            editorItb.SetImageTargetType(typeValues[EditorGUILayout.Popup("Type", typeValues.ToList().IndexOf(editorItb.ImageTargetType), typeNames)]);

            bool typeChanged = editorItb.ImageTargetType != oldType;

            if (editorItb.ImageTargetType == ImageTargetType.PREDEFINED)
                DrawPredefinedTargetInsprectorUI(itb, typeChanged);
            else if (editorItb.ImageTargetType == ImageTargetType.USER_DEFINED)
                DrawUserDefinedTargetInspectorUI(itb, typeChanged);
            else
                DrawCloudRecoTargetInspectorUI(itb, typeChanged);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(itb);

            SceneManager.Instance.SceneUpdated();
        }
    }

    #endregion // UNITY_EDITOR_METHODS



    #region PRIVATE_METHODS

    private static void AddVirtualButton(ImageTargetBehaviour it,
                                         ConfigData.VirtualButtonData vb)
    {
        IEditorVirtualButtonBehaviour newVBBehaviour =
            it.CreateVirtualButton(vb.name, new Vector2(0.0f, 0.0f),
                                   new Vector2(1.0f, 1.0f));
        if (newVBBehaviour != null)
        {
            newVBBehaviour.SetPosAndScaleFromButtonArea(
                new Vector2(vb.rectangle[0], vb.rectangle[1]),
                new Vector2(vb.rectangle[2], vb.rectangle[3]));

            VirtualButtonEditor.CreateVBMesh(newVBBehaviour);

            // Load default material.
            VirtualButtonEditor.CreateMaterial(newVBBehaviour);

            newVBBehaviour.enabled = vb.enabled;

            // Add Component to destroy VirtualButton meshes at runtime.
            newVBBehaviour.gameObject.AddComponent<TurnOffBehaviour>();

            // Make sure Virtual Button is correctly aligned with Image Target
            newVBBehaviour.UpdatePose();
        }
        else
        {
            Debug.LogError("VirtualButton could not be added!");
        }
    }


    private static void UpdateMesh(IEditorImageTargetBehaviour it)
    {
        GameObject itObject = it.gameObject;

        MeshFilter meshFilter = itObject.GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            meshFilter = itObject.AddComponent<MeshFilter>();
        }

        Vector3 p0, p1, p2, p3;

        if (it.AspectRatio <= 1.0f)
        {
            p0 = new Vector3(-0.5f, 0, -it.AspectRatio * 0.5f);
            p1 = new Vector3(-0.5f, 0, it.AspectRatio * 0.5f);
            p2 = new Vector3(0.5f, 0, -it.AspectRatio * 0.5f);
            p3 = new Vector3(0.5f, 0, it.AspectRatio * 0.5f);
        }
        else
        {
            float aspectRationInv = 1.0f / it.AspectRatio;

            p0 = new Vector3(-aspectRationInv * 0.5f, 0, -0.5f);
            p1 = new Vector3(-aspectRationInv * 0.5f, 0, 0.5f);
            p2 = new Vector3(aspectRationInv * 0.5f, 0, -0.5f);
            p3 = new Vector3(aspectRationInv * 0.5f, 0, 0.5f);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { p0, p1, p2, p3 };
        mesh.triangles = new int[]  {
                                        0,1,2,
                                        2,1,3
                                    };

        mesh.normals = new Vector3[mesh.vertices.Length];
        mesh.uv = new Vector2[]{
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,0),
                new Vector2(1,1)
                };

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = itObject.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            meshRenderer = itObject.AddComponent<MeshRenderer>();
        }

        // Cleanup assets that have been created temporarily.
        EditorUtility.UnloadUnusedAssets();
    }

    private void DrawCloudRecoTargetInspectorUI(IEditorImageTargetBehaviour itb, bool typeChanged)
    {
        if (typeChanged)
        {
            ConfigData.ImageTargetData itConfig = QCARUtilities.CreateDefaultImageTarget();
            itb.SetNameForTrackable(string.Empty);

            // Update the aspect ratio and mesh used for visualisation:
            UpdateAspectRatio(itb, itConfig.size);

            // Update the material:
            UpdateMaterial(itb);
        }

        // Draw check box to de-/activate "preserve child size" mode.
        itb.SetPreserveChildSize(EditorGUILayout.Toggle("Preserve child size", itb.PreserveChildSize));
    }

    private void DrawUserDefinedTargetInspectorUI(IEditorImageTargetBehaviour itb, bool typeChanged)
    {
        if (typeChanged)
        {
            ConfigData.ImageTargetData itConfig = QCARUtilities.CreateDefaultImageTarget();
            itb.SetNameForTrackable(string.Empty);

            // Update the aspect ratio and mesh used for visualisation:
            UpdateAspectRatio(itb, itConfig.size);

            // Update the material:
            UpdateMaterial(itb);
        }

        if (itb.TrackableName.Length > 64)
            EditorGUILayout.HelpBox("Target name must not exceed 64 character limit!", MessageType.Error);

        itb.SetNameForTrackable(EditorGUILayout.TextField("Target Name", itb.TrackableName));

        // Draw check box to de-/activate "preserve child size" mode.
        itb.SetPreserveChildSize(EditorGUILayout.Toggle("Preserve child size", itb.PreserveChildSize));
    }

    private void DrawPredefinedTargetInsprectorUI(IEditorImageTargetBehaviour itb, bool typeChanged)
    {
        if (typeChanged)
            UpdateMaterial(itb);

        if (ConfigDataManager.Instance.NumConfigDataObjects > 1) //< "> 1" because we ignore the default dataset.
        {
            // Draw list for choosing a data set.
            string[] dataSetList = new string[ConfigDataManager.Instance.NumConfigDataObjects];
            ConfigDataManager.Instance.GetConfigDataNames(dataSetList);
            int currentDataSetIndex = QCARUtilities.GetIndexFromString(itb.DataSetName, dataSetList);

            // If name is not in array we automatically choose default name;
            if (currentDataSetIndex < 0)
                currentDataSetIndex = 0;

            int newDataSetIndex = EditorGUILayout.Popup("Data Set",
                                                        currentDataSetIndex,
                                                        dataSetList);

            string chosenDataSet = dataSetList[newDataSetIndex];

            ConfigData dataSetData = ConfigDataManager.Instance.GetConfigData(chosenDataSet);

            // Draw list for choosing a Trackable
            int targetCount = dataSetData.NumImageTargets;
            string[] namesList = new string[targetCount];
            dataSetData.CopyImageTargetNames(namesList, 0);

            // get the current index
            int currentTrackableIndex = QCARUtilities.GetIndexFromString(itb.TrackableName, namesList);

            // If name is not in array we automatically choose default name;
            if (currentTrackableIndex < 0)
                currentTrackableIndex = 0;

            // Reset name index if a new data set has been chosen.
            if (newDataSetIndex != currentDataSetIndex)
            {
                currentTrackableIndex = 0;
            }

            int newTrackableIndex = EditorGUILayout.Popup("Image Target",
                                                            currentTrackableIndex,
                                                            namesList);

            // Draw check box to de-/activate "preserve child size" mode.
            itb.SetPreserveChildSize(EditorGUILayout.Toggle("Preserve child size", itb.PreserveChildSize));

            if (namesList.Length > 0)
            {
                if (newDataSetIndex != currentDataSetIndex || newTrackableIndex != currentTrackableIndex || typeChanged)
                {
                    itb.SetDataSetPath("QCAR/" + dataSetList[newDataSetIndex] + ".xml");

                    string selectedString = namesList[newTrackableIndex];

                    ConfigData.ImageTargetData itConfig;
                    itb.SetNameForTrackable(selectedString);
                    dataSetData.GetImageTarget(itb.TrackableName, out itConfig);
                    
                    // Update the aspect ratio and mesh used for visualisation:
                    UpdateAspectRatio(itb, itConfig.size);

                    // Update the material:
                    UpdateMaterial(itb);
                }
            }
        }
        else
        {
            if (GUILayout.Button("No targets defined. Press here for target " +
                                    "creation!"))
            {
                SceneManager.Instance.GoToARPage();
            }
        }
    }

    #endregion // PRIVATE_METHODS
}