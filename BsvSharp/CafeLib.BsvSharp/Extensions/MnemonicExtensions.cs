using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Passphrase;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Extensions
{
    public static class MnemonicExtensions
    {
        public static UInt512 ToSeed(this Mnemonic _, string phrase, string password = "") => HdPrivateKey.Bip39Seed(phrase, password);
    }
}
