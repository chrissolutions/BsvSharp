#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Cryptography;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Encrypt
{
    public class KzEncryptTests
    {
        [Fact]
        public void Aes_Encrypt_Decrypt_String()
        {
            const string msg = "all good men must act";
            const string password = "really strong password...;-)";

            var encrypt = AesEncryption.Encrypt(msg, password);
            var decrypt = AesEncryption.Decrypt(encrypt, password);
            Assert.Equal(msg, decrypt);
        }

        [Fact]
        public void Aes_Encrypt_Decrypt_With_IV()
        {
            var msg = "all good men must act";
            var data1 = msg.Utf8ToBytes();
            var password = "really strong password...;-)";

            var key = AesEncryption.KeyFromPassword(password);

            var iv = AesEncryption.InitializationVector(key, data1);
            var edata1 = AesEncryption.Encrypt(data1, key, iv, true);
            var ddata1 = AesEncryption.Decrypt(edata1, key, iv);
            Assert.Equal(data1, ddata1);
            Assert.Equal(msg, Encoders.Utf8.Encode(ddata1));
        }

        [Fact]
        public void AesEncryptStringTests_BadPassword()
        {
            const string msg = "all good men must act";
            const string password = "really strong password...;-)";

            var encrypt = AesEncryption.Encrypt(msg, password);
            Assert.Throws<ApplicationException>(() => AesEncryption.Decrypt(encrypt, "Bad password"));
        }
    }
}