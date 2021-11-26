#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.Paymail;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests {
    public class PaymailClientTests
    {
        [Fact]
        public async Task EnsureCapabilityFor()
        {
            const string domain = "moneybutton.com";
            var r = new PaymailClient();
            var pkiOk = await r.DomainHasCapability(domain, Capability.Pki);
            var pdOk = await r.DomainHasCapability(domain, Capability.PaymentDestination);
            var svOk = await r.DomainHasCapability(domain, Capability.SenderValidation);
            var vpkoOk = await r.DomainHasCapability(domain, Capability.VerifyPublicKeyOwner);
            var raOk = await r.DomainHasCapability(domain, Capability.ReceiverApprovals);
        }

        [Theory]
        [InlineData(
            "kzpaymailasp@kzbsv.org", 
            "02c4aa80834a289b43870b56a6483c924b57650eebe6e5185b19258c76656baa35"
        )]
        [InlineData(
            "testpaymail@kizmet.org", 
            "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68"
        )]
        [InlineData(
            "tonesnotes@moneybutton.com", 
            "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef"
        )]
        public async Task GetPubKey(string email, string publicKey)
        {
            var r = new PaymailClient();

            //var privkey = KzElectrumSv.GetMasterPrivKey("<replace with actual wallet seed>").Derive($"0/{int.MaxValue}").PrivKey;
            //var privkey = PrivateKey.FromB58("KxXvocKqZtdHvZP5HHNShrwDQVz2muNPisrzoyeyhXc4tZhBj1nM");
            //var pubkey = privkey.GetPubKey();
            var pubkey = new PublicKey(publicKey);
            var k = await r.GetPublicKey(email);
            Assert.Equal(k, pubkey);
        }

        [Fact]
        public async Task VerifyPubKey()
        {
            var r = new PaymailClient();

            foreach (var tc in new []
            {
                new { r = true, p = "kzpaymailasp@kzbsv.org", k = "02c4aa80834a289b43870b56a6483c924b57650eebe6e5185b19258c76656baa35" },
                new { r = true, p = "testpaymail@kizmet.org", k = "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68" },
                new { r = true, p = "tonesnotes@moneybutton.com", k = "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef" },   
                new { r = false, p = "testpaymail@kizmet.org", k = "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef" },
                new { r = false, p = "tonesnotes@moneybutton.com", k = "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68" },   
            })
            {
                var pubkey = new PublicKey(tc.k);
                var ok = await r.VerifyPubKey(tc.p, pubkey);
                if (tc.r)
                    Assert.True(ok);
                else
                    Assert.False(ok);
            }
        }

        [Fact]
        public async Task GetOutputScript()
        {
            // Paymail server configuration for testpaymail@kizmet.org:
            // testpaymail@kizmet.org
            // Generate public addresses beneath this derivation path: M/0
            // Master public key for derivations (M): xpub661MyMwAqRbcEaJYm4GjL9XnYrwbTR7Rug3oZ66juJHMXYwCYD4Z3RVgyoPhhpU97Ls9fACV3Y7kYqMPxGAA8XWFdPpaXAj3qb8VHnRMU8c
            // Public key returned by GetPubKey("testpaymail@kizmet.org"): M/0/{int.MaxValue}
            // Private key for that public key: m/0/{int.MaxValue}
            // var key = KzElectrumSv.GetMasterPrivKey("<replace with actual wallet seed>").Derive($"0/{int.MaxValue}").PrivKey;
            var key = PrivateKey.FromBase58("KxXvocKqZtdHvZP5HHNShrwDQVz2muNPisrzoyeyhXc4tZhBj1nM");

            var r = new PaymailClient();
            var s = await r.GetOutputScript(key, "tonesnotes@moneybutton.com", "testpaymail@kizmet.org");
            Assert.True(s.Length > 0);
        }

        [Fact]
        public void SignatureTest2()
        {
            var paymail = "some@paymail.com";
            var amount = "500";
            var when = "2019-03-01T05:00:00.000Z";
            var purpose = "some reason";

            var message = $"{paymail}{amount}{when}{purpose}";

            var privkey = PrivateKey.FromBase58("KxWjJiTRSA7oExnvbWRaCizYB42XMKPxyD6ryzANbdXCJw1fo4sR");
            var signature = privkey.SignMessage(message).ToString();
            Assert.Equal("H7mUf95shi5aTyzDnI7DQWSoGAI2nbPG+56IsSkINJdtAYvhr5ivp2ZdQr91vASAwlm62pAhzoEJ0lcS2ja7llI=", signature);

            var pub = privkey.CreatePublicKey();
            var ok = pub.VerifyMessage(message, signature);
            Assert.True(ok);
        }


#if false
        [Fact]
        public async Task VerifyMessageSignature()
        {
            var r = new PaymailClient();

            foreach (var tc in new[]
            {
                //new { p = "testpaymail@kizmet.org", k = "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68" },
                new { r = true, p = "tonesnotes@moneybutton.com", m = "147@moneybutton.com02019-06-07T20:55:57.562ZPayment with Money Button", s = "H4Q8tvj632hXiirmiiDJkuUN9Z20zDu3KaFuwY8cInZiLhgVJKJdKrZx1RZN06E/AARnFX7Fn618OUBQigCis4M=" },
                new { r = true, p = "tone@simply.cash", m = "tone@simply.cash02019-07-11T12:24:04.260Z", s = "IJ1C3gXhnUxKpU8JOIjGHC8talwIgfIXKMmRZ5mjysb0eHjLPQP5Tlx29Xi5KNDZuOsOPk8HiVtwKAefq1pJVDs=" },
            })
            {
                var (ok, pubkey) = await r.IsValidSignature(tc.m, tc.s, tc.p, null);
                Assert.True(ok);
                (ok, _) = await r.IsValidSignature(tc.m, tc.s, tc.p, pubkey);
                Assert.True(ok);
            }
        }

        [Fact]
        public void SignatureTest()
        {
            //var pk = await new PaymailClient().GetPubKey("147@moneybutton.com");
            var pub = new PublicKey("02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef");

            var message = "147@moneybutton.com02019-06-07T20:55:57.562ZPayment with Money Button";
            var signature = "H4Q8tvj632hXiirmiiDJkuUN9Z20zDu3KaFuwY8cInZiLhgVJKJdKrZx1RZN06E/AARnFX7Fn618OUBQigCis4M=";
            var ok = pub.VerifyMessage(message, signature);

            Assert.True(ok);
        }

        [Fact]
        public async Task SignatureTest1()
        {
            //var To = "testpaymail@kizmet.org";
            //var PubKey = "";
            const string from = "tone@simply.cash";
            const string when = "2019-07-11T12:24:04.260Z";
            const string amount = "";
            const string purpose = "";
            const string signature = "IJ1C3gXhnUxKpU8JOIjGHC8talwIgfIXKMmRZ5mjysb0eHjLPQP5Tlx29Xi5KNDZuOsOPk8HiVtwKAefq1pJVDs=";

            var pub = await new PaymailClient().GetPublicKey("tone@simply.cash");
            // var pub = new KzPubKey();
            // pub.Set("02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef".HexToBytes());

            var message = $"{from}{(amount == "" ? "0" : amount)}{when}{purpose}";
            var ok = pub.VerifyMessage(message, signature);

            Assert.True(ok);
        }
#endif
    }
}
