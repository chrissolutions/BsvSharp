namespace CafeLib.BsvSharp.Signatures
{
    /// <summary>
    /// Base signature hash types
    /// Base sig hash types not defined in this enum may be used, but they will be
    /// represented as UNSUPPORTED.  See transaction
    /// c99c49da4c38af669dea436d3e73780dfdb6c1ecf9958baa52960e8baee30e73 for an
    /// example where an unsupported base sig hash of 0 was used.
    /// </summary>
    public enum BaseSignatureHashEnum : byte
    {
        Unsupported = 0,
        All = SignatureHashEnum.All,
        None = SignatureHashEnum.None,
        Single = SignatureHashEnum.Single
    };
}