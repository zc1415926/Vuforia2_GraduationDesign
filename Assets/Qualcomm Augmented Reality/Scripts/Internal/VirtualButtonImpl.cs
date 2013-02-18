/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class VirtualButtonImpl : VirtualButton
{
    #region PRIVATE_MEMBER_VARIABLES

    private string mName;
    private int mID;
    private RectangleData mArea;
    private bool mIsEnabled;

    private ImageTarget mParentImageTarget;
    private DataSetImpl mParentDataSet;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    public override string  Name
    {
        get { return mName; }
    }

    public override int ID
    {
        get { return mID; }
    }

    public override bool Enabled 
    {
        get { return mIsEnabled; } 
    }

    public override VirtualButton.RectangleData Area
    {
        get { return mArea; }
    }

    #endregion // PROPERTIES



    #region CONSTRUCTION

    public VirtualButtonImpl(string name, int id, RectangleData area,
                             ImageTarget imageTarget, DataSet dataSet)
    {
        mName = name;
        mID = id;
        mArea = area;
        mIsEnabled = true;
        mParentImageTarget = imageTarget;
        mParentDataSet = (DataSetImpl)dataSet;
    }

    #endregion // CONSTRUCTION



    #region PUBLIC_METHODS

    public override bool SetArea(RectangleData area)
    {

        IntPtr rectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(
                                typeof(VirtualButton.RectangleData)));
        Marshal.StructureToPtr(area, rectPtr, false);
        int success = QCARWrapper.Instance.VirtualButtonSetAreaRectangle(mParentDataSet.DataSetPtr,
            mParentImageTarget.Name, this.Name, rectPtr);
        Marshal.FreeHGlobal(rectPtr);

        if (success == 0)
        {
            Debug.LogError("Virtual Button area rectangle could not be set.");
            return false;
        }

        return true;
    }

    public override bool SetSensitivity(VirtualButton.Sensitivity sensitivity)
    {
        int success = QCARWrapper.Instance.VirtualButtonSetSensitivity(mParentDataSet.DataSetPtr,
                                                mParentImageTarget.Name,
                                                mName,
                                                (int)sensitivity);

        if (success == 0)
        {
            Debug.LogError("Virtual Button sensitivity could not be set.");
            return false;
        }
        return true;
    }

    public override bool SetEnabled(bool enabled)
    {
        int success = QCARWrapper.Instance.VirtualButtonSetEnabled(mParentDataSet.DataSetPtr,
                                                mParentImageTarget.Name,
                                                mName, enabled ? 1 : 0);

        if (success == 0)
        {
            Debug.LogError("Virtual Button enabled value could not be set.");
            return false;
        }

        mIsEnabled = enabled;
        return true;
    }

    #endregion // PUBLIC_METHODS
}

