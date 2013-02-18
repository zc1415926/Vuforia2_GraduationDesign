/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VirtualButtonBehaviour))]
public class VirtualButtonEditor : Editor
{
    #region PUBLIC_METHODS

    // This methods validates all Virtual Buttons in the scene and prints
    // errors accordingly.
    public static void Validate()
    {
        // First check for duplicate virtual buttons in all image targets
        ImageTargetBehaviour[] its = (ImageTargetBehaviour[])
            UnityEngine.Object.FindObjectsOfType(typeof(ImageTargetBehaviour));

        foreach (ImageTargetBehaviour it in its)
        {
            DetectDuplicates(it);
        }

        // Check for Virtual Buttons that don't have an Image Target as an
        // ancestor:
        VirtualButtonBehaviour[] vbs = (VirtualButtonBehaviour[])
            UnityEngine.Object.FindObjectsOfType(
            typeof(VirtualButtonBehaviour));

        foreach (VirtualButtonBehaviour vb in vbs)
        {
            IEditorImageTargetBehaviour it = vb.GetImageTargetBehaviour();
            if (it == null)
            {
                Debug.LogError("Virtual Button '" + vb.name + "' doesn't " +
                    "have an Image Target as an ancestor.");
            }
            else
            {
                if (it.ImageTargetType == ImageTargetType.USER_DEFINED)
                    Debug.LogError("Virtual Button '" + vb.name + "' cannot be added to a user defined target.");
            }
        }
    }


    // Correct Virtual Button Poses.
    public static bool CorrectPoses(IEditorVirtualButtonBehaviour[] vbs)
    {
        bool posesUpdated = false;
        foreach (IEditorVirtualButtonBehaviour vb in vbs)
        {
            // Check if Virtual Button pose has changed in scene or has never
            // been updated
            if (vb.PreviousTransform != vb.transform.localToWorldMatrix ||
                (vb.transform.parent != null && vb.PreviousParent !=
                vb.transform.parent.gameObject) ||
                !vb.HasUpdatedPose)
            {
                // Update the buttons pose
                if (vb.UpdatePose())
                {
                    // The button area has changed, need to serialize:
                    posesUpdated = true;
                }

                vb.SetPreviousTransform(vb.transform.localToWorldMatrix);
                vb.SetPreviousParent(vb.transform.parent ?
                            vb.transform.parent.gameObject : null);
            }
        }
        return posesUpdated;
    }


    // Create a mesh with size 1, 1.
    public static void CreateVBMesh(IEditorVirtualButtonBehaviour vb)
    {
        GameObject vbObject = vb.gameObject;

        MeshFilter meshFilter = vbObject.GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            meshFilter = vbObject.AddComponent<MeshFilter>();
        }

        // Setup vertex positions.
        Vector3 p0 = new Vector3(-0.5f, 0, -0.5f);
        Vector3 p1 = new Vector3(-0.5f, 0, 0.5f);
        Vector3 p2 = new Vector3(0.5f, 0, -0.5f);
        Vector3 p3 = new Vector3(0.5f, 0, 0.5f);

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { p0, p1, p2, p3 };
        mesh.triangles = new int[]  {
                                        0,1,2,
                                        2,1,3
                                    };

        // Add UV coordinates.
        mesh.uv = new Vector2[]{
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
                };

        // Add empty normals array.
        mesh.normals = new Vector3[mesh.vertices.Length];

        // Automatically calculate normals.
        mesh.RecalculateNormals();
        mesh.name = "VBPlane";

        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = vbObject.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            meshRenderer = vbObject.AddComponent<MeshRenderer>();
        }

        meshRenderer.sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath(
                        QCARUtilities.GlobalVars.VIRTUAL_BUTTON_MATERIAL_PATH,
                        typeof(Material));

        EditorUtility.UnloadUnusedAssets();
    }


    // Assign default material to Virtual Button.
    public static void CreateMaterial(IEditorVirtualButtonBehaviour vb)
    {
        // Load reference material
        string referenceMaterialPath =
            QCARUtilities.GlobalVars.VIRTUAL_BUTTON_MATERIAL_PATH;
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

        // If the texture is null we simply assign a default material
        vb.renderer.sharedMaterial = referenceMaterial;

        // Cleanup assets that have been created temporarily.
        EditorUtility.UnloadUnusedAssets();
    }


    //// Sets position and scale in the transform component of the Virtual Button
    //// game object. The values are calculated from rectangle values (top-left
    //// and bottom-right corners).
    //// Returns false if Virtual Button is not child of an Image Target.
    //public static bool SetPosAndScaleFromButtonArea(Vector2 topLeft,
    //                                                Vector2 bottomRight,
    //                                                ImageTargetBehaviour it,
    //                                                VirtualButtonBehaviour vb)
    //{
    //    if (it == null)
    //    {
    //        return false;
    //    }

    //    float itScale = it.transform.lossyScale[0];

    //    Vector2 pos = (topLeft + bottomRight) * 0.5f;

    //    Vector2 scale = new Vector2(bottomRight[0] - topLeft[0],
    //                                topLeft[1] - bottomRight[1]);

    //    Vector3 vbPosITSpace =
    //        new Vector3(pos[0] / itScale, VirtualButtonBehaviour.TARGET_OFFSET,
    //                    pos[1] / itScale);


    //    Vector3 vbScaleITSpace =
    //        new Vector3(scale[0],
    //                    (scale[0] + scale[1]) * 0.5f,
    //                    scale[1]);

    //    vb.transform.position = it.transform.TransformPoint(vbPosITSpace);

    //    // Image Target scale is canceled out (included in both scales)
    //    vb.transform.localScale =
    //        vbScaleITSpace / vb.transform.parent.lossyScale[0];

    //    // Done:
    //    return true;
    //}

    #endregion // PUBLIC_METHODS



    #region UNITY_EDITOR_METHODS

    // Initializes the Virtual Button when it is drag-dropped into the scene.
    public void OnEnable()
    {
        if (QCARUtilities.GetPrefabType(target) == PrefabType.Prefab)
        {
            return;
        }

        if (!SceneManager.Instance.SceneInitialized)
        {
            SceneManager.Instance.InitScene();
        }

        // Create the virtual button mesh and update pose:
        VirtualButtonBehaviour vb = (VirtualButtonBehaviour)target;

        // Update the pose if required:
        if (!vb.HasUpdatedPose)
        {
            vb.UpdatePose();
        }

        // Create the mesh for visualization if required:
        if (!IsVBMeshCreated(vb))
        {
            CreateVBMesh(vb);
        }

        // Validate virtual buttons:
        Validate();
    }


    // Lets the user set sensitivity and name of a Virtual Button.
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();

        DrawDefaultInspector();

        VirtualButtonBehaviour vb = (VirtualButtonBehaviour)target;
        IEditorVirtualButtonBehaviour editorVB = vb;

        editorVB.SetVirtualButtonName(EditorGUILayout.TextField(
                            "Name", editorVB.VirtualButtonName));

        editorVB.SetSensitivitySetting((VirtualButton.Sensitivity)EditorGUILayout.EnumPopup(
                "Sensitivity Setting", editorVB.SensitivitySetting));

        if (GUI.changed)
        {
            // Let Unity know that there is new data for serialization.
            EditorUtility.SetDirty(vb);
        }
    }


    // Locks the y-scale of a Virtual Button at 1.
    public void OnSceneGUI()
    {
        VirtualButtonBehaviour vb = (VirtualButtonBehaviour)target;

        vb.transform.localScale = new Vector3(vb.transform.localScale[0],
                                              1.0f,
                                              vb.transform.localScale[2]);
    }

    #endregion // UNITY_EDITOR_METHODS



    #region PRIVATE_METHODS

    // This method checks for duplicate virtual buttons in a given image
    // target and prints an error accordingly.
    private static void DetectDuplicates(ImageTargetBehaviour it)
    {
        IEditorImageTargetBehaviour editorIt = it;
        VirtualButtonBehaviour[] vbs =
                        it.GetComponentsInChildren<VirtualButtonBehaviour>();

        for (int i = 0; i < vbs.Length; ++i)
        {
            for (int j = i + 1; j < vbs.Length; ++j)
            {
                if (vbs[i].VirtualButtonName == vbs[j].VirtualButtonName)
                {
                    Debug.LogError("Duplicate virtual buttons with name '" +
                        vbs[i].VirtualButtonName + "' detected in " +
                        "Image Target '" + editorIt.TrackableName + "'.");
                }
            }
        }
    }


    // This method checks if the Virtual Button already contains a mesh
    // component.
    private static bool IsVBMeshCreated(VirtualButtonBehaviour vb)
    {
        GameObject vbObject = vb.gameObject;
        MeshFilter meshFilter = vbObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = vbObject.GetComponent<MeshRenderer>();
        if (meshFilter == null || meshRenderer == null
            || meshFilter.sharedMesh == null)
        {
            return false;
        }

        return true;
    }

    #endregion // PRIVATE_METHODS
}