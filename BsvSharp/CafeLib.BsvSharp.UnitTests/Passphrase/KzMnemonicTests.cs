#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Passphrase;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Passphrase
{
    public class MnemonicTests
    {
        [Fact]
        public void RecoverLastWord() {
            const string words = "sword victory much blossom cradle sense boy float soda render arrive";
            var valid = WordLists.GetWords(Languages.English)
                .Select(word => $"{words} {word}")
                .Where(t => Mnemonic.IsValid(t))
                .ToList();
            Assert.Equal(128, valid.Count);
        }

        //[Fact]
        //public void ElectrumStandardMnemonic() 
        //{
        //    var words = "sword victory much blossom cradle sense boy float soda render arrive arrive";
        //    var h = Hashes.HmacSha512("Seed version".Utf8ToBytes(), words.Utf8NormalizedToBytes());
        //    var hb = h.Span;
        //}

        [Fact]
        public void Base6AndBase10()
        {
            var e = new byte[] { 0, 1 };
            var s10 = Mnemonic.ToDigitsBase10(e);
            Assert.Equal("256", s10);
            var s6 = Mnemonic.ToDigitsBase6(e);
            Assert.Equal("1104", s6);
            var bn10 = Mnemonic.Base10ToBigInteger(s10);
            Assert.Equal(256, bn10);
            var bn6 = Mnemonic.Base6ToBigInteger(s6);
            Assert.Equal(256, bn6);
        }

        [Fact]
        public void WordListsComplete()
        {
            Assert.True(WordLists.GetWords(Languages.English).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.English).First() == "abandon");
            Assert.True(WordLists.GetWords(Languages.Spanish).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.Spanish).First() == "ábaco");
            Assert.True(WordLists.GetWords(Languages.French).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.French).First() == "abaisser");
            Assert.True(WordLists.GetWords(Languages.Italian).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.Italian).First() == "abaco");
            Assert.True(WordLists.GetWords(Languages.Japanese).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.Japanese).First() == "あいこくしん");
            Assert.True(WordLists.GetWords(Languages.PortugueseBrazil).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.PortugueseBrazil).First() == "abacate");
            Assert.True(WordLists.GetWords(Languages.ChineseSimplified).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.ChineseSimplified).First() == "的");
            Assert.True(WordLists.GetWords(Languages.ChineseTraditional).Length == 2048);
            Assert.True(WordLists.GetWords(Languages.ChineseTraditional).First() == "的");
        }

        [Fact]
        public void IsValid()
        {
            Assert.True(Mnemonic.IsValid("afirmar diseño hielo fideo etapa ogro cambio fideo toalla pomelo número buscar"));

            Assert.False(Mnemonic.IsValid("afirmar diseño hielo fideo etapa ogro cambio fideo hielo pomelo número buscar"));

            Assert.False(Mnemonic.IsValid("afirmar diseño hielo fideo etapa ogro cambio fideo hielo pomelo número oneInvalidWord"));

            Assert.False(Mnemonic.IsValid("totally invalid phrase"));

            Assert.True(Mnemonic.IsValid("caution opprimer époque belote devenir ficeler filleul caneton apologie nectar frapper fouiller"));
        }

        [Fact]
        public void Constructors()
        {
            var words = "afirmar diseño hielo fideo etapa ogro cambio fideo toalla pomelo número buscar";
            var m1 = new Mnemonic(words);
            Assert.Equal(Languages.Spanish, m1.Language);
            Assert.Equal(m1.Words, Mnemonic.FromWords(words).Words);

            var m2 = new Mnemonic(m1.Entropy, m1.Language);
            Assert.Equal(m1.Words, m2.Words);
            Assert.Equal(m2.Words, Mnemonic.FromEntropy(m1.Entropy, m1.Language).Words);

            var m3 = new Mnemonic(new byte[] { 5, 40, 161, 175, 172, 69, 19, 67, 74, 26, 196, 233, 87, 10, 119, 18 }, Languages.Spanish);
            Assert.Equal(m1.Words, m3.Words);

            var m4 = new Mnemonic(bitLength:256);
            Assert.Equal(24, m4.Words.Split(' ').Length);
            Assert.Equal(24, Mnemonic.FromLength(256).Words.Split(' ').Length);

        }

        [Fact]
        public void WordListLength()
        {
            Assert.Equal(12, new Mnemonic().Words.Split(' ').Length);
            Assert.Equal(15, new Mnemonic(32 * 5).Words.Split(' ').Length);
            Assert.Equal(18, new Mnemonic(32 * 6).Words.Split(' ').Length);
            Assert.Equal(21, new Mnemonic(32 * 7).Words.Split(' ').Length);
            Assert.Equal(24, new Mnemonic(32 * 8).Words.Split(' ').Length);
        }

        [Fact]
        public void ToStringIsWords()
        {
            var m1 = new Mnemonic();
            Assert.Equal(m1.Words, m1.ToString());
        }

        [Fact]
        public void MnemonicsAreDifferent()
        {
            var m1 = new Mnemonic();
            Assert.Equal(m1.Words, m1.ToString());

            var m2 = new Mnemonic();
            Assert.Equal(m2.Words, m2.ToString());

            Assert.NotEqual(m1.ToHex(), m2.ToHex());
        }

        [Fact]
        public void MnemonicToAddress()
        {
            const string words = "dutch expire chief blue paddle flush upset health catch drill turtle slot";
            var m = ExtPrivateKey.MasterBip39(words);
            var path = new KeyPath("m/44'/0'/0'");
            var hdPrivateKey = m.Derive(path);
            var privateKey = hdPrivateKey.PrivateKey;
            var hdPublicKey = hdPrivateKey.GetExtPublicKey();
            var publicKey = hdPublicKey.PublicKey;
            var address = hdPublicKey.PublicKey.ToAddress();

            Assert.Equal("L4qUUQ5egQ31LC2gGCP4wmuauM9RbLtKhPfk5AJTFemDapDSWH1E", privateKey.ToString());
            Assert.Equal("12ApBetoCb4CH6ye4K8ro52iBpGHRkdKMr", publicKey.ToString());
            Assert.Equal("12ApBetoCb4CH6ye4K8ro52iBpGHRkdKMr", address.ToString());
        }

        [Fact]
        public void FromBase6()
        {
            var rolls1 = "10000000000000000000000000000000000000000000000002";
            var m1 = Mnemonic.FromBase6(rolls1);
            Assert.Equal("acoustic abandon abandon abandon anchor cancel pole advance naive alpha noodle slogan", m1.Words);

            var rolls2 = "20433310335200331223501035145525323501554453150402";
            var m2 = Mnemonic.FromBase6(rolls2);
            Assert.Equal("little jar barrel spatial tenant business manual cabin pig nerve trophy purity", m2.Words);

            var rolls3 = "2043331033520033122350103533025405142024330443100234401130333301433333523345145525323501554453150402";
            var m3 = Mnemonic.FromBase6(rolls3, 256);
            Assert.Equal("little jar crew spice goat sell journey behind used choose eyebrow property audit firm later blind invite fork camp shock floor reduce submit bronze", m3.Words);
        }
    }
}
