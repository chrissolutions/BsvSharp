﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class KzPublicKeyTests
    {
        [Fact]
        public void FromPrivateKey()
        {
            const string privateHex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string publicHex = "02a1633cafcc01ebfb6d78e39f687a1f0995c62fc95f51ead10a02ee0be551b5dc";

            var privkey = PrivateKey.FromHex(privateHex);
            var pubkey = privkey.CreatePublicKey();
            Assert.Equal(privateHex, privkey.ToHex());
            Assert.Equal(publicHex, pubkey.ToHex());
            Assert.True(privkey.VerifyPublicKey(pubkey));
        }

        [Fact]
        public void PublicKey_Data_Test()
        {
            const string publicHex = "02a1633cafcc01ebfb6d78e39f687a1f0995c62fc95f51ead10a02ee0be551b5dc";
            var publicKey = PublicKey.FromHex(publicHex);
            var bytes = publicKey.ToArray();
            Assert.Equal(0, publicKey.Data.SequenceCompareTo(bytes));
        }

        [Fact]
        public void PublicKey_Address_Test()
        {
            const string privateHex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            const string publicHex = "02a1633cafcc01ebfb6d78e39f687a1f0995c62fc95f51ead10a02ee0be551b5dc";
            const string publicKeyAddress = "17JarKo61PkpuZG3GyofzGmFSCskGRBUT3";

            var privkey = PrivateKey.FromHex(privateHex);
            var pubkey = privkey.CreatePublicKey();
            var address = pubkey.ToAddress();

            Assert.Equal(privateHex, privkey.ToHex());
            Assert.Equal(publicHex, pubkey.ToHex());
            Assert.True(privkey.VerifyPublicKey(pubkey));
            Assert.Equal(publicKeyAddress, address.ToString());
        }

        [Fact]
        public void TestPublicKey_Hexadecimal()
        {
            const string publicKeyHex = "041ff0fe0f7b15ffaa85ff9f4744d539139c252a49710fb053bb9f2b933173ff9a7baad41d04514751e6851f5304fd243751703bed21b914f6be218c0fa354a341";
            var publicKey = new PublicKey(publicKeyHex);
            var result = publicKey.ToHex();
            Assert.Equal(publicKeyHex, result);
            Assert.Equal(publicKey.ToString(), result);
        }
    }
}