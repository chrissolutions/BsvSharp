using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Passphrase;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Extensions
{
    public static class MnemonicExtensions
    {
        public static UInt512 ToSeed(this Mnemonic _, string mnemonic, string password = "") => HdPrivateKey.Bip39Seed(mnemonic, password);
    }
}
