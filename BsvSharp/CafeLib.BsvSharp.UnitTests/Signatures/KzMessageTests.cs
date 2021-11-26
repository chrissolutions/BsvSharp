#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Signatures;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Signatures
{
    public class KzMessageTests
    {
        [Fact]
        public void SignMessageTest()
        {
            const string message = "147@moneybutton.com02019-06-07T20:55:57.562ZPayment with Money Button";
            const string signature = "HxjyaWDKtUXXN78HOpVwK9xTuIjtP2AZeOTKrbo/PnBJMa4qVhDiyhzulBL89zJnp0sxqq4hpt6mUmGrd/Q/R2U=";

            var privateKey = PrivateKey.FromWif("L3nrwRssVKMkScjejmmu6kmq4hSuUApJnFdW1hGvBP69jnQuKYCh");
            var sig = privateKey.SignMessage(message);
            Assert.Equal(signature, sig.ToString());

            var ok = privateKey.CreatePublicKey().VerifyMessage(message, sig);
            Assert.True(ok);
        }

        [Fact]
        public void VerifyMessageSignatureTest()
        {
            const string message = "This is an example of a signed message.";
            const string signature = "H6sliOnVrD9r+J8boZAKHZwBIW2zLiD72IfTIF94bfZhBI0JdMu9AM9rrF7P6eH+866YvM4H9xWGVN4jMJZycFU=";

            var publicKey = PublicKey.FromSignedMessage(message, signature);
            var ok = publicKey.VerifyMessage(message, signature);
            Assert.True(ok);
        }

        [Theory]
        [InlineData(
            "15jZVzLc9cXz5PUFFda5A4Z7kZDYPg2NnL",
            "L3TiCqmvPkXJpzCCZJuhy6wQtJZWDkR1AuqFY4Utib5J5XLuvLdZ",
            "This is an example of a signed message.",
            "H6sliOnVrD9r+J8boZAKHZwBIW2zLiD72IfTIF94bfZhBI0JdMu9AM9rrF7P6eH+866YvM4H9xWGVN4jMJZycFU="
        )]
        [InlineData(
            "1QFqqMUD55ZV3PJEJZtaKCsQmjLT6JkjvJ",
            "5HxWvvfubhXpYYpS3tJkw6fq9jE9j18THftkZjHHfmFiWtmAbrj",
            "hello world",
            "G+dnSEywl3v1ijlWXvpY6zpu+AKNNXJcVmrdE35m0mMlzwFzXDiNg+uZrG9k8mpQL6sjHKrlBoDNSA+yaPW7PEA="
        )]
        [InlineData(
            "1GvPJp7H8UYsYDvE4GFoV4f2gSCNZzGF48",
            "5JEeah4w29axvf5Yg9v9PKv86zcCN9qVbizJDMHmiSUxBqDFoUT",
            "This is an example of a signed message2",
            "G8YNwlo+I36Ct+hZKGSBFl3q8Kbx1pxPpwQmwdsG85io76+DUOHXqh/DfBq+Cn2R3C3dI//g3koSjxy7yNxJ9m8="
        )]
        [InlineData(
            "1GvPJp7H8UYsYDvE4GFoV4f2gSCNZzGF48",
            "5JEeah4w29axvf5Yg9v9PKv86zcCN9qVbizJDMHmiSUxBqDFoUT",
            "this is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long message",
            "HFKBHewleUsotk6fWG0OvWS/E2pP4o5hixdD6ui60in/x4376FBI4DvtJYrljXLNJTG1pBOZG+qRT/7S9WiIBfQ="
        )]
        //[InlineData(
        //    "1Q1wVsNNiUo68caU7BfyFFQ8fVBqxC2DSc",
        //    null,
        //    "Localbitcoins.com will change the world",
        //    "IJ/17TjGGUqmEppAliYBUesKHoHzfY4gR4DW0Yg7QzrHUB5FwX1uTJ/H21CF8ncY8HHNB5/lh8kPAOeD5QxV8Xc="
        //)]
        public void VerifyMessage_Test(string address, string privateKey, string message, string signature)
        {
            var addr = new Address(address);
            var privKey = PrivateKey.FromBase58(privateKey);
            var sign = new Signature(Encoders.Base64.Decode(signature));

            var pubKey = privKey.CreatePublicKey();
            Assert.Equal(addr.PubKeyHash, pubKey.GetId());
            Assert.True(pubKey.VerifyMessage(message, sign));
        }
    }
}
