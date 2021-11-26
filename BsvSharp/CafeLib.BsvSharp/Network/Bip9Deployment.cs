using System;

namespace CafeLib.BsvSharp.Network
{
    public struct Bip9Deployment
    {
        /** Bit position to select the particular bit in nVersion. */
        public int Bit;

        /** Start MedianTime for version bits miner confirmation. Can be a date in the past */
        public Int64 StartTime;

        /** Timeout/expiry MedianTime for the deployment attempt. */
        public Int64 Timeout;
    };
}