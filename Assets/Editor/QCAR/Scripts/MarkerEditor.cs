/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MarkerBehaviour))]
public class MarkerEditor : Editor
{
    #region PUBLIC_METHODS

    // Updates the scale values in the transform component from a given size.
    public static void UpdateScale(IEditorMarkerBehaviour marker, Vector2 size)
    {
        // Update the scale.
        float childScaleFactor = marker.transform.localScale.x / size[0];

        marker.transform.localScale = new Vector3(size[0], size[0], size[0]);

        // Check if 3D content should keep its size or if it should be scaled
        // with the target.
        if (marker.PreserveChildSize)
        {
            foreach (Transform child in marker.transform)
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


    // Create a mesh with size 1, 1.
    public static void CreateMesh(MarkerBehaviour marker)
    {
        GameObject markerObject = marker.gameObject;

        MeshFilter meshFilter = markerObject.GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            meshFilter = markerObject.AddComponent<MeshFilter>();
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
                new Vector2(0,1),
                new Vector2(1,0),
                new Vector2(1,1)
                };

        // Add empty normals array.
        mesh.normals = new Vector3[mesh.vertices.Length];

        // Automatically calculate normals.
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = markerObject.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            meshRenderer = markerObject.AddComponent<MeshRenderer>();
        }

        // Cleanup assets that have been created temporarily.
        EditorUtility.UnloadUnusedAssets();
    }

    #endregion // PUBLIC_METHODS



    #region UNITY_EDITOR_METHODS

    // Initializes the Marker when it is drag-dropped into the scene.
    public void OnEnable()
    {
        MarkerBehaviour markerBehaviour = (MarkerBehaviour)target;

        // We don't want to initialize if this is a prefab.
        if (QCARUtilities.GetPrefabType(markerBehaviour) == PrefabType.Prefab)
        {
            return;
        }

        // Initialize scene manager
        if (!SceneManager.Instance.SceneInitialized)
        {
            SceneManager.Instance.InitScene();
        }

        IEditorMarkerBehaviour editorMarkerBehaviour = markerBehaviour;

        // Only setup marker if it has not been set up previously.
        if (!editorMarkerBehaviour.InitializedInEditor)
        {
            editorMarkerBehaviour.SetMarkerID(SceneManager.Instance.GetNextFrameMarkerID());

            CreateMesh(markerBehaviour);
            editorMarkerBehaviour.SetNameForTrackable("FrameMarker" + editorMarkerBehaviour.MarkerID);
            markerBehaviour.name = "FrameMarker" + editorMarkerBehaviour.MarkerID;
            editorMarkerBehaviour.SetInitializedInEditor(true);

            // Inform Unity that the behaviour properties have changed.
            EditorUtility.SetDirty(markerBehaviour);

            // Inform the scene manager about the newly added marker (to get validation).
            SceneManager.Instance.SceneUpdated();
        }

        // Cache the current scale of the marker:
        editorMarkerBehaviour.SetPreviousScale(markerBehaviour.transform.localScale);
    }


    // Lets the user choose a Marker by specifying an ID.
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();

        DrawDefaultInspector();

        MarkerBehaviour mb = (MarkerBehaviour)target;
        IEditorMarkerBehaviour editorMb = mb;

        if (QCARUtilities.GetPrefabType(mb) == PrefabType.Prefab)
        {
            // Only allow initial values for prefabs.
            editorMb.SetMarkerID(-1);
            EditorGUILayout.IntField("Marker ID", editorMb.MarkerID);
            EditorGUILayout.Toggle("Preserve child size",
                                   editorMb.PreserveChildSize);
        }
        else
        {
            int newMarkerID = EditorGUILayout.IntField("Marker ID",
                                                       editorMb.MarkerID);

            if (newMarkerID < 0)
            {
                newMarkerID = 0;
            }
            else if (newMarkerID >=
                QCARUtilities.GlobalVars.MAX_NUM_FRAME_MARKERS)
            {
                newMarkerID =
                    QCARUtilities.GlobalVars.MAX_NUM_FRAME_MARKERS - 1;
            }

            if (newMarkerID != editorMb.MarkerID)
            {
                editorMb.SetMarkerID(newMarkerID);
                editorMb.SetNameForTrackable("FrameMarker" + newMarkerID);
            }

            editorMb.SetPreserveChildSize(EditorGUILayout.Toggle("Preserve child size",
                                       editorMb.PreserveChildSize));
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(mb);

            SceneManager.Instance.SceneUpdated();
        }
    }

    #endregion // UNITY_EDITOR_METHODS
}