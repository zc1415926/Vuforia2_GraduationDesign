/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// An interface for handling virtual button state changes.
/// </summary>
public interface IVirtualButtonEventHandler
{
    /// <summary>
    /// Called when the virtual button has just been pressed.
    /// </summary>
    void OnButtonPressed(VirtualButtonBehaviour vb);

    /// <summary>
    /// Called when the virtual button has just been released.
    /// </summary>
    void OnButtonReleased(VirtualButtonBehaviour vb);
}
