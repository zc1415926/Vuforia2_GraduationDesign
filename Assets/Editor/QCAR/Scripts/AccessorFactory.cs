/*==============================================================================
Copyright (c) 2010-2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEngine;

public class AccessorFactory
{
    #region PUBLIC_METHODS

    // Creates a new Accessor object of the appropriate type. The accessor takes
    // a TrackableBehaviour as a target (the Accessor instance accesses this
    // single object).
    public static TrackableAccessor Create(TrackableBehaviour target)
    {
        if (target is MarkerBehaviour)
        {
            return new MarkerAccessor((MarkerBehaviour)target);
        }
        else if (target is ImageTargetBehaviour)
        {
            return new ImageTargetAccessor((ImageTargetBehaviour)target);
        }
        else if (target is MultiTargetBehaviour)
        {
            return new MultiTargetAccessor((MultiTargetBehaviour)target);
        }
        else
        {
            Debug.LogWarning(target.GetType().ToString() +
                             " is not derived from TrackableBehaviour.");
            return null;
        }
    }

    #endregion // PUBLIC_METHODS
}
