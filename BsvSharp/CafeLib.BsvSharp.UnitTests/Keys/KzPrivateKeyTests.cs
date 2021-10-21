#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class KzPrivateKeyTests
    {
        [Fact]
        public void FromHexAndB58()
        {
            const string hex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string b58 = "L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61";

            var key1 = new PrivateKey(hex);
            var key2 = PrivateKey.FromBase58(b58);
            Assert.Equal(key1, key2);
            Assert.Equal(hex, key1.ToHex());
            Assert.Equal(b58, key1.ToBase58().ToString());
            Assert.Equal(b58, key1.ToString());
            Assert.Equal(hex, key2.ToHex());
            Assert.Equal(b58, key2.ToBase58().ToString());
            Assert.Equal(b58, key2.ToString());
        }

        [Fact]
        public void PublicKey_From_PrivateKey_Test()
        {
            const string hex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string b58 = "L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61";
            const string publicKey = "17JarKo61PkpuZG3GyofzGmFSCskGRBUT3";

            var key1 = new PrivateKey(hex);
            var key2 = PrivateKey.FromBase58(b58);

            var pubKey1 = key1.CreatePublicKey();
            var pubKey2 = key2.CreatePublicKey();

            Assert.Equal(pubKey1, pubKey2);
            Assert.Equal(publicKey, pubKey1.ToString());
            Assert.Equal(publicKey, pubKey2.ToString());
        }
    }
}
