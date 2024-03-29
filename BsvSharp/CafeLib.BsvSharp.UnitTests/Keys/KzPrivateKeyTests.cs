﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class KzPrivateKeyTests
    {
        [Fact]
        public void FromHexAndWif()
        {
            const string hex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string wif = "L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61";

            var key1 = new PrivateKey(hex);
            var key2 = PrivateKey.FromWif(wif);
            Assert.Equal(key1, key2);
            Assert.Equal(hex, key1.ToHex());
            Assert.Equal(wif, key1.ToWif().ToString());
            Assert.Equal(wif, key1.ToString());
            Assert.Equal(hex, key2.ToHex());
            Assert.Equal(wif, key2.ToWif().ToString());
            Assert.Equal(wif, key2.ToString());
        }

        [Fact]
        public void Create_PrivateKey_Compressed_Test()
        {
            var privateKey = PrivateKey.FromRandom();
            var keyStr = privateKey.ToString();
            Assert.Contains(RootService.GetNetwork().PrivateKeyCompressed, x => x == (byte)keyStr[0]);
        }

        [Fact]
        public void Create_PrivateKey_Uncompressed_Test()
        {
            var privateKey = new PrivateKey();
            var keyStr = privateKey.ToString();
            Assert.Equal(RootService.GetNetwork().PrivateKeyUncompressed[0], (byte)keyStr[0]);
        }

        [Fact]
        public void PublicKey_From_PrivateKey_Test()
        {
            const string hex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string wif = "L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61";
            const string publicKeyAddress = "17JarKo61PkpuZG3GyofzGmFSCskGRBUT3";
            const string pubHex = "02a1633cafcc01ebfb6d78e39f687a1f0995c62fc95f51ead10a02ee0be551b5dc";

            var key1 = new PrivateKey(hex);
            var key2 = PrivateKey.FromWif(wif);

            var pubKey1 = key1.CreatePublicKey();
            var pubKey2 = key2.CreatePublicKey();

            Assert.Equal(pubHex, pubKey1.ToHex());
            Assert.Equal(pubHex, pubKey2.ToHex());

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
