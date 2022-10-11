using System;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    [Flags]
    public enum BloomFlags : byte
    {
        UPDATE_NONE = 0,
        UPDATE_ALL = 1,
        // Only adds outpoints to the filter if the output is a pay-to-pubkey/pay-to-multisig script
        UPDATE_P2PUBKEY_ONLY = 2,
        UPDATE_MASK = 3,
    };
}
