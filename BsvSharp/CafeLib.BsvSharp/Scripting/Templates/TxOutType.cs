using System;
using System.Collections.Generic;
using System.Linq;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    //TODO : Is*Conform can be used to parses the script

    public enum TxOutType
    {
        TX_NONSTANDARD,
        // 'standard' transaction types:
        TX_PUBKEY,
        TX_PUBKEYHASH,
        TX_SCRIPTHASH,
        TX_MULTISIG,
        TX_NULL_DATA,
    };
}
