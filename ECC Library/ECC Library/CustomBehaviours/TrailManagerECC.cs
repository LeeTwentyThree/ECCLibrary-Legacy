﻿namespace ECCLibrary.Internal;

/// <summary>
/// Class used for ECC TrailManagers that inherits from the standard TrailManager class, for the sake of distinction.
/// </summary>
public class TrailManagerECC : TrailManager
{
    /// <summary>
    /// If set to true, the TrailManager code is overriden by ECCLibrary into something that works better.
    /// </summary>
    public bool usesNewECCTrailBehaviour = true;
}
