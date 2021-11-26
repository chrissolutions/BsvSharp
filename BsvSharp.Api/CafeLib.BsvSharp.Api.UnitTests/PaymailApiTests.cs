#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.Paymail;
using CafeLib.BsvSharp.Keys;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests
{
    public class PaymailApiTests
    {
        public PaymailClient Paymail { get; } = new PaymailClient();

        [Theory]
        [InlineData("moneybutton.com", Capability.Pki, true)]
        [InlineData("moneybutton.com", Capability.PaymentDestination, true)]
        [InlineData("moneybutton.com", Capability.SenderValidation, false)]
        [InlineData("moneybutton.com", Capability.VerifyPublicKeyOwner, true)]
        [InlineData("moneybutton.com", Capability.ReceiverApprovals, false)]
        public async Task EnsureCapabilityFor_Test(string domain, Capability c, bool expectedResult)
        {
            var result = await Paymail.DomainHasCapability(domain, c);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("kzpaymailasp@kzbsv.org", "02c4aa80834a289b43870b56a6483c924b57650eebe6e5185b19258c76656baa35")]
        [InlineData("testpaymail@kizmet.org", "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68")]
        [InlineData("tonesnotes@moneybutton.com", "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef")]
        public async Task GetPublicKey_Test(string paymail, string pubkey)
        {
            //var privkey = KzElectrumSv.GetMasterPrivKey("<replace with actual wallet seed>").Derive($"0/{int.MaxValue}").PrivKey;
            //var privkey = PrivateKey.FromB58("KxXvocKqZtdHvZP5HHNShrwDQVz2muNPisrzoyeyhXc4tZhBj1nM");
            //var pubkey = privkey.GetPublicKey();
            var paymailKey = await Paymail.GetPublicKey(paymail);
            var expectedKey = new PublicKey(pubkey);
            Assert.Equal(expectedKey, paymailKey);
        }

        [Theory]
        [InlineData("kzpaymailasp@kzbsv.org", "02c4aa80834a289b43870b56a6483c924b57650eebe6e5185b19258c76656baa35")]
        [InlineData("testpaymail@kizmet.org", "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68")]
        [InlineData("tonesnotes@moneybutton.com", "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef")]
        public async Task GetPublicKey_List_Test(string paymail, string pubkey)
        {
            var paymailKey = await Paymail.GetPublicKey(paymail);
            var expectedKey = new PublicKey(pubkey);
            Assert.Equal(expectedKey, paymailKey);
        }

        [Theory]
        [InlineData("kzpaymailasp@kzbsv.org", "02c4aa80834a289b43870b56a6483c924b57650eebe6e5185b19258c76656baa35", true)]
        [InlineData("testpaymail@kizmet.org", "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68", true)]
        [InlineData("tonesnotes@moneybutton.com", "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef", true)]
        [InlineData("testpaymail@kizmet.org", "02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef", false)]
        [InlineData("tonesnotes@moneybutton.com", "02fe6a13c0734578b77d28680aac58a78eb1722dd654117451b8820c9380b10e68", false)]
        public async Task VerifyPubKey(string paymail, string pubkey, bool expectedResult)
        {
            var result = await Paymail.VerifyPubKey(paymail, new PublicKey(pubkey));
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetOutputScript_Test()
        {
            // Paymail server configuration for testpaymail@kizmet.org:
            // testpaymail@kizmet.org
            // Generate public addresses beneath this derivation path: M/0
            // Master public key for derivations (M): xpub661MyMwAqRbcEaJYm4GjL9XnYrwbTR7Rug3oZ66juJHMXYwCYD4Z3RVgyoPhhpU97Ls9fACV3Y7kYqMPxGAA8XWFdPpaXAj3qb8VHnRMU8c
            // Public key returned by GetPublicKey("testpaymail@kizmet.org"): M/0/{int.MaxValue}
            // Private key for that public key: m/0/{int.MaxValue}
            // var key = KzElectrumSv.GetMasterPrivKey("<replace with actual wallet seed>").Derive($"0/{int.MaxValue}").PrivKey;
            var key = PrivateKey.FromBase58("KxXvocKqZtdHvZP5HHNShrwDQVz2muNPisrzoyeyhXc4tZhBj1nM");

            var r = new PaymailClient();
            var s = await r.GetOutputScript(key, "tonesnotes@moneybutton.com", "testpaymail@kizmet.org");
            Assert.True(s.Length > 0);
        }

#if false
        [Theory]
        [InlineData("tonesnotes@moneybutton.com", "147@moneybutton.com02019-06-07T20:55:57.562ZPayment with Money Button", "H4Q8tvj632hXiirmiiDJkuUN9Z20zDu3KaFuwY8cInZiLhgVJKJdKrZx1RZN06E/AARnFX7Fn618OUBQigCis4M=", true)]
        [InlineData("tone@simply.cash", "tone@simply.cash02019-07-11T12:24:04.260Z", "IJ1C3gXhnUxKpU8JOIjGHC8talwIgfIXKMmRZ5mjysb0eHjLPQP5Tlx29Xi5KNDZuOsOPk8HiVtwKAefq1pJVDs=", true)]
        public async Task VerifyMessageSignature(string paymail, string message, string signature, bool expectedResult)
        {
            var (result, pubkey) = await Paymail.IsValidSignature(message, signature, paymail, null);
            Assert.Equal(expectedResult, result);
            (result, _) = await Paymail.IsValidSignature(message, signature, paymail, pubkey);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("tonesnotes@moneybutton.com", "147@moneybutton.com02019-06-07T20:55:57.562ZPayment with Money Button", "H4Q8tvj632hXiirmiiDJkuUN9Z20zDu3KaFuwY8cInZiLhgVJKJdKrZx1RZN06E/AARnFX7Fn618OUBQigCis4M=", true)]
        [InlineData("tone@simply.cash", "tone@simply.cash02019-07-11T12:24:04.260Z", "IJ1C3gXhnUxKpU8JOIjGHC8talwIgfIXKMmRZ5mjysb0eHjLPQP5Tlx29Xi5KNDZuOsOPk8HiVtwKAefq1pJVDs=", true)]
        public async Task VerifyMessageSignatureViaPublicKey(string paymail, string message, string signature, bool expectedResult)
        {
            var pubKey = await Paymail.GetPublicKey(paymail);
            Assert.True(pubKey.IsValid);
            Assert.Equal(expectedResult, pubKey.VerifyMessage(message, signature));
        }

        [Fact]
        public void SignatureTest()
        {
            //var pk = await new KzPaymailClient().GetPublicKey("147@moneybutton.com");
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
            const string purpose = "";
            const string signature = "IJ1C3gXhnUxKpU8JOIjGHC8talwIgfIXKMmRZ5mjysb0eHjLPQP5Tlx29Xi5KNDZuOsOPk8HiVtwKAefq1pJVDs=";

            var pub = await new PaymailClient().GetPublicKey("tone@simply.cash");
            // var pub = new PublicKey();
            // pub.Set("02e36811b6a8db1593aa5cf97f91dd2211af1c38b9890567e58367945137dca8ef".HexToBytes());

            var message = $"{from}{("0")}{when}{purpose}";
            var ok = pub.VerifyMessage(message, signature);

            Assert.True(ok);
        }

        [Fact]
        public void SignatureTest2()
        {
            const string paymail = "some@paymail.com";
            const string amount = "500";
            const string when = "2019-03-01T05:00:00.000Z";
            const string purpose = "some reason";

            var message = $"{paymail}{amount}{when}{purpose}";

            var privkey = PrivateKey.FromBase58("KxWjJiTRSA7oExnvbWRaCizYB42XMKPxyD6ryzANbdXCJw1fo4sR");
            var signature = privkey.SignMessageToBase64(message);
            Assert.Equal("H1CV5DE7tya0jM2ZynueSTRgkv4CpNY9/pz5lK5ENdGOWXCt/MTReMcQ54LCRt4ogf/g53HokXDpuSfc1D5gBUE=", signature);

            var pub = privkey.CreatePublicKey();
            var ok = pub.VerifyMessage(message, signature);
            Assert.True(ok);
        }

#endif
    }
}
