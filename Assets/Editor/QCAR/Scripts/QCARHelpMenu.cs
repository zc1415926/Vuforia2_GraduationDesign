/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;

public class QCARHelpMenu : Editor
{
    #region PUBLIC_METHODS

    // Method opens up a browser Window with the specified URL.
    // This method is called when "Vuforia Documentation" is chosen from the
    // Unity "Help" menu.
    [MenuItem("Vuforia/Vuforia Documentation", false, 0)]
    public static void browseQCARHelp()
    {
        System.Diagnostics.Process.Start(
            "https://developer.vuforia.com/resources/dev-guide/getting-started");
    }


    // Method opens up a browser Window with the specified URL.
    // This method is called when "Release Notes" is chosen from the
    // Unity "Help" menu.
    [MenuItem("Vuforia/Release Notes", false, 1)]
    public static void browseQCARReleaseNotes()
    {
        System.Diagnostics.Process.Start(
            "https://developer.vuforia.com/resources/sdk/unity-extensions-vuforia-v20#sdkReleaseNotes");
    }

    #endregion PUBLIC_METHODS
}
