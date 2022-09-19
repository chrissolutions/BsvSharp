using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Encodings;
using CafeLib.Cryptography;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Extensions
{
    public class ByteExtensionTests
    {
        [Fact]
        public void Hash256_Verify_Test()
        {
            const string text = "Bitcoin protocol is set in stone and there is no need to change it anytime in future as well as most of the global trade financial transactions are possible to be built using the current protocol itself";
            var shaHash1 = Hashes.Hash256(text.Utf8ToBytes());
            var shaHash2 = text.Utf8ToBytes().Hash256();
            var hexString = new HexEncoder().Encode(shaHash2);
            Assert.Equal(shaHash1, shaHash2);
            Assert.Equal("9ec3931d0c3da0157f170ebe5158f14a9e0b965ca9697dcff5063d2feb453fd2", hexString);
        }

        [Fact]
        public void Hash256_Text_Test()
        {
            const string text = "Bitcoin protocol is set in stone and there is no need to change it anytime in future as well as most of the global trade financial transactions are possible to be built using the current protocol itself";
            var shaHash = text.Utf8ToBytes().Hash256();
            var hexString = new HexEncoder().Encode(shaHash);
            Assert.Equal("9ec3931d0c3da0157f170ebe5158f14a9e0b965ca9697dcff5063d2feb453fd2", hexString);
        }
    }
}
