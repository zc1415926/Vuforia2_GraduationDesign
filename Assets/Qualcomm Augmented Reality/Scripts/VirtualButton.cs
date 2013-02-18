/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// A virtual button on a trackable
///
/// Methods to modify a VirtualButton must not be called while the
/// corresponding DataSet is active. The dataset must be deactivated first
/// before reconfiguring a VirtualButton.
/// </summary>
public abstract class VirtualButton
{
    #region NESTED
    
    /// <summary>
    /// The sensitivity of this virtual button. This is a trade off between fast
    /// detection and robustness again accidental occlusion.
    /// </summary>
    public enum Sensitivity
    {
        HIGH,           // Fast detection.
        MEDIUM,         // Balanced between fast and robust.
        LOW             // Robust detection.
    }

    /// <summary>
    /// This struct defines the 2D coordinates of a rectangle. It is used for
    /// rectangular Virtual Button definitions.
    /// The struct is used internally by the VirtualButtonBehaviour.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RectangleData
    {
        public float leftTopX;
        public float leftTopY;
        public float rightBottomX;
        public float rightBottomY;
    }

    #endregion // NESTED



    #region CONSTANTS
    /// <summary>
    /// The standard sensitivity
    /// </summary>
    public const VirtualButton.Sensitivity DEFAULT_SENSITIVITY = VirtualButton.Sensitivity.LOW;

    #endregion // CONSTANTS



    #region PROPERTIES

    /// <summary>
    /// Returns the name of the button
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Returns a unique id for this virtual button.
    /// </summary>
    public abstract int ID { get; }

    /// <summary>
    /// Returns true if the virtual button is active (updates while tracking).
    /// </summary>
    public abstract bool Enabled { get; }

    /// <summary>
    /// Returns the currently set Area
    /// </summary>
    public abstract RectangleData Area { get; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Defines a new area for the button area in 3D scene units (the
    /// coordinate system is local to the ImageTarget).
    ///
    /// This method must not be called while the corresponding DataSet is
    /// active or it will return false.
    /// </summary>
    public abstract bool SetArea(RectangleData area);
    
    /// <summary>
    /// Sets the sensitivity of the virtual button
    ///
    /// Sensitivity allows deciding between fast and robust button press
    /// measurements. This method must not be called while the corresponding
    /// DataSet is active or it will return false.
    /// </summary>
    public abstract bool SetSensitivity(Sensitivity sensitivity);
    
    /// <summary>
    /// Enables or disables a virtual button
    ///
    /// This method must not be called while the corresponding DataSet is
    /// active or it will return false.
    /// </summary>
    public abstract bool SetEnabled(bool enabled);

    #endregion // PUBLIC METHODS
}

