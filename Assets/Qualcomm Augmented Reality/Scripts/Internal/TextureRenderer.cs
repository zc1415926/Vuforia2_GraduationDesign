/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Helper class that renders a given texture into a RenderTexture on demand.
/// Used to buffer frames coming from the web cam stream
/// </summary>
public class TextureRenderer
{
    #region PRIVATE_MEMBER_VARIABLES

    // camera used to render buffered frames:
    private Camera mTextureBufferCamera = null;
    private int mTextureWidth;
    private int mTextureHeight;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    public int Width
    {
        get { return mTextureWidth; }
    }

    public int Height
    {
        get { return mTextureHeight; }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    // sets up all gameobjects needed to render frames, including a mesh with the correct material
    public TextureRenderer(Texture textureToRender, int renderTextureLayer, QCARRenderer.Vec2I requestedTextureSize)
    {
        if (renderTextureLayer > 31)
        {
            Debug.LogError("WebCamBehaviour.SetupTextureBufferCamera: configured layer > 31 is not supported by Unity!");
            return;
        }

        mTextureWidth = requestedTextureSize.x;
        mTextureHeight = requestedTextureSize.y;

        float halfMeshHeight = (mTextureHeight / (float)mTextureWidth) * 0.5f;

        // camera object:
        GameObject texBufferGameObj = new GameObject("TextureBufferCamera");
        mTextureBufferCamera = texBufferGameObj.AddComponent<Camera>();
        mTextureBufferCamera.isOrthoGraphic = true;
        mTextureBufferCamera.orthographicSize = halfMeshHeight;
        mTextureBufferCamera.aspect = mTextureWidth / (float)mTextureHeight;
        mTextureBufferCamera.nearClipPlane = 0.5f;
        mTextureBufferCamera.farClipPlane = 1.5f;
        mTextureBufferCamera.cullingMask = (1 << renderTextureLayer);
        mTextureBufferCamera.enabled = false; // camera will only render on demand!!

        // mesh to display the given texture
        GameObject textureBufferMesh = new GameObject("TextureBufferMesh", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
        textureBufferMesh.transform.parent = texBufferGameObj.transform;
        textureBufferMesh.layer = renderTextureLayer;

        Mesh mesh = new Mesh
        {
            vertices = new[] 
                        {
                            new Vector3(-0.5f, halfMeshHeight, 1f),
                            new Vector3(0.5f, halfMeshHeight, 1f),  
                            new Vector3(-0.5f, -halfMeshHeight, 1f),  
                            new Vector3(0.5f, -halfMeshHeight, 1f),            
                        },
            uv = new[]
                        {
                            new Vector2(0f, 0f),
                            new Vector2(1f, 0f), 
                            new Vector2(0f, 1f),
                            new Vector2(1f, 1f),
                        },
            triangles = new[]
                        {
                            0,1,2,
                            2,1,3
                        }
        };

        // renderer and material
        MeshRenderer meshRenderer = textureBufferMesh.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        meshRenderer.material.mainTexture = textureToRender;
        MeshFilter meshFilter = textureBufferMesh.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    // renders the given texture once and returns the RenderTexture it was rendered to
    public RenderTexture Render()
    {
        // create buffered frame
        RenderTexture bufferedFrame = RenderTexture.GetTemporary(mTextureWidth, mTextureHeight);
        mTextureBufferCamera.targetTexture = bufferedFrame;
        mTextureBufferCamera.Render();

        return bufferedFrame;
    }

    // destroys the gameobject hierarchy that was set up in constructor
    public void Destroy()
    {
        if (mTextureBufferCamera != null)
            Object.Destroy(mTextureBufferCamera.gameObject);
    }

    #endregion // PUBLIC_METHODS
}