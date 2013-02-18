/*==============================================================================
            Copyright (c) 2012 Qualcomm Austria Research Center GmbH.
            All Rights Reserved.
            Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

public class BGRenderingBehaviour : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    public Camera Camera = null;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private QCARRenderer.VideoTextureInfo mTextureInfo;

    private ScreenOrientation mScreenOrientation;
    private int mScreenWidth = 0;
    private int mScreenHeight = 0;

    private bool mFlipHorizontally;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    public void CheckAndSetActive(bool isActive)
    {
        // turn on and off camera and  depending if Background Rendering has been enabled or not
        // not a redundant check, since enabling gameobjects does a lot internally, so we should not do it every frame.
        // ReSharper disable RedundantCheckBeforeAssignment
        if (Camera.gameObject.active != isActive)
            Camera.gameObject.SetActiveRecursively(isActive);
        // ReSharper restore RedundantCheckBeforeAssignment
    }

    // sets an external texture to be rendered as the background
    public void SetTexture(Texture texture)
    {
        // Assign texture to the renderer
        renderer.material.mainTexture = texture;
    }

    // if the image should be rendered flip (necessary for some front facing cameras)
    public void SetFlipHorizontally(bool flip)
    {
        mFlipHorizontally = flip;
    }

    #endregion // PUBLIC_METHODS



    #region UNITY_MONOBEHAVIOUR_METHODS

    // Use this for initialization
    void Start()
    {
        // Use the main camera if one wasn't set in the Inspector
        if (Camera == null)
        {
            Camera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Setup the geometry and orthographic camera as soon as the video
        // background info is available.
        if (QCARRenderer.Instance.IsVideoBackgroundInfoAvailable())
        {
            // Check if we need to update the texture:
            QCARRenderer.VideoTextureInfo texInfo = QCARRenderer.Instance.GetVideoTextureInfo();
            if (!mTextureInfo.imageSize.Equals(texInfo.imageSize) ||
                !mTextureInfo.textureSize.Equals(texInfo.textureSize))
            {
                // Cache the info:
                mTextureInfo = texInfo;

                Debug.Log("VideoTextureInfo " + texInfo.textureSize.x + " " +
                    texInfo.textureSize.y + " " + texInfo.imageSize.x + " " + texInfo.imageSize.y);

                // Create the video mesh
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }

                meshFilter.mesh = CreateVideoMesh();

                // Position the video mesh
                PositionVideoMesh();
            }
            else if (mScreenOrientation != QCARRuntimeUtilities.ScreenOrientation ||
                     mScreenWidth != Screen.width ||
                     mScreenHeight != Screen.height)
            {
                // Position the video mesh
                PositionVideoMesh();
            }
        }
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS

    // Create a video mesh with the given number of rows and columns
    // Minimum two rows and two columns
    private Mesh CreateVideoMesh()
    {
        const int numRows = 2;
        const int numCols = 2;

        Mesh mesh = new Mesh();

        // Build mesh:
        mesh.vertices = new Vector3[numRows * numCols];
        Vector3[] vertices = mesh.vertices;

        for (int r = 0; r < numRows; ++r)
        {
            for (int c = 0; c < numCols; ++c)
            {
                float x = (((float)c) / (float)(numCols-1)) -0.5F;
                float z = (1.0F - ((float)r) / (float)(numRows-1)) - 0.5F;

                vertices[r * numCols + c].x = x * 2.0F;
                vertices[r * numCols + c].y = 0.0F;
                vertices[r * numCols + c].z = z * 2.0F;
            }
        }
        mesh.vertices = vertices;

        // Builds triangles:
        mesh.triangles = new int[numRows*numCols*2*3];
        int triangleIndex = 0;

        // Setup UVs to match texture info:
        float scaleFactorX = (float)mTextureInfo.imageSize.x / (float)mTextureInfo.textureSize.x;
        float scaleFactorY = (float)mTextureInfo.imageSize.y / (float)mTextureInfo.textureSize.y;

        mesh.uv = new Vector2[numRows * numCols];

        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        for (int r = 0; r < numRows-1; ++r)
        {
            for (int c = 0; c < numCols-1; ++c)
            {
                // p0-p3
                // |\ |
                // p2-p1

                int p0Index = r * numCols + c;
                int p1Index = r * numCols + c + numCols + 1;
                int p2Index = r * numCols + c + numCols;
                int p3Index = r * numCols + c + 1;

                triangles[triangleIndex++] = p0Index;
                triangles[triangleIndex++] = p1Index;
                triangles[triangleIndex++] = p2Index;

                triangles[triangleIndex++] = p1Index;
                triangles[triangleIndex++] = p0Index;
                triangles[triangleIndex++] = p3Index;

                uvs[p0Index] = new Vector2(((float)c) / ((float)(numCols-1)) * scaleFactorX,
                                                ((float)r) / ((float)(numRows-1)) * scaleFactorY);

                uvs[p1Index] = new Vector2(((float)(c + 1)) / ((float)(numCols - 1)) * scaleFactorX,
                                ((float)(r + 1)) / ((float)(numRows - 1)) * scaleFactorY);

                uvs[p2Index] = new Vector2(((float)c) / ((float)(numCols - 1)) * scaleFactorX,
                            ((float)(r + 1)) / ((float)(numRows - 1)) * scaleFactorY);

                uvs[p3Index] = new Vector2(((float)(c + 1)) / ((float)(numCols - 1)) * scaleFactorX,
                            ((float)r) / ((float)(numRows - 1)) * scaleFactorY);

                if (mFlipHorizontally)
                {
                    uvs[p0Index].x = 1 - uvs[p0Index].x;
                    uvs[p1Index].x = 1 - uvs[p1Index].x;
                    uvs[p2Index].x = 1 - uvs[p2Index].x;
                    uvs[p3Index].x = 1 - uvs[p3Index].x;
                }
            }
        }

        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.normals = new Vector3[mesh.vertices.Length];
        mesh.RecalculateNormals();

        return mesh;
    }

    // Scale and position the video mesh to fill the screen
    private void PositionVideoMesh()
    {
        // Cache the screen orientation and size
        mScreenOrientation = QCARRuntimeUtilities.ScreenOrientation;
        mScreenWidth = Screen.width;
        mScreenHeight = Screen.height;

        // Reset the rotation so the mesh faces the camera
        gameObject.transform.localRotation = Quaternion.AngleAxis(270.0f, Vector3.right);

        // Adjust the rotation for the current orientation
        if (mScreenOrientation == ScreenOrientation.Landscape)
        {
            gameObject.transform.localRotation *= Quaternion.identity;
        }
        else if (mScreenOrientation == ScreenOrientation.Portrait)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
        }
        else if (mScreenOrientation == ScreenOrientation.LandscapeRight)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(180.0f, Vector3.up);
        }
        else if (mScreenOrientation == ScreenOrientation.PortraitUpsideDown)
        {
            gameObject.transform.localRotation *= Quaternion.AngleAxis(270.0f, Vector3.up);
        }
        
        // Scale game object for full screen video image:
        gameObject.transform.localScale = new Vector3(1, 1, 1 * (float)mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x);

        // Set the scale of the orthographic camera to match the screen size:
        Camera.orthographic = true;

        // Visible portion of the image:
        float visibleHeight;
        if (ShouldFitWidth())
        {
            // should fit width is true, so we have to adjust the horizontal autographic size so that
            // the viewport covers the whole texture WIDTH.
            if (QCARRuntimeUtilities.IsPortraitOrientation)
            {
                // in portrait mode, the background is rotated by 90 degrees. It's actual height is
                // therefore 1, so we have to set the visible height so that the visible width results in 1.
                visibleHeight = (mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x) *
                                ((float)mScreenHeight / (float)mScreenWidth);
            }
            else
            {
                // in landscape mode, we have to set the visible height to the screen ratio to
                // end up with a visible width of 1.
                visibleHeight = (float)mScreenHeight / (float)mScreenWidth;
            }
        }
        else
        {
            // should fit width is true, so we have to adjust the horizontal autographic size so that
            // the viewport covers the whole texture HEIGHT.
            if (QCARRuntimeUtilities.IsPortraitOrientation)
            {
                // in portrait mode, texture height is 1
                visibleHeight = 1.0f;
            }
            else
            {
                // in landscape mode, the texture height will be this value (see above)
                visibleHeight = mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x;
            }
        }

        Camera.orthographicSize = visibleHeight;
    }

    // Returns true if the video mesh should be scaled to match the width of the screen
    // Returns false if the video mesh should be scaled to match the height of the screen
    private bool ShouldFitWidth()
    {
        float screenAspect = mScreenWidth / (float)mScreenHeight;
        float cameraAspect;
        if (QCARRuntimeUtilities.IsPortraitOrientation)
            cameraAspect = mTextureInfo.imageSize.y / (float)mTextureInfo.imageSize.x;
        else
            cameraAspect = mTextureInfo.imageSize.x / (float)mTextureInfo.imageSize.y;

        return (screenAspect >= cameraAspect);
    }

    #endregion // PRIVATE_METHODS
}
