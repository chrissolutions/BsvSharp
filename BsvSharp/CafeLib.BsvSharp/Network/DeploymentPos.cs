namespace CafeLib.BsvSharp.Network
{
    public enum DeploymentPos
    {
        DeploymentTestDummy,
        // Deployment of BIP68, BIP112, and BIP113.
        DeploymentCsv,
        // 
        // 
        /// <summary>
        /// NOTE: Also add new deployments to VersionBitsDeploymentInfo in versionbits.cpp
        /// </summary>
        MaxVersionBitsDeployments
    };
}