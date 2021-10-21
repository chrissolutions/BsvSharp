#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Crypto;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Encrypt
{
    public class KzEciesTests
    {
        private readonly PrivateKey _aliceKey = PrivateKey.FromWif("L1Ejc5dAigm5XrM3mNptMEsNnHzS7s51YxU7J61ewGshZTKkbmzJ");
        private readonly PrivateKey _bobKey = PrivateKey.FromWif("KxfxrUXSMjJQcb3JgnaaA6MqsrKQ1nBSxvhuigdKRyFiEm6BZDgG");

        [Fact]
        public void BsvPrivateKeyTests()
        {
            var hex = "96c132224121b509b7d0a16245e957d9192609c5637c6228311287b1be21627a";
            var wifLivenet = "L2Gkw3kKJ6N24QcDuH4XDqt9cTqsKTVNDGz1CRZhk9cq4auDUbJy";

            var privKey = new PrivateKey(hex);
            var privKey2 = PrivateKey.FromWif(wifLivenet);
            Assert.Equal(privKey, privKey2);
        }

        [Fact]
        public void WifToPrivateKey_Test()
        {
            const string wifKeySource = "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ";
            const string privateKeyHex = "0C28FCA386C7A227600B2FE50B7CAE11EC86D3BF1FBE471BE89827E19D72AA1D";

            var privkey = PrivateKey.FromWif(wifKeySource);
            var decodedPrivKey = privkey.ToHex().ToUpper();
            //var outPrivKey = privkey.ToString();
            Assert.Equal(privateKeyHex, decodedPrivKey);
        }

        //[Fact]
        //public void AesJsTests()
        //{
        //    var encrypted = new byte[] { 215, 238, 74, 62, 188, 204, 110, 226, 60, 165, 249, 53, 192, 105, 170, 242 };
        //    //            [11, 46, 168, 90, 158, 182, 37, 132, 9, 110, 109, 228, 252, 198, 9, 2]
        //    var iv = new byte[] { 235, 86, 206, 143, 225, 253, 82, 192, 220, 64, 112, 106, 26, 194, 193, 218 };
        //    var key = new byte[] { 101, 134, 119, 140, 213, 160, 234, 3, 148, 221, 195, 153, 240, 69, 253, 255 };
        //    var plaintext = new byte[] { 168, 8, 221, 252, 21, 193, 149, 122, 213, 17, 252, 154, 186, 66, 168, 129 }; 
        //    //[173, 10, 220, 38, 56, 150, 251, 88, 72, 82, 231, 198, 251, 106, 85, 180]

        //    using var aes = new AesCryptoServiceProvider
        //    {
        //        Padding = PaddingMode.None, Mode = CipherMode.CBC, Key = key, IV = iv
        //    };
        //    byte[] r;

        //    var e = aes.CreateEncryptor();
        //    var r1 = e.TransformFinalBlock(plaintext, 0, plaintext.Length);

        //    using (var ms = new MemoryStream()) {
        //        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
        //            cs.Write(plaintext);
        //        }
        //        r = ms.ToArray();
        //    }
        //    Assert.Equal(encrypted, r);
        //}

        [Fact]
        public void Ecies_Default_Test()
        {
            const string message = "attack at dawn";
            const string encrypted = "0339e504d6492b082da96e11e8f039796b06cd4855c101e2492a6f10f3e056a9e712c732611c6917ab5c57a1926973bc44a1586e94a783f81d05ce72518d9b0a80e2e13c7ff7d1306583f9cc7a48def5b37fbf2d5f294f128472a6e9c78dede5f5";
            // encrypted broken down: 
            // priv.pubkey 0339e504d6492b082da96e11e8f039796b06cd4855c101e2492a6f10f3e056a9e7
            // ivbuf       12c732611c6917ab5c57a1926973bc44
            // encrypted   a1586e94a783f81d05ce72518d9b0a80
            // sig         e2e13c7ff7d1306583f9cc7a48def5b37fbf2d5f294f128472a6e9c78dede5f5

            var alice = new Ecies { PrivateKey = _aliceKey, PublicKey = _bobKey.CreatePublicKey() };
            var bob = new Ecies { PrivateKey = _bobKey };

            var cipherText = alice.Encrypt(message);
            Assert.Equal(encrypted, cipherText.ToHex());

            var decryptedText = bob.DecryptToUtf8(cipherText);
            Assert.Equal(message, decryptedText);
        }

        [Fact]
        public void Ecies_ShortTag_Test()
        {
            const string message = "attack at dawn";
            const string encrypted = "0339e504d6492b082da96e11e8f039796b06cd4855c101e2492a6f10f3e056a9e712c732611c6917ab5c57a1926973bc44a1586e94a783f81d05ce72518d9b0a80e2e13c7f";
            // encrypted broken down: 
            // priv.pubkey 0339e504d6492b082da96e11e8f039796b06cd4855c101e2492a6f10f3e056a9e7
            // ivbuf       12c732611c6917ab5c57a1926973bc44
            // encrypted   a1586e94a783f81d05ce72518d9b0a80
            // sig         e2e13c7f

            var alice = new Ecies { PrivateKey = _aliceKey, PublicKey = _bobKey.CreatePublicKey(), ShortTag = true };
            var bob = new Ecies { PrivateKey = _bobKey, ShortTag = true };

            var cipherText = alice.Encrypt(message);
            Assert.Equal(encrypted, cipherText.ToHex());

            var decryptedText = bob.DecryptToUtf8(cipherText);
            Assert.Equal(message, decryptedText);
        }

        [Fact]
        public void Ecies_NoKey_Test()
        {
            const string message = "attack at dawn";
            const string encrypted = "12c732611c6917ab5c57a1926973bc44a1586e94a783f81d05ce72518d9b0a80e2e13c7ff7d1306583f9cc7a48def5b37fbf2d5f294f128472a6e9c78dede5f5";
            // encrypted broken down: 
            // priv.pubkey 
            // ivbuf       12c732611c6917ab5c57a1926973bc44
            // encrypted   a1586e94a783f81d05ce72518d9b0a80
            // sig         e2e13c7ff7d1306583f9cc7a48def5b37fbf2d5f294f128472a6e9c78dede5f5

            var alice = new Ecies { PrivateKey = _aliceKey, PublicKey = _bobKey.CreatePublicKey(), NoKey = true };
            var bob = new Ecies { PrivateKey = _bobKey, PublicKey = _aliceKey.CreatePublicKey(), NoKey = true };

            var cipherText = alice.Encrypt(message);
            Assert.Equal(encrypted, cipherText.ToHex());

            var decryptedText = bob.DecryptToUtf8(cipherText);
            Assert.Equal(message, decryptedText);
        }

        [Fact]
        public void Ecies_ShortKey_NoKey_Test()
        {
            const string message = "attack at dawn";
            const string encrypted = "12c732611c6917ab5c57a1926973bc44a1586e94a783f81d05ce72518d9b0a80e2e13c7f";
            // encrypted broken down: 
            // priv.pubkey 
            // ivbuf       12c732611c6917ab5c57a1926973bc44
            // encrypted   a1586e94a783f81d05ce72518d9b0a80
            // sig         e2e13c7f

            var alice = new Ecies { PrivateKey = _aliceKey, PublicKey = _bobKey.CreatePublicKey(), NoKey = true, ShortTag = true };
            var bob = new Ecies { PrivateKey = _bobKey, PublicKey = _aliceKey.CreatePublicKey(), NoKey = true, ShortTag = true };

            var cipherText = alice.Encrypt(message);
            Assert.Equal(encrypted, cipherText.ToHex());

            var decryptedText = bob.DecryptToUtf8(cipherText);
            Assert.Equal(message, decryptedText);
        }

        [Fact]
        public void Ecies_Decrypt_Message()
        {
            const string message = "this is my test message";
            var messageBuffer = Encoders.Utf8.Decode(message);

            var cipherBuffer = Encoders.Base64.Decode("AznlBNZJKwgtqW4R6PA5eWsGzUhVwQHiSSpvEPPgVqnnxhFRo3jc99xEb2Eu3Lc2bjnRRRkfdBAfaiiJ1v+qCd6P5Mu9IJ406w7cy0VTy2w70myzVbE+EsYkSjLgeDtOuszO4tw2GmUl9tQEVBdUZ3c=");
            var alice = new Ecies { PrivateKey = _aliceKey, PublicKey = _aliceKey.CreatePublicKey()};

            var decrypt = alice.Decrypt(cipherBuffer);
            Assert.Equal(messageBuffer, decrypt);
        }
    }
}
