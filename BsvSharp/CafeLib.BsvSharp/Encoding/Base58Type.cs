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