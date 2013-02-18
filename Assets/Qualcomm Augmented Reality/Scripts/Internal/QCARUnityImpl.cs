/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

public static class QCARUnityImpl
{
    #region PUBLIC_METHODS

    // Retrieves initialization error code or success
    public static QCARUnity.InitError CheckInitializationError()
    {
        return (QCARUnity.InitError)QCARWrapper.Instance.GetInitErrorCode();
    }


    // Checks if the GL surface has changed
    public static bool IsRendererDirty()
    {
        CameraDeviceImpl cameraDeviceImpl = (CameraDeviceImpl)CameraDevice.Instance;
        if (QCARRuntimeUtilities.IsPlayMode())
        {
            return cameraDeviceImpl.IsDirty();
        }
        else
        {
            // check native renderer
            return QCARWrapper.Instance.IsRendererDirty() == 1 || cameraDeviceImpl.IsDirty();
        }
    }


    // Sets a hint for the QCAR SDK
    // Hints help the SDK to understand the developer's needs.
    // However, depending on the device or SDK version the hints
    // might not be taken into consideration.
    // Returns false if the hint is unknown or deprecated.
    public static bool SetHint(QCARUnity.QCARHint hint, int value)
    {
        Debug.Log("SetHint");
        return QCARWrapper.Instance.QcarSetHint((int)hint, value) == 1;
    }


    // Indicates whether the rendering surface needs to support an alpha channel
    // for transparency
    public static bool RequiresAlpha()
    {
        return QCARWrapper.Instance.QcarRequiresAlpha() == 1;
    }


    // Returns the QCAR projection matrix
    public static Matrix4x4 GetProjectionGL(float nearPlane, float farPlane, ScreenOrientation screenOrientation)
    {
        float[] projMatrixArray = new float[16];
        IntPtr projMatrixPtr = Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(float)) * projMatrixArray.Length);

        QCARWrapper.Instance.GetProjectionGL(nearPlane, farPlane, projMatrixPtr,
                    (int)screenOrientation);

        Marshal.Copy(projMatrixPtr, projMatrixArray, 0, projMatrixArray.Length);
        Matrix4x4 projMatrix = Matrix4x4.identity;
        for (int i = 0; i < 16; i++)
            projMatrix[i] = projMatrixArray[i];

        Marshal.FreeHGlobal(projMatrixPtr);

        return projMatrix;
    }


    // Sets the Unity version for internal use
    public static void SetUnityVersion(string path, bool setNative=false)
    {
        int major  = 0;
        int minor  = 0;
        int change = 0;

        // Use non-numeric values as tokens for split
        string versionPattern = "[^0-9]";

        // Split Unity version string into multiple parts
        string[] unityVersionBits = Regex.Split(Application.unityVersion,
                                                versionPattern);

        // Sanity check if nothing went wrong
        if (unityVersionBits.Length >= 3)
        {
            major  = int.Parse(unityVersionBits[0]);
            minor  = int.Parse(unityVersionBits[1]);
            change = int.Parse(unityVersionBits[2]);
        }

        // store the used Unity version in a file:
        try
        {
            File.WriteAllText(Path.Combine(path, "unity.txt"), string.Format("{0}.{1}.{2}", major, minor, change));
        }
        catch (Exception e)
        {
            Debug.LogError("Writing Unity version to file failed: " + e.Message);
        }

        if (setNative) QCARWrapper.Instance.SetUnityVersion(major, minor, change);
    }

    #endregion // PUBLIC_METHODS
}
