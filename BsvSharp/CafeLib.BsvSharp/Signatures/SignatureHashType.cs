using CafeLib.BsvSharp.Keys;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CafeLib.BsvSharp.Signatures
{
    /// <summary>
    /// Signature hash type.
    /// </summary>
    public struct SignatureHashType : IEquatable<SignatureHashType>
    {
        private SignatureHashEnum SignatureHash => (SignatureHashEnum)RawSigHashType;

        public SignatureHashType()
        {
            RawSigHashType = (uint)SignatureHashEnum.All;
        }

        public SignatureHashType(SignatureHashEnum sigHash)
        {
            RawSigHashType = (uint)sigHash;
        }

        public SignatureHashType(uint sigHash)
        {
            RawSigHashType = sigHash;
        }

        public bool IsDefined 
        {
            get
            {
                var baseType = SignatureHash & ~(SignatureHashEnum.ForkId | SignatureHashEnum.AnyoneCanPay);
                return baseType >= SignatureHashEnum.All && baseType <= SignatureHashEnum.Single;
            }
        }

        public bool HasForkId => (SignatureHash & SignatureHashEnum.ForkId) != 0;

        public bool HasAnyoneCanPay => (SignatureHash & SignatureHashEnum.AnyoneCanPay) != 0;

        public uint RawSigHashType { get; }

        public SignatureHashType WithBaseType(BaseSignatureHashEnum baseSigHashType)
        {
            return new SignatureHashType((RawSigHashType & ~(uint)0x1f) | (uint)baseSigHashType);
        }

        public SignatureHashType WithForkValue(uint forkId)
        {
            return new SignatureHashType((forkId << 8) | (RawSigHashType & 0xff));
        }

        public SignatureHashType WithForkId(bool forkId = true)
        {
            return new SignatureHashType((RawSigHashType & ~(uint)SignatureHashEnum.ForkId) | (forkId ? (uint)SignatureHashEnum.ForkId : 0));
        }

        public SignatureHashType WithAnyoneCanPay(bool anyoneCanPay = true)
        {
            return new SignatureHashType((RawSigHashType & ~(uint)SignatureHashEnum.AnyoneCanPay) | (anyoneCanPay ? (uint)SignatureHashEnum.AnyoneCanPay : 0));
        }

        public BaseSignatureHashEnum GetBaseType() { return (BaseSignatureHashEnum)((int)RawSigHashType & 0x1f); }

        public bool IsBaseNone => ((int)RawSigHashType & 0x1f) == (int)BaseSignatureHashEnum.None;

        public bool IsBaseSingle => ((int)RawSigHashType & 0x1f) == (int)BaseSignatureHashEnum.Single;

        public uint GetForkValue() { return RawSigHashType >> 8; }

        public override string ToString()
        {
            return $"{RawSigHashType}";
        }

        public bool Equals(SignatureHashType other)
        {
            return RawSigHashType == other.RawSigHashType;
        }

        public override bool Equals(object obj)
        {
            return obj is SignatureHashType type && Equals(type);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SignatureHash, IsDefined, HasForkId, HasAnyoneCanPay, RawSigHashType, IsBaseNone, IsBaseSingle);
        }

        public static bool operator ==(SignatureHashType left, SignatureHashType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SignatureHashType left, SignatureHashType right)
        {
            return !(left == right);
        }
    }
}