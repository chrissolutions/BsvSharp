#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class KzBip32Tests
    {
        /// <summary>
        /// Test Derivation
        /// </summary>
        private class TestDerivation
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }

            /// <summary>
            /// key path
            /// </summary>
            public string Path { get; set; }
        }

        /// <summary>
        /// Test Vector
        /// </summary>
        private class TestVector
        {
            /// <summary>
            /// Master seed as hex string.
            /// </summary>
            public string MasterSeed { get; set; }

            /// <summary>
            /// Array of Test Derivations
            /// </summary>
            public TestDerivation[] Derivations { get; set; }
        }

        private readonly TestVector[] _testVectors =
        {
            new TestVector {
                MasterSeed = "000102030405060708090a0b0c0d0e0f",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        PublicKey = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8",
                        PrivateKey = "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                    },
                    new TestDerivation {
                        Path = "m/0'",
                        PublicKey ="xpub68Gmy5EdvgibQVfPdqkBBCHxA5htiqg55crXYuXoQRKfDBFA1WEjWgP6LHhwBZeNK1VTsfTFUHCdrfp1bgwQ9xv5ski8PX9rL2dZXvgGDnw",
                        PrivateKey ="xprv9uHRZZhk6KAJC1avXpDAp4MDc3sQKNxDiPvvkX8Br5ngLNv1TxvUxt4cV1rGL5hj6KCesnDYUhd7oWgT11eZG7XnxHrnYeSvkzY7d2bhkJ7",
                    },
                    new TestDerivation {
                        Path = "m/0'/1",
                        PublicKey ="xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ",
                        PrivateKey ="xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'",
                        PublicKey ="xpub6D4BDPcP2GT577Vvch3R8wDkScZWzQzMMUm3PWbmWvVJrZwQY4VUNgqFJPMM3No2dFDFGTsxxpG5uJh7n7epu4trkrX7x7DogT5Uv6fcLW5",
                        PrivateKey ="xprv9z4pot5VBttmtdRTWfWQmoH1taj2axGVzFqSb8C9xaxKymcFzXBDptWmT7FwuEzG3ryjH4ktypQSAewRiNMjANTtpgP4mLTj34bhnZX7UiM",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'/2",
                        PublicKey ="xpub6FHa3pjLCk84BayeJxFW2SP4XRrFd1JYnxeLeU8EqN3vDfZmbqBqaGJAyiLjTAwm6ZLRQUMv1ZACTj37sR62cfN7fe5JnJ7dh8zL4fiyLHV",
                        PrivateKey ="xprvA2JDeKCSNNZky6uBCviVfJSKyQ1mDYahRjijr5idH2WwLsEd4Hsb2Tyh8RfQMuPh7f7RtyzTtdrbdqqsunu5Mm3wDvUAKRHSC34sJ7in334",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'/2/1000000000",
                        PublicKey ="xpub6H1LXWLaKsWFhvm6RVpEL9P4KfRZSW7abD2ttkWP3SSQvnyA8FSVqNTEcYFgJS2UaFcxupHiYkro49S8yGasTvXEYBVPamhGW6cFJodrTHy",
                        PrivateKey ="xprvA41z7zogVVwxVSgdKUHDy1SKmdb533PjDz7J6N6mV6uS3ze1ai8FHa8kmHScGpWmj4WggLyQjgPie1rFSruoUihUZREPSL39UNdE3BBDu76",
                    },
                }
            },
            new TestVector {
                MasterSeed = "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        PublicKey = "xpub661MyMwAqRbcFW31YEwpkMuc5THy2PSt5bDMsktWQcFF8syAmRUapSCGu8ED9W6oDMSgv6Zz8idoc4a6mr8BDzTJY47LJhkJ8UB7WEGuduB",
                        PrivateKey = "xprv9s21ZrQH143K31xYSDQpPDxsXRTUcvj2iNHm5NUtrGiGG5e2DtALGdso3pGz6ssrdK4PFmM8NSpSBHNqPqm55Qn3LqFtT2emdEXVYsCzC2U",
                    },
                    new TestDerivation {
                        Path = "m/0",
                        PublicKey = "xpub69H7F5d8KSRgmmdJg2KhpAK8SR3DjMwAdkxj3ZuxV27CprR9LgpeyGmXUbC6wb7ERfvrnKZjXoUmmDznezpbZb7ap6r1D3tgFxHmwMkQTPH",
                        PrivateKey = "xprv9vHkqa6EV4sPZHYqZznhT2NPtPCjKuDKGY38FBWLvgaDx45zo9WQRUT3dKYnjwih2yJD9mkrocEZXo1ex8G81dwSM1fwqWpWkeS3v86pgKt",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H",
                        PublicKey = "xpub6ASAVgeehLbnwdqV6UKMHVzgqAG8Gr6riv3Fxxpj8ksbH9ebxaEyBLZ85ySDhKiLDBrQSARLq1uNRts8RuJiHjaDMBU4Zn9h8LZNnBC5y4a",
                        PrivateKey = "xprv9wSp6B7kry3Vj9m1zSnLvN3xH8RdsPP1Mh7fAaR7aRLcQMKTR2vidYEeEg2mUCTAwCd6vnxVrcjfy2kRgVsFawNzmjuHc2YmYRmagcEPdU9",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1",
                        PublicKey = "xpub6DF8uhdarytz3FWdA8TvFSvvAh8dP3283MY7p2V4SeE2wyWmG5mg5EwVvmdMVCQcoNJxGoWaU9DCWh89LojfZ537wTfunKau47EL2dhHKon",
                        PrivateKey = "xprv9zFnWC6h2cLgpmSA46vutJzBcfJ8yaJGg8cX1e5StJh45BBciYTRXSd25UEPVuesF9yog62tGAQtHjXajPPdbRCHuWS6T8XA2ECKADdw4Ef",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1/2147483646H",
                        PublicKey = "xpub6ERApfZwUNrhLCkDtcHTcxd75RbzS1ed54G1LkBUHQVHQKqhMkhgbmJbZRkrgZw4koxb5JaHWkY4ALHY2grBGRjaDMzQLcgJvLJuZZvRcEL",
                        PrivateKey = "xprvA1RpRA33e1JQ7ifknakTFpgNXPmW2YvmhqLQYMmrj4xJXXWYpDPS3xz7iAxn8L39njGVyuoseXzU6rcxFLJ8HFsTjSyQbLYnMpCqE2VbFWc",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1/2147483646H/2",
                        PublicKey = "xpub6FnCn6nSzZAw5Tw7cgR9bi15UV96gLZhjDstkXXxvCLsUXBGXPdSnLFbdpq8p9HmGsApME5hQTZ3emM2rnY5agb9rXpVGyy3bdW6EEgAtqt",
                        PrivateKey = "xprvA2nrNbFZABcdryreWet9Ea4LvTJcGsqrMzxHx98MMrotbir7yrKCEXw7nadnHM8Dq38EGfSh6dqA9QWTyefMLEcBYJUuekgW4BYPJcr9E7j",
                        },
                }
            },
            new TestVector {
                MasterSeed = "4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        PublicKey = "xpub661MyMwAqRbcEZVB4dScxMAdx6d4nFc9nvyvH3v4gJL378CSRZiYmhRoP7mBy6gSPSCYk6SzXPTf3ND1cZAceL7SfJ1Z3GC8vBgp2epUt13",
                        PrivateKey = "xprv9s21ZrQH143K25QhxbucbDDuQ4naNntJRi4KUfWT7xo4EKsHt2QJDu7KXp1A3u7Bi1j8ph3EGsZ9Xvz9dGuVrtHHs7pXeTzjuxBrCmmhgC6",
                        },
                    new TestDerivation {
                        Path = "m/0H",
                        PublicKey = "xpub68NZiKmJWnxxS6aaHmn81bvJeTESw724CRDs6HbuccFQN9Ku14VQrADWgqbhhTHBaohPX4CjNLf9fq9MYo6oDaPPLPxSb7gwQN3ih19Zm4Y",
                        PrivateKey = "xprv9uPDJpEQgRQfDcW7BkF7eTya6RPxXeJCqCJGHuCJ4GiRVLzkTXBAJMu2qaMWPrS7AANYqdq6vcBcBUdJCVVFceUvJFjaPdGZ2y9WACViL4L",
                        },
                }
            },
        };

        [Fact]
        public void TestCases()
        {
            foreach (var tv in _testVectors) {

                var seed = tv.MasterSeed.HexToBytes();
                var m = ExtPrivateKey.MasterBip32(seed);

                foreach (var d in tv.Derivations)
                {
                    var path = new KeyPath(d.Path);
                    var priv = m.Derive(path);
                    var pub = priv.GetExtPublicKey();

                    var strPriv = priv.ToString();
                    var strPub = pub.ToString();
                    Assert.Equal(d.PrivateKey, strPriv);
                    Assert.Equal(d.PublicKey, strPub);

                    var data = new byte[ExtKey.Bip32KeySize];
                    priv.Encode(data);
                    pub.Encode(data);

                    // Test private key
                    var b58Key = new Base58ExtPrivateKey();
                    b58Key.SetKey(priv);
                    Assert.Equal(d.PrivateKey, b58Key.ToString());

                    var b58KeyDecodeCheck = new Base58ExtPrivateKey(d.PrivateKey);
                    var checkKey = b58KeyDecodeCheck.GetKey();
                    // ensure a base58 decoded pubkey also matches
                    Assert.Equal(checkKey, priv);

                    if (priv.Hardened == false && path.Parent != null)
                    {
                        // Compare with public derivation
                        var pubkeyNew2 = m.Derive(path.Parent).GetExtPublicKey().Derive((int)path.Last());
                        Assert.True(pubkeyNew2 != null);
                        Assert.Equal(pub, pubkeyNew2);
                    }
                }
            }
        }
    }
}
