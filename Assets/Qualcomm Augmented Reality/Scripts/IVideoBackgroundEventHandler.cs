/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// Interface for handling events regarding the video background
/// </summary>
public interface IVideoBackgroundEventHandler
{
    /// <summary>
    /// Called after the video background config has been changed
    /// </summary>
    void OnVideoBackgroundConfigChanged();
}
