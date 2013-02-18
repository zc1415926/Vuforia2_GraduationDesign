/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


public abstract class TrackableAccessor
{
    #region PROTECTED_MEMBER_VARIABLES

    // Every accessor instance has only one dedicated TrackableBehaviour
    // instance that it is assigned to. It is referenced by this variable.
    protected TrackableBehaviour mTarget = null;

    #endregion // PROTECTED_MEMBER_VARIABLES



    #region PUBLIC_METHODS

    // This method updates the respective Trackable properties (e.g. size)
    // with data set data.
    public abstract void ApplyDataSetProperties();

    // This method updates the respective Trackable appearance (e.g.
    // aspect ratio and texture) with data set data.
    public abstract void ApplyDataSetAppearance();

    #endregion // PUBLIC_METHODS
}
