/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;

public class TrackableSourceImpl : TrackableSource
{
    #region PROPERTIES

    public IntPtr TrackableSourcePtr 
    { get; private set; }

    #endregion



    #region CONSTRUCTION

    public TrackableSourceImpl(IntPtr trackableSourcePtr)
    {
        TrackableSourcePtr = trackableSourcePtr;
    }

    #endregion // CONSTRUCTION
}