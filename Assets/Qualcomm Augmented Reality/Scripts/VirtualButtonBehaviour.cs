/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// This behaviour associates a Virtual Button with a game object. Use the
/// functionality in ImageTargetBehaviour to create and destroy Virtual Buttons
/// at run-time.
/// </summary>
public class VirtualButtonBehaviour : MonoBehaviour, IEditorVirtualButtonBehaviour
{

    #region PROPERTIES

    /// <summary>
    /// The names of this virtual button.
    /// </summary>
    public string VirtualButtonName
    {
        get { return mName; }
    }


    /// <summary>
    /// Returns true if this button is currently pressed.
    /// </summary>
    public bool Pressed
    {
        get { return mPressed; }
    }


    /// <summary>
    /// if the pose has been updated once
    /// </summary>
    public bool HasUpdatedPose 
    {
        get { return mHasUpdatedPose; }
    }


    /// <summary>
    /// Unregistering Virtual Buttons should only be done if they have been 
    /// registered at runtime. This property is automatically set by
    /// ImageTargetBehaviour on registration.
    /// </summary>
    public bool UnregisterOnDestroy
    {
        get
        {
            return mUnregisterOnDestroy;
        }

        set
        {
            mUnregisterOnDestroy = value;
        }
    }


    /// <summary>
    /// The VirtualButton Object created at runtime
    /// </summary>
    public VirtualButton VirtualButton
    {
        get { return mVirtualButton; }
    }

    #endregion // PROPERTIES



    #region CONSTANTS

    /// <summary>
    /// The vertical offset of the graphic representation of a virtual button in respect to the target
    /// </summary>
    public const float TARGET_OFFSET = 0.001f;

    #endregion // CONSTANTS



    #region PRIVATE_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    private string mName;

    [SerializeField]
    [HideInInspector]
    private VirtualButton.Sensitivity mSensitivity;

    [SerializeField]
    [HideInInspector]
    private bool mHasUpdatedPose = false;

    [SerializeField]
    [HideInInspector]
    private Matrix4x4 mPrevTransform = Matrix4x4.zero;

    [SerializeField]
    [HideInInspector]
    private GameObject mPrevParent = null;

    private bool mSensitivityDirty;
    private bool mPreviouslyEnabled;
    private bool mPressed;
    private List<IVirtualButtonEventHandler> mHandlers = null;
    private Vector2 mLeftTop;
    private Vector2 mRightBottom;
    private bool mUnregisterOnDestroy;

    VirtualButton mVirtualButton;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    public VirtualButtonBehaviour()
    {
        mName = "";
        mPressed = false;
        mSensitivity = VirtualButton.DEFAULT_SENSITIVITY;
        mSensitivityDirty = false;
        mHandlers = new List<IVirtualButtonEventHandler>();
        mHasUpdatedPose = false;
    }

    #endregion // CONSTRUCTION

    
    
    #region PUBLIC_METHODS

    /// <summary>
    /// Registers an event handler with this Virtual Button which will be called
    /// when a state changed is detected.
    /// </summary>
    public void RegisterEventHandler(IVirtualButtonEventHandler eventHandler)
    {
        mHandlers.Add(eventHandler);
    }


    /// <summary>
    /// Registers an event handler with this Virtual Button which will be called
    /// when a state changed is detected.
    /// Returns true on success. False otherwise.
    /// </summary>
    public bool UnregisterEventHandler(IVirtualButtonEventHandler eventHandler)
    {
        return mHandlers.Remove(eventHandler);
    }


    /// <summary>
    /// Calculates the 2D button area that the Virtual Button currently occupies
    /// in the Image Target.
    /// Returns true if the area was computed successfully. False otherwise.
    /// Passes out the top left and bottom right position of the rectangle area.
    /// </summary>
    public bool CalculateButtonArea(out Vector2 topLeft,
                                    out Vector2 bottomRight)
    {
        // Error if we don't have an image target as a root:
        ImageTargetBehaviour itb = this.GetImageTargetBehaviour();
        if (itb == null)
        {
            topLeft = bottomRight = Vector2.zero;
            return false;
        }

        Vector3 vbPosITSpace = itb.transform.InverseTransformPoint(
                                                this.transform.position);

        // The scale of the image Target:
        float itScale = itb.transform.lossyScale[0];

        // Scale the button position:
        Vector2 pos = new Vector2(vbPosITSpace[0] * itScale,
                                  vbPosITSpace[2] * itScale);

        // Scale the button area:
        Vector2 scale = new Vector2(this.transform.lossyScale[0],
                                    this.transform.lossyScale[2]);

        // Calculate top left and bottom right points:
        Vector2 radius = Vector2.Scale(scale * 0.5F, new Vector2(1.0f, -1.0f));

        topLeft = pos - radius;
        bottomRight = pos + radius;

        // Done:
        return true;
    }


    /// <summary>
    /// Update the virtual button rect in native
    /// </summary>
    public bool UpdateAreaRectangle()
    {
        VirtualButton.RectangleData rectData = new VirtualButton.RectangleData();

        rectData.leftTopX = mLeftTop.x;
        rectData.leftTopY = mLeftTop.y;
        rectData.rightBottomX = mRightBottom.x;
        rectData.rightBottomY = mRightBottom.y;

        if (mVirtualButton == null) return false;

        return mVirtualButton.SetArea(rectData);
    }


    /// <summary>
    /// Update sensitivity in native
    /// </summary>
    public bool UpdateSensitivity()
    {
        if (mVirtualButton == null) return false;

        return mVirtualButton.SetSensitivity(mSensitivity);
    }


    /// <summary>
    /// Update enabled status in native
    /// </summary>
    private bool UpdateEnabled()
    {
        return mVirtualButton.SetEnabled(enabled);
    }


    /// <summary>
    /// UpdatePose() is called each frame to ensure the virtual button is clamped
    /// to the image target plane and remains axis-aligned with respect to the
    /// target. Return true if the defining area of the virtual button has
    /// changed, false otherwise.
    /// </summary>
    public bool UpdatePose()
    {
        // The image target to which the button belongs:
        ImageTargetBehaviour itb = this.GetImageTargetBehaviour();

        // If there is no image target we return:
        if (itb == null)
        {
            return false;
        }

        // We explicitly disallow any objects with non-uniform scaling in the
        // object hierachy of the virtual button. Combined with a rotation
        // this would result in skewing the virtual button.
        Transform t = transform.parent;
        while (t != null)
        {
            if (t.localScale[0] != t.localScale[1] ||
                t.localScale[0] != t.localScale[2])
            {
                Debug.LogWarning("Detected non-uniform scale in virtual " +
                    " button object hierarchy. Forcing uniform scaling of " +
                    "object '" + t.name + "'.");

                //  Force uniform scale:
                t.localScale = new Vector3(t.localScale[0], t.localScale[0],
                                            t.localScale[0]);
            }
            t = t.parent;
        }

        // Remember we have updated once:
        mHasUpdatedPose = true;

        // Clamp to center of parent object:
        if (transform.parent != null &&
            transform.parent.gameObject != itb.gameObject)
        {
            transform.localPosition = Vector3.zero;
        }

        // Clamp position to image target plane:
        Vector3 vbPosITSpace = itb.transform.InverseTransformPoint(
                                                    this.transform.position);

        // Set the y offset in Image Target space:
        vbPosITSpace.y = TARGET_OFFSET;
        Vector3 vbPosWorldSpace = itb.transform.TransformPoint(vbPosITSpace);
        this.transform.position = vbPosWorldSpace;

        // Clamp orientation to the image target plane:
        this.transform.rotation = itb.transform.rotation;

        // Update the button area:
        Vector2 leftTop, rightBottom;
        CalculateButtonArea(out leftTop, out rightBottom);

        // Change the button area only if the change is larger than a fixed
        // proportion of the image target size:
        float threshold = itb.transform.localScale[0] * 0.001f;

        if (!Equals(leftTop, mLeftTop, threshold) ||
            !Equals(rightBottom, mRightBottom, threshold))
        {
            // Area has changed significantly:
            mLeftTop = leftTop;
            mRightBottom = rightBottom;
            return true;
        }

        // Area has not changed significantly:
        return false;
    }


    /// <summary>
    /// Called after the QCARBehaviour has updated.
    /// </summary>
    public void OnTrackerUpdated(bool pressed)
    {
        if (mPreviouslyEnabled != enabled)
        {
            mPreviouslyEnabled = enabled;
            UpdateEnabled();
        }

        if (!enabled)
        {
            return;
        }

        // Trigger the appropriate callback if there was state change:
        if (mPressed != pressed && mHandlers != null)
        {
            if (pressed)
            {
                foreach (IVirtualButtonEventHandler handler in mHandlers)
                {
                    handler.OnButtonPressed(this);
                }
            }
            else
            {
                foreach (IVirtualButtonEventHandler handler in mHandlers)
                {
                    handler.OnButtonReleased(this);
                }
            }
        }

        // Cache pressed state:
        mPressed = pressed;
    }


    /// <summary>
    /// Returns the Image Target that this Virtual Button is associated with.
    /// </summary>
    public ImageTargetBehaviour GetImageTargetBehaviour()
    {
        if (transform.parent == null)
            return null;

        GameObject p = transform.parent.gameObject;

        while (p != null)
        {
            ImageTargetBehaviour itb = p.GetComponent<ImageTargetBehaviour>();
            if (itb != null)
            {
                return itb;
            }

            if (p.transform.parent == null)
            {
                // Not found:
                return null;
            }

            p = p.transform.parent.gameObject;
        }

        // Not found:
        return null;
    }

    #endregion // PUBLIC_METHODS



    #region EDITOR_INTERFACE_IMPLEMENTATION

    // Initializes the Virtual Button name. Not allowed after runtime object has been created.
    bool IEditorVirtualButtonBehaviour.SetVirtualButtonName(string virtualButtonName)
    {
        if (mVirtualButton == null)
        {
            mName = virtualButtonName;
            return true;
        }

        return false;
    }


    VirtualButton.Sensitivity IEditorVirtualButtonBehaviour.SensitivitySetting 
    {
        get { return mSensitivity; }
    }

    // sets the sensitivity. At runtime the VirtualButton object should be used to change sensibility.
    bool IEditorVirtualButtonBehaviour.SetSensitivitySetting(VirtualButton.Sensitivity sensibility)
    {
        if (mVirtualButton == null)
        {
            mSensitivity = sensibility;
            mSensitivityDirty = true;
            return true;
        }

        return false;
    }

    Matrix4x4 IEditorVirtualButtonBehaviour.PreviousTransform
    {
        get { return mPrevTransform; } 
    }

    bool IEditorVirtualButtonBehaviour.SetPreviousTransform(Matrix4x4 transform)
    {
        if (mVirtualButton == null)
        {
            mPrevTransform = transform;
            return true;
        }

        return false;
    }

    GameObject IEditorVirtualButtonBehaviour.PreviousParent 
    {
        get { return mPrevParent; }
    }

    bool IEditorVirtualButtonBehaviour.SetPreviousParent(GameObject parent)
    {
        if (mVirtualButton == null)
        {
            mPrevParent = parent;
            return true;
        }

        return false;
    }

    // Initializes the Virtual Button runtime object
    void IEditorVirtualButtonBehaviour.InitializeVirtualButton(VirtualButton virtualButton)
    {
        mVirtualButton = virtualButton;
    }

    // Sets position and scale in the transform component of the Virtual Button
    // game object. The values are calculated from rectangle values (top-left
    // and bottom-right corners).
    // Returns false if Virtual Button is not child of an Image Target.
    bool IEditorVirtualButtonBehaviour.SetPosAndScaleFromButtonArea(Vector2 topLeft, Vector2 bottomRight)
    {
        // Error if we don't have an image target as a root:
        ImageTargetBehaviour itb = this.GetImageTargetBehaviour();
        if (itb == null)
        {
            return false;
        }

        float itScale = itb.transform.lossyScale[0];

        Vector2 pos = (topLeft + bottomRight) * 0.5f;

        Vector2 scale = new Vector2(bottomRight[0] - topLeft[0],
                                    topLeft[1] - bottomRight[1]);

        Vector3 vbPosITSpace =
            new Vector3(pos[0] / itScale, VirtualButtonBehaviour.TARGET_OFFSET,
                        pos[1] / itScale);


        Vector3 vbScaleITSpace =
            new Vector3(scale[0],
                        (scale[0] + scale[1]) * 0.5f,
                        scale[1]);

        this.transform.position = itb.transform.TransformPoint(vbPosITSpace);

        // Image Target scale is canceled out (included in both scales)
        this.transform.localScale =
            vbScaleITSpace / this.transform.parent.lossyScale[0];

        // Done:
        return true;
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION



    #region UNITY_MONOBEHAVIOUR_METHODS

    // Overriding standard Unity MonoBehaviour methods.

    void LateUpdate()
    {
        // Update the button pose:
        if (UpdatePose())
        {
            // Area has changed, update the QCAR trackable:
            UpdateAreaRectangle();
        }

        // Update the sensitivity of the button if it has changed since the
        // last update:
        if (mSensitivityDirty)
        {
            if (UpdateSensitivity())
            {
                mSensitivityDirty = false;
            }
        }
    }


    void OnDisable()
    {
        if (QCARRuntimeUtilities.IsQCAREnabled())
        {
            if (mPreviouslyEnabled != enabled)
            {
                mPreviouslyEnabled = enabled;
                UpdateEnabled();
            }

            // Trigger the appropriate callback if there was state change:
            if (mPressed && mHandlers != null)
            {
                foreach (IVirtualButtonEventHandler handler in mHandlers)
                {
                    handler.OnButtonReleased(this);
                }
            }

            // Cache pressed state:
            mPressed = false;
        }
    }


    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            if (mUnregisterOnDestroy)
            {
                ImageTargetBehaviour itb = GetImageTargetBehaviour();
                if (itb != null)
                    itb.ImageTarget.DestroyVirtualButton(mVirtualButton);
            }
        }
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS



    #region PRIVATE_METHODS
    
    private static bool Equals(Vector2 vec1, Vector2 vec2, float threshold)
    {
        Vector2 diff = vec1 - vec2;
        return (Math.Abs(diff.x) < threshold) && (Math.Abs(diff.y) < threshold);
    }

    #endregion // PRIVATE_METHODS
}
