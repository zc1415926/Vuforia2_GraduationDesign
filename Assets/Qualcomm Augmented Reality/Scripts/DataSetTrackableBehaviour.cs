/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

/// <summary>
/// This is the base class for all trackables that are part of a dataset
/// </summary>
public abstract class DataSetTrackableBehaviour : TrackableBehaviour, IEditorDataSetTrackableBehaviour
{
    #region PROTECTED_MEMBER_VARIABLES

    [SerializeField]
    [HideInInspector]
    protected string mDataSetPath = "";

    #endregion // PROTECTED_MEMBER_VARIABLES



    #region EDITOR_INTERFACE_IMPLEMENTATION

    /// <summary>
    /// The name of the data set the Trackable belongs to.
    /// Please be aware that the data set name is not a unique identifier at runtime!
    /// </summary>
    string IEditorDataSetTrackableBehaviour.DataSetName
    {
        get
        {
            // Create the data set name from path.
            string nameWithExtension = QCARRuntimeUtilities.StripFileNameFromPath(mDataSetPath);

            string extension = QCARRuntimeUtilities.StripExtensionFromPath(mDataSetPath);

            int extensionLength = extension.Length;

            if (extensionLength > 0)
            {
                // Add "dot" if the file had an extension.
                ++extensionLength;

                return nameWithExtension.Remove(nameWithExtension.Length - extensionLength);
            }

            return nameWithExtension;
        }
    }

    /// <summary>
    /// The path to the data set in the file system.
    /// Please be aware that the data set name is not a unique identifier at runtime!
    /// </summary>
    string IEditorDataSetTrackableBehaviour.DataSetPath
    {
        get
        {
            return mDataSetPath;
        }
    }

    /// <summary>
    /// sets the DataSetPath (only in editor mode)
    /// </summary>
    bool IEditorDataSetTrackableBehaviour.SetDataSetPath(string dataSetPath)
    {
        if (mTrackable == null)
        {
            mDataSetPath = dataSetPath;
            return true;
        }
        return false;
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION
}
