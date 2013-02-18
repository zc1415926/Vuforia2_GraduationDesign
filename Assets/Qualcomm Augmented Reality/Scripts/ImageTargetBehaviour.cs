/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class serves both as an augmentation definition for an ImageTarget in the editor
/// as well as a tracked image target result at runtime
/// </summary>
public class ImageTargetBehaviour : DataSetTrackableBehaviour, IEditorImageTargetBehaviour
{
    #region PROPERTIES

    /// <summary>
    /// The image target that this ImageTargetBehaviour augments
    /// </summary>
    public ImageTarget ImageTarget
    {
        get { return mImageTarget; }
    }

    #endregion // PROPERTIES



    #region PRIVATE_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    private float mAspectRatio;

    [SerializeField]
    [HideInInspector]
    private ImageTargetType mImageTargetType;

    private ImageTarget mImageTarget;
    private Dictionary<int, VirtualButtonBehaviour> mVirtualButtonBehaviours;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    public ImageTargetBehaviour()
    {
        mAspectRatio = 1.0f;
    }

    #endregion // CONSTRUCTION



    #region PROTECTED_METHODS


    /// <summary>
    /// Scales the Trackable uniformly
    /// </summary>
    protected override bool CorrectScaleImpl()
    {
        bool scaleChanged = false;

        for (int i = 0; i < 3; ++i)
        {
            // Force uniform scale:
            if (this.transform.localScale[i] != mPreviousScale[i])
            {
                this.transform.localScale =
                    new Vector3(this.transform.localScale[i],
                                this.transform.localScale[i],
                                this.transform.localScale[i]);

                mPreviousScale = this.transform.localScale;
                scaleChanged = true;
                break;
            }
        }

        return scaleChanged;
    }

    /// <summary>
    /// This method disconnects the TrackableBehaviour from it's associated trackable.
    /// Use it only if you know what you are doing - e.g. when you want to destroy a trackable, but reuse the TrackableBehaviour.
    /// </summary>
    protected override void InternalUnregisterTrackable()
    {
        mTrackable = mImageTarget = null;
    }

    #endregion // PROTECTED_METHODS



    #region PUBLIC_METHODS

    /// <summary>
    /// This method creates a Virtual Button and adds it to this Image Target as
    /// a direct child.
    /// </summary>
    public VirtualButtonBehaviour CreateVirtualButton(string vbName,
                                                      Vector2 position,
                                                      Vector2 size)
    {
        GameObject virtualButtonObject = new GameObject(vbName);
        VirtualButtonBehaviour newVBB =
            virtualButtonObject.AddComponent<VirtualButtonBehaviour>();

        // Add Virtual Button to its parent game object
        virtualButtonObject.transform.parent = this.transform;

        // Set Virtual Button attributes
        IEditorVirtualButtonBehaviour newEditorVBB = newVBB;
        newEditorVBB.SetVirtualButtonName(vbName);
        newEditorVBB.transform.localScale = new Vector3(size.x, 1.0f, size.y);
        newEditorVBB.transform.localPosition = new Vector3(position.x, 1.0f,
                                                        position.y);

        // Only register the virtual button with the qcarBehaviour at run-time:
        if (Application.isPlaying)
        {
            if (!CreateNewVirtualButtonFromBehaviour(newVBB)) 
                return null;
        }
        
        // If we manually register the button it should be unregistered if the
        // Unity object is destroyed.
        newVBB.UnregisterOnDestroy = true;
        
        return newVBB;
    }


    /// <summary>
    /// This methods adds the Virtual Button as a child of "immediateParent".
    /// Returns null if "immediateParent" is not an Image Target or a child of an
    /// Image Target.
    /// </summary>
    public static VirtualButtonBehaviour CreateVirtualButton(string vbName,
                                                  Vector2 localScale,
                                                  GameObject immediateParent)
    {
        GameObject virtualButtonObject = new GameObject(vbName);
        VirtualButtonBehaviour newVBB =
            virtualButtonObject.AddComponent<VirtualButtonBehaviour>();

        GameObject rootParent = immediateParent.transform.root.gameObject;
        ImageTargetBehaviour parentImageTarget =
            rootParent.GetComponentInChildren<ImageTargetBehaviour>();

        if (parentImageTarget == null || parentImageTarget.ImageTarget == null)
        {
            Debug.LogError("Could not create Virtual Button. " +
                           "immediateParent\"immediateParent\" object is not " +
                           "an Image Target or a child of one.");
            GameObject.Destroy(virtualButtonObject);
            return null;
        }

        // Add Virtual Button to its parent game object
        virtualButtonObject.transform.parent = immediateParent.transform;

        // Set Virtual Button attributes
        IEditorVirtualButtonBehaviour newEditorVBB = newVBB;
        newEditorVBB.SetVirtualButtonName(vbName);
        newEditorVBB.transform.localScale = new Vector3(localScale[0], 1.0f, localScale[1]); 

        // Only register the virtual button with the qcarBehaviour at run-time:
        if (Application.isPlaying)
        {
            if (!parentImageTarget.CreateNewVirtualButtonFromBehaviour(newVBB))
                return null;
        }

        // If we manually register the button it should be unregistered if the
        // Unity object is destroyed.
        newVBB.UnregisterOnDestroy = true;

        return newVBB;
    }

    /// <summary>
    ///  Returns the virtual button behaviours for this imageTargetBehaviour
    /// </summary>
    public IEnumerable<VirtualButtonBehaviour> GetVirtualButtonBehaviours()
    {
        return mVirtualButtonBehaviours.Values;
    }


    /// <summary>
    /// Destroys the virtual button with the given name.
    /// </summary>
    public void DestroyVirtualButton(string vbName)
    {
        List<VirtualButtonBehaviour> virtualButtonBehaviours = new List<VirtualButtonBehaviour>(mVirtualButtonBehaviours.Values);
        foreach (VirtualButtonBehaviour vb in virtualButtonBehaviours)
        {
            if (vb.VirtualButtonName == vbName)
            {
                mVirtualButtonBehaviours.Remove(vb.VirtualButton.ID);
                // Unregister pre-existing buttons when explicitly destroyed
                vb.UnregisterOnDestroy = true;
                Destroy(vb.gameObject);
                return;
            }
        }
    }


    /// <summary>
    /// Returns the size of this target in scene units
    /// </summary>
    public Vector2 GetSize()
    {
        if (mAspectRatio <= 1.0f)
        {
            return new Vector2(transform.localScale.x,
                                transform.localScale.x * mAspectRatio);
        }
        else
        {
            return new Vector2(transform.localScale.x / mAspectRatio,
                                transform.localScale.x);
        }
    }

    #endregion // PUBLIC_METHODS



    #region EDITOR_INTERFACE_IMPLEMENTATION

    // The aspect ratio of the target.
    float IEditorImageTargetBehaviour.AspectRatio
    {
        get
        {
            return mAspectRatio;
        }
    }

    // If the image target is a user created target or a static one from a dataset
    ImageTargetType IEditorImageTargetBehaviour.ImageTargetType
    {
        get
        {
            return mImageTargetType;
        }
    }

    // sets the Aspect Ratio (only in editor mode)
    bool IEditorImageTargetBehaviour.SetAspectRatio(float aspectRatio)
    {
        if (mTrackable == null)
        {
            mAspectRatio = aspectRatio;
            return true;
        }
        return false;
    }

    // sets the ImageTargetType (only in editor mode)
    bool IEditorImageTargetBehaviour.SetImageTargetType(ImageTargetType imageTargetType)
    {
        if (mTrackable == null)
        {
            mImageTargetType = imageTargetType;
            return true;
        }
        return false;
    }

    void IEditorImageTargetBehaviour.InitializeImageTarget(ImageTarget imageTarget)
    {
        mTrackable = mImageTarget = imageTarget;
        mVirtualButtonBehaviours = new Dictionary<int, VirtualButtonBehaviour>();

        // do not change the aspect ratio of user defined targets, these are set by the algorithm internally
        if (imageTarget.ImageTargetType == ImageTargetType.PREDEFINED)
        {
            // Handle any changes to the image target in the scene
            // that are not reflected in the config file
            Vector2 imgTargetUnitySize = GetSize();

            imageTarget.SetSize(imgTargetUnitySize);
        }
        else // instead, set the aspect of the unity object to the value of the user defined target
        {
            Vector2 udtSize = imageTarget.GetSize();

            // set the size of the target to the value returned from cloud reco:
            transform.localScale =
                new Vector3(udtSize.x,
                            udtSize.x,
                            udtSize.x);

            IEditorImageTargetBehaviour editorThis = this;
            editorThis.CorrectScale();

            editorThis.SetAspectRatio(udtSize.y / udtSize.x);
        }
    }


    /// <summary>
    /// Associates existing virtual button behaviour with virtualbuttons and creates new VirtualButtons if necessary
    /// </summary>
    void IEditorImageTargetBehaviour.AssociateExistingVirtualButtonBehaviour(VirtualButtonBehaviour virtualButtonBehaviour)
    {
        VirtualButton virtualButton = mImageTarget.GetVirtualButtonByName(virtualButtonBehaviour.VirtualButtonName);

        if (virtualButton == null)
        {
            Vector2 leftTop, rightBottom;
            virtualButtonBehaviour.CalculateButtonArea(out leftTop, out rightBottom);
            VirtualButton.RectangleData area = new VirtualButton.RectangleData
                                                {
                                                    leftTopX = leftTop.x,
                                                    leftTopY = leftTop.y,
                                                    rightBottomX = rightBottom.x,
                                                    rightBottomY = rightBottom.y
                                                };
            virtualButton = mImageTarget.CreateVirtualButton(virtualButtonBehaviour.VirtualButtonName, area);

            // Create the virtual button
            if (virtualButton != null)
            {
                Debug.Log("Successfully created virtual button " +
                          virtualButtonBehaviour.VirtualButtonName +
                          " at startup");

                virtualButtonBehaviour.UnregisterOnDestroy = true;
            }
            else
            {
                Debug.LogError("Failed to create virtual button " +
                               virtualButtonBehaviour.VirtualButtonName +
                               " at startup");
            }
        }

        if (virtualButton != null)
        {
            //  Duplicate check:
            if (!mVirtualButtonBehaviours.ContainsKey(virtualButton.ID))
            {
                // OK:
                IEditorVirtualButtonBehaviour editorVirtualButtonBehaviour = virtualButtonBehaviour;
                editorVirtualButtonBehaviour.InitializeVirtualButton(virtualButton);
                mVirtualButtonBehaviours.Add(virtualButton.ID, virtualButtonBehaviour);

                Debug.Log("Found VirtualButton named " +
                        virtualButtonBehaviour.VirtualButton.Name + " with id " +
                        virtualButtonBehaviour.VirtualButton.ID);

                // Handle any changes to the virtual button in the scene
                // that are not reflected in the config file
                virtualButtonBehaviour.UpdatePose();
                if (!virtualButtonBehaviour.UpdateAreaRectangle() ||
                    !virtualButtonBehaviour.UpdateSensitivity())
                {
                    Debug.LogError("Failed to update virtual button " +
                                   virtualButtonBehaviour.VirtualButton.Name +
                                   " at startup");
                }
                else
                {
                    Debug.Log("Updated virtual button " +
                              virtualButtonBehaviour.VirtualButton.Name +
                              " at startup");
                }
            }
        }
    }


    void IEditorImageTargetBehaviour.CreateMissingVirtualButtonBehaviours()
    {
        foreach(VirtualButton virtualButton in mImageTarget.GetVirtualButtons())
            CreateVirtualButtonFromNative(virtualButton);
    }

    bool IEditorImageTargetBehaviour.TryGetVirtualButtonBehaviourByID(int id, out VirtualButtonBehaviour virtualButtonBehaviour)
    {
        return mVirtualButtonBehaviours.TryGetValue(id, out virtualButtonBehaviour);
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION


    #region PRIVATE_METHODS
    
    // creates the specified VirtualButtonBehaviour for this ImageTarget
    private void CreateVirtualButtonFromNative(VirtualButton virtualButton)
    {
        GameObject virtualButtonObject = new GameObject(virtualButton.Name);
        VirtualButtonBehaviour newVBB =
            virtualButtonObject.AddComponent<VirtualButtonBehaviour>();

        // We need to set the Image Target as a parent BEFORE we set the size
        // of the Virtual Button.
        newVBB.transform.parent = transform;

        IEditorVirtualButtonBehaviour newEditorVBB = newVBB;

        Debug.Log("Creating Virtual Button with values: " +
                  "\n ID:           " + virtualButton.ID +
                  "\n Name:         " + virtualButton.Name +
                  "\n Rectangle:    " + virtualButton.Area.leftTopX + "," +
                                        virtualButton.Area.leftTopY + "," +
                                        virtualButton.Area.rightBottomX + "," +
                                        virtualButton.Area.rightBottomY);

        newEditorVBB.SetVirtualButtonName(virtualButton.Name);
        newEditorVBB.SetPosAndScaleFromButtonArea(new Vector2(virtualButton.Area.leftTopX, virtualButton.Area.leftTopY),
                                                  new Vector2(virtualButton.Area.rightBottomX, virtualButton.Area.rightBottomY));
        // This button is part of a data set and should therefore not be
        // unregistered in native only because the Unity object is destroyed.
        newEditorVBB.UnregisterOnDestroy = false;
        newEditorVBB.InitializeVirtualButton(virtualButton);
        mVirtualButtonBehaviours.Add(virtualButton.ID, newVBB);
    }

    private bool CreateNewVirtualButtonFromBehaviour(VirtualButtonBehaviour newVBB)
    {
        // Calculate the button area:
        Vector2 leftTop, rightBottom;
        newVBB.CalculateButtonArea(out leftTop, out rightBottom);
        VirtualButton.RectangleData area = new VirtualButton.RectangleData
        {
            leftTopX = leftTop.x,
            leftTopY = leftTop.y,
            rightBottomX = rightBottom.x,
            rightBottomY = rightBottom.y
        };

        VirtualButton virtualButton = mImageTarget.CreateVirtualButton(newVBB.VirtualButtonName, area);

        if (virtualButton == null)
        {
            Destroy(newVBB.gameObject);
            return false;
        }

        IEditorVirtualButtonBehaviour newEditorVBB = newVBB;
        newEditorVBB.InitializeVirtualButton(virtualButton);
        mVirtualButtonBehaviours.Add(virtualButton.ID, newVBB);
        return true;
    }

    #endregion // PRIVATE_METHODS
}
