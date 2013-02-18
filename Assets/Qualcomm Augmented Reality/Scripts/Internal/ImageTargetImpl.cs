/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ImageTargetImpl : TrackableImpl, ImageTarget
{
    #region PRIVATE_MEMBER_VARIABLES

    private Vector2 mSize;
    private readonly DataSetImpl mDataSet;
    private readonly ImageTargetType mImageTargetType;
    private readonly Dictionary<int, VirtualButton> mVirtualButtons;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region CONSTRUCTION

    public ImageTargetImpl(string name, int id, ImageTargetType imageTargetType, DataSet dataSet)
        : base(name, id)
    {
        Type = TrackableType.IMAGE_TARGET;
        mImageTargetType = imageTargetType;
        mDataSet = (DataSetImpl)dataSet;

        // read size from native:
        IntPtr sizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Vector2)));
        QCARWrapper.Instance.ImageTargetGetSize(mDataSet.DataSetPtr, Name, sizePtr);
        mSize = (Vector2)Marshal.PtrToStructure(sizePtr, typeof(Vector2));
        Marshal.FreeHGlobal(sizePtr);

        mVirtualButtons = new Dictionary<int, VirtualButton>();
        CreateVirtualButtonsFromNative();
    }

    #endregion // CONSTRUCTION



    #region PROPERTIES

    /// <summary>
    /// The type of this ImageTarget (Predefined, User Defined, Cloud Reco)
    /// </summary>
    public ImageTargetType ImageTargetType
    {
        get { return mImageTargetType; }
    }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns the size (width and height) of the target (in 3D scene units).
    /// </summary>
    public Vector2 GetSize()
    {
        return mSize;
    }


    /// <summary>
    /// Sets the size (width and height) of the target (in 3D scene units).
    /// This is only allowed when the dataset is not active!
    /// </summary>
    public void SetSize(Vector2 size)
    {
        mSize = size;

        // set size in native:
        IntPtr sizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Vector2)));
        Marshal.StructureToPtr(size, sizePtr, false);
        // It is safe to assume that at loading stage the data set is not active.
        QCARWrapper.Instance.ImageTargetSetSize(mDataSet.DataSetPtr, Name, sizePtr);
        Marshal.FreeHGlobal(sizePtr);
    }


    /// <summary>
    /// Creates a new virtual button and adds it to the ImageTarget
    /// Returns NULL if the corresponding DataSet is currently active.
    /// </summary>
    public VirtualButton CreateVirtualButton(string name, VirtualButton.RectangleData area)
    {
        VirtualButton virtualButton = CreateNewVirtualButtonInNative(name, area);

        if (virtualButton == null)
        {
            Debug.LogError("Could not create Virtual Button.");
        }
        else
        {
            Debug.Log("Created Virtual Button successfully.");
        }

        return virtualButton;
    }


    /// <summary>
    /// Returns a virtual button by its name
    /// Returns NULL if no virtual button with that name
    /// exists in this ImageTarget
    /// </summary>
    public VirtualButton GetVirtualButtonByName(string name)
    {
        foreach (VirtualButton virtualButton in mVirtualButtons.Values)
        {
            if (virtualButton.Name == name)
                return virtualButton;
        }

        return null;
    }
    
    /// <summary>
    ///  Returns the virtual buttons that are defined for this imageTarget
    /// </summary>
    public IEnumerable<VirtualButton> GetVirtualButtons()
    {
        return mVirtualButtons.Values;
    }


    /// <summary>
    /// Removes and destroys one of the ImageTarget's virtual buttons
    /// Returns false if the corresponding DataSet is currently active.
    /// </summary>
    public bool DestroyVirtualButton(VirtualButton vb)
    {
        bool success = false;

        ImageTracker imageTracker = (ImageTracker)
                                    TrackerManager.Instance.GetTracker(Tracker.Type.IMAGE_TRACKER);
        if (imageTracker != null)
        {
            bool isActiveDataSet = false;

            foreach(DataSet ads in imageTracker.GetActiveDataSets())
                if (mDataSet == ads)
                    isActiveDataSet = true;

            if (isActiveDataSet)
            {
                imageTracker.DeactivateDataSet(mDataSet);
            }
            if (UnregisterVirtualButtonInNative(vb))
            {
                Debug.Log("Unregistering virtual button successfully");
                success = true;
                mVirtualButtons.Remove(vb.ID);
            }
            else
            {
                Debug.LogError("Failed to unregister virtual button.");
            }
            if (isActiveDataSet)
            {
                imageTracker.ActivateDataSet(mDataSet);
            }
        }

        return success;
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    // Registers a Virtual Button at native code.
    private VirtualButton CreateNewVirtualButtonInNative(string name, VirtualButton.RectangleData rectangleData)
    {
        // virtual buttons cannot be registered for user defined targets:
        if (ImageTargetType != ImageTargetType.PREDEFINED)
        {
            Debug.LogError("DataSet.RegisterVirtualButton: virtual button '" + name +
                           "' cannot be registered for a user defined target.");
            return null;
        }


        IntPtr rectPtr = Marshal.AllocHGlobal(
            Marshal.SizeOf(typeof(VirtualButton.RectangleData)));
        Marshal.StructureToPtr(rectangleData, rectPtr, false);

        bool registerWorked =
            (QCARWrapper.Instance.ImageTargetCreateVirtualButton(mDataSet.DataSetPtr, Name,
                                            name, rectPtr) != 0);

        VirtualButton vb = null;

        if (registerWorked)
        {
            int id = QCARWrapper.Instance.VirtualButtonGetId(mDataSet.DataSetPtr, Name,
                                        name);

            // Check we don't have an entry for this id:
            if (!mVirtualButtons.ContainsKey(id))
            {
                // Add:
                vb = new VirtualButtonImpl(name, id, rectangleData, this, mDataSet);
                mVirtualButtons.Add(id, vb);
            }
            else
            {
                vb = mVirtualButtons[id];
            }
        }

        return vb;
    }


    // Unregister a Virtual Button at native code. 
    private bool UnregisterVirtualButtonInNative(VirtualButton vb)
    {
        int id = QCARWrapper.Instance.VirtualButtonGetId(mDataSet.DataSetPtr, Name, vb.Name);

        bool unregistered = false;

        if (QCARWrapper.Instance.ImageTargetDestroyVirtualButton(mDataSet.DataSetPtr, Name, vb.Name) != 0)
        {
            if (mVirtualButtons.Remove(id))
            {
                unregistered = true;
            }
        }

        if (!unregistered)
        {
            Debug.LogError("UnregisterVirtualButton: Failed to destroy " +
                            "the Virtual Button.");
        }

        return unregistered;
    }


    /// <summary>
    /// Creates virtual buttons for this ImageTarget from native and stores them
    /// </summary>
    private void CreateVirtualButtonsFromNative()
    {
        // Allocate array for all Image Targets.
        int numVirtualButtons = QCARWrapper.Instance.ImageTargetGetNumVirtualButtons(mDataSet.DataSetPtr, Name);
        IntPtr virtualButtonDataPtr =
            Marshal.AllocHGlobal(Marshal.SizeOf(typeof(QCARManagerImpl.VirtualButtonData)) * numVirtualButtons);
        IntPtr rectangleDataPtr =
            Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VirtualButton.RectangleData)) * numVirtualButtons);

        // Copy Virtual Button data from native.
        QCARWrapper.Instance.ImageTargetGetVirtualButtons(virtualButtonDataPtr,
                                     rectangleDataPtr,
                                     numVirtualButtons,
                                     mDataSet.DataSetPtr,
                                     Name);

        for (int i = 0; i < numVirtualButtons; ++i)
        {
            IntPtr vbPtr = new IntPtr(virtualButtonDataPtr.ToInt32() + i *
                    Marshal.SizeOf(typeof(QCARManagerImpl.VirtualButtonData)));
            QCARManagerImpl.VirtualButtonData vbData = (QCARManagerImpl.VirtualButtonData)
                    Marshal.PtrToStructure(vbPtr, typeof(QCARManagerImpl.VirtualButtonData));

            // Do not overwrite existing Virtual Buttons.
            if (mVirtualButtons.ContainsKey(vbData.id))
            {
                continue;
            }

            IntPtr rectPtr = new IntPtr(rectangleDataPtr.ToInt32() + i *
                    Marshal.SizeOf(typeof(VirtualButton.RectangleData)));
            VirtualButton.RectangleData rectData = (VirtualButton.RectangleData)
                    Marshal.PtrToStructure(rectPtr, typeof(VirtualButton.RectangleData));

            // QCAR support names up to 64 characters in length, but here we allocate 
            // a slightly larger buffer:
            int nameLength = 128;
            System.Text.StringBuilder vbName = new System.Text.StringBuilder(nameLength);
            if (QCARWrapper.Instance.ImageTargetGetVirtualButtonName(mDataSet.DataSetPtr, Name,
                    i, vbName, nameLength) == 0)
            {
                Debug.LogError("Failed to get virtual button name.");
                continue;
            }

            VirtualButton virtualButton = new VirtualButtonImpl(vbName.ToString(), vbData.id, rectData, this, mDataSet);
            mVirtualButtons.Add(vbData.id, virtualButton);
        }

        Marshal.FreeHGlobal(virtualButtonDataPtr);
        Marshal.FreeHGlobal(rectangleDataPtr);
    }

    #endregion // PRIVATE_METHODS
}