#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Network;
using Xunit;
// ReSharper disable StringLiteralTypo

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
            const string publicKeyAddress = "17JarKo61PkpuZG3GyofzGmFSCskGRBUT3";

            var key1 = new PrivateKey(hex);
            var key2 = PrivateKey.FromBase58(b58);

            var pubKey1 = key1.CreatePublicKey();
            var pubKey2 = key2.CreatePublicKey();

            Assert.Equal(pubKey1, pubKey2);
            Assert.Equal(publicKeyAddress, pubKey1.ToAddress().ToString());
            Assert.Equal(publicKeyAddress, pubKey2.ToAddress().ToString());
        }

        [Fact]
        public void PrivateKey_From_To_Wif_Test()
        {
            const string wifMainnet = "L2Gkw3kKJ6N24QcDuH4XDqt9cTqsKTVNDGz1CRZhk9cq4auDUbJy";

            var privateKey = PrivateKey.FromWif(wifMainnet);
            var wifKey = privateKey.ToWif();
            Assert.True(wifKey.IsValid);
            Assert.Equal(wifMainnet, wifKey.ToString());
        }

        [Fact]
        public void PrivateKey_From_To_Wif_Uncompressed_Test()
        {
            const string wifMainnetUncompressed = "5JxgQaFM1FMd38cd14e3mbdxsdSa9iM2BV6DHBYsvGzxkTNQ7Un";

            var privateKey = PrivateKey.FromWif(wifMainnetUncompressed);
            var wifKey = WifPrivateKey.FromPrivateKey(privateKey);
            Assert.True(wifKey.IsValid);
            Assert.Equal(wifMainnetUncompressed, wifKey.ToString());
        }

        [Fact]
        public void Wif_PrivateKey_To_Testnet_Address_Test()
        {
            const string wifKeyText = "92VYMmwFLXRwXn5688edGxYYgMFsc3fUXYhGp17WocQhU6zG1kd";
            var wifKey = WifPrivateKey.FromString(wifKeyText);
            var privateKey = PrivateKey.FromWif(wifKeyText);
            var publicKey = PublicKey.FromPrivateKey(privateKey);
            var address = publicKey.ToAddress(wifKey.NetworkType);

            Assert.Equal(NetworkType.Test, wifKey.NetworkType);
            Assert.Equal(NetworkType.Test, address.NetworkType);
            Assert.Equal(wifKeyText, wifKey.ToString());
            Assert.Equal(wifKeyText, privateKey.ToWif(wifKey.NetworkType).ToString());
            Assert.Equal("moiAvLUw16qgrwhFGo1eDnXHC2wPMYiv7Y", address.ToString());
        }
    }
}
