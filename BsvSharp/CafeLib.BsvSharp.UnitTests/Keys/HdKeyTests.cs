using System.Linq;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Keys.Base58;
using CafeLib.Core.Numerics;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class HdKeyTests
    {
        private const string MasterSeed = "000102030405060708090a0b0c0d0e0f";

        [Theory]
        [InlineData("m",
                    "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                    "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8")]

        [InlineData("m/0'/1",
                    "xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs",
                    "xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ")]
        public void KeyPath_Test(string strPath, string expectedPrivate, string expectedPublic)
        {
            var seed = HdPrivateKey.ToMasterSeed(MasterSeed.HexToBytes());
            var m = HdPrivateKey.FromSeed(seed);

            // Test key path.
            var path = new KeyPath(strPath);
            var hdPriv = m.Derive(path);
            var hdPriv2 = m.Derive(path);
            Assert.Equal(hdPriv, hdPriv2);
            Assert.Equal(expectedPrivate, hdPriv.ToString());
            Assert.Equal(expectedPublic, hdPriv.GetHdPublicKey().ToString());
        }

        [Theory]
        [InlineData("m",
                    "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                    "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8")]

        [InlineData("m/0'/1",
                    "xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs",
                    "xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ")]
        public void ToArray_Encode_Test(string strPath, string expectedPrivate, string expectedPublic)
        {
            var seed = HdPrivateKey.ToMasterSeed(MasterSeed.HexToBytes());
            var m = HdPrivateKey.FromSeed(seed);

            // Test key path.
            var path = new KeyPath(strPath);
            var hdPriv = m.Derive(path);

            var data = hdPriv.ToArray();
            var hdPriv2 = HdPrivateKey.FromKey(data);
            Assert.Equal(hdPriv, hdPriv2);


            var hdPub = hdPriv.GetHdPublicKey();
            data = hdPub.ToArray();
            var hdPub2 = HdPublicKey.FromKey(data);
            Assert.Equal(hdPub, hdPub2);


            Assert.Equal(expectedPrivate, hdPriv.ToString());
            Assert.Equal(expectedPublic, hdPub.ToString());
        }
    }
}
