/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

/// <summary>
/// An interface for handling cloud reco events.
/// </summary>
public interface ICloudRecoEventHandler
{
    /// <summary>
    /// called when the CloudRecoBehaviour has finished initializing
    /// </summary>
    void OnInitialized();

    /// <summary>
    /// called when an error is reported during initialization
    /// </summary>
    void OnInitError(TargetFinder.InitState initError);

    /// <summary>
    /// called when an error is reported while updating
    /// </summary>
    void OnUpdateError(TargetFinder.UpdateState updateError);

    /// <summary>
    /// called when the CloudRecoBehaviour starts or stops scanning
    /// </summary>
    void OnStateChanged(bool scanning);

    /// <summary>
    /// called when a new search result is found
    /// </summary>
    void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult);
}
