/*==============================================================================
Copyright (c) 2012 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/


public abstract class TrackableImpl : Trackable
{

    protected TrackableImpl(string name, int id)
    {
        Name = name;
        ID = id;
    }

    public TrackableType Type 
    { get; protected set; }

    public string Name
    { get; protected set; }

    public int ID
    { get; protected set; }
}