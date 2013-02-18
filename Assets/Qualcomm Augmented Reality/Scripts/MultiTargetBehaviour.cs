/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// This class serves both as an augmentation definition for a MultiTarget in the editor
/// as well as a tracked MultiTarget result at runtime
/// </summary>
public class MultiTargetBehaviour : DataSetTrackableBehaviour, IEditorMultiTargetBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES

    private MultiTarget mMultiTarget;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region PROPERTIES

    /// <summary>
    /// The MultiTarget that this MultiTargetBehaviour augments
    /// </summary>
    public MultiTarget MultiTarget
    {
        get { return mMultiTarget; }
    }

    #endregion // PROPERTIES



    #region PROTECTED_METHODS

    /// <summary>
    /// This method disconnects the TrackableBehaviour from it's associated trackable.
    /// Use it only if you know what you are doing - e.g. when you want to destroy a trackable, but reuse the TrackableBehaviour.
    /// </summary>
    protected override void InternalUnregisterTrackable()
    {
        mTrackable = mMultiTarget = null;
    }

    #endregion // PROTECTED_METHODS



    #region EDITOR_INTERFACE_IMPLEMENTATION

    void IEditorMultiTargetBehaviour.InitializeMultiTarget(MultiTarget multiTarget)
    {
        mTrackable = mMultiTarget = multiTarget;
    }

    #endregion // EDITOR_INTERFACE_IMPLEMENTATION
}
