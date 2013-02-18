/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// An image - Used to expose the camera frame.
/// 
/// The image's pixel buffer can have a different size than the
/// Wdith and Height properties report. This is e.g. the
/// case when an image is used for rendering as a texture without
/// non-power-of-two support.
/// The real size of the image's pixel buffer can be queried using
/// BufferWidth and BufferHeight. 
/// </summary>
public abstract class Image
{
    #region NESTED

    /// <summary>
    /// The pixel format of an image
    /// </summary>
    public enum PIXEL_FORMAT
    {
    UNKNOWN_FORMAT = 0,         // Unknown format - default pixel type for
                                // undefined images
    RGB565 = 1,                 // A color pixel stored in 2 bytes using 5
                                // bits for red, 6 bits for green and 5 bits
                                // for blue
    RGB888 = 2,                 // A color pixel stored in 3 bytes using
                                // 8 bits each
    GRAYSCALE = 4,              // A grayscale pixel stored in one byte
    YUV = 8,                    // A color pixel stored in 12 or more bits
                                // using Y, U and V planes
    RGBA8888 = 16,              // A color pixel stored in 4 bytes using
                                // 8 bits each
    };

    #endregion // NESTED



    #region PROPERTIES

    /// <summary>
    /// The width of the image in pixels. Note the pixel buffer can be
    /// wider than this.
    /// </summary>
    public abstract int Width { get; set; }

    /// <summary>
    /// The height of the image in pixels. Note that the pixel buffer
    /// can be higher than this.
    /// </summary>
    public abstract int Height { get; set; }

    /// <summary>
    /// The number bytes from one row of pixels to the next row.
    /// </summary>
    public abstract int Stride { get; set; }

    /// <summary>
    /// The number of pixel columns that fit into the pixel buffer.
    /// </summary>
    public abstract int BufferWidth { get; set; }

    /// <summary>
    /// The number of rows that fit into the pixel buffer.
    /// </summary>
    public abstract int BufferHeight { get; set; }

    /// <summary>
    /// The pixel format of the image.
    /// </summary>
    public abstract PIXEL_FORMAT PixelFormat { get; set; }

    /// <summary>
    /// The pixel data.
    /// </summary>
    public abstract byte[] Pixels { get; set; }

    #endregion // PROPERTIES



    #region PUBLIC_METHODS

    /// <summary>
    /// Returns true if this image has been fully initiailzed with valid data.
    // False otherwise.
    /// </summary>
    public abstract bool IsValid();

    #endregion // PUBLIC_METHODS
}
