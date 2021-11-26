#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Scripting 
{
    public enum TemplateId
    {
        /// <summary>
        /// An unspendable OP_RETURN of unknown protocol
        /// </summary>
        OpRet = -1,
        /// <summary>
        /// Script is not one of the enumerated script templates.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// [pubkey in long or short format] OP_CHECKSIG
        /// </summary>
        Pay2PublicKey = 1,
        /// <summary>
        /// OP_DUP OP_HASH160 [20 byte hashed pubkey] OP_EQUALVERIFY OP_CHECKSIG
        /// PubKey is moved to ScriptSig.
        /// Hash160 value can be converted into a bitcoin address checksum format.
        /// </summary>
        Pay2PublicKeyHash = 2,
        /// <summary>
        /// OP_0 OP_RETURN OP_PUSH4 ...
        /// </summary>
        OpRetPush4 = 3,
        /// <summary>
        /// Pay to script hash
        /// </summary>
        Pay2ScriptHash = 4,
        /// <summary>
        /// B:// protocol
        /// </summary>
        OpRetB = 5,
        /// <summary>
        /// Bcat protocol
        /// </summary>
        OpRetBcat = 6,
        /// <summary>
        /// Bcat part protocol
        /// </summary>
        OpRetBcatPart = 7
    }
}
