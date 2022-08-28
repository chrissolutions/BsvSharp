#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Encoding
{
    public enum Base58Type
    {
        PrivateKeyCompressed,
        PrivateKeyUncompressed,
        PubkeyAddress,
        ScriptAddress,
        SecretKey,
        HdPublicKey,
        HdSecretKey,

        MaxBase58Types
    };
}