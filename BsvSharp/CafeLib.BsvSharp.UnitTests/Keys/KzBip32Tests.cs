﻿using System.Linq;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Keys.Base58;
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
            public string HdPublicKey { get; init; }
            public string HdPrivateKey { get; init; }

            /// <summary>
            /// key path
            /// </summary>
            public string Path { get; init; }
        }

        /// <summary>
        /// Test Vector
        /// </summary>
        private class TestVector
        {
            /// <summary>
            /// Master seed as hex string.
            /// </summary>
            public string MasterSeed { get; init; }

            /// <summary>
            /// Array of Test Derivations
            /// </summary>
            public TestDerivation[] Derivations { get; init; }
        }

        private readonly TestVector[] _testVectors =
        {
            new()
            {
                MasterSeed = "000102030405060708090a0b0c0d0e0f",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        HdPublicKey = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8",
                        HdPrivateKey = "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                    },
                    new TestDerivation {
                        Path = "m/0'",
                        HdPublicKey ="xpub68Gmy5EdvgibQVfPdqkBBCHxA5htiqg55crXYuXoQRKfDBFA1WEjWgP6LHhwBZeNK1VTsfTFUHCdrfp1bgwQ9xv5ski8PX9rL2dZXvgGDnw",
                        HdPrivateKey ="xprv9uHRZZhk6KAJC1avXpDAp4MDc3sQKNxDiPvvkX8Br5ngLNv1TxvUxt4cV1rGL5hj6KCesnDYUhd7oWgT11eZG7XnxHrnYeSvkzY7d2bhkJ7",
                    },
                    new TestDerivation {
                        Path = "m/0'/1",
                        HdPublicKey ="xpub6ASuArnXKPbfEwhqN6e3mwBcDTgzisQN1wXN9BJcM47sSikHjJf3UFHKkNAWbWMiGj7Wf5uMash7SyYq527Hqck2AxYysAA7xmALppuCkwQ",
                        HdPrivateKey ="xprv9wTYmMFdV23N2TdNG573QoEsfRrWKQgWeibmLntzniatZvR9BmLnvSxqu53Kw1UmYPxLgboyZQaXwTCg8MSY3H2EU4pWcQDnRnrVA1xe8fs",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'",
                        HdPublicKey ="xpub6D4BDPcP2GT577Vvch3R8wDkScZWzQzMMUm3PWbmWvVJrZwQY4VUNgqFJPMM3No2dFDFGTsxxpG5uJh7n7epu4trkrX7x7DogT5Uv6fcLW5",
                        HdPrivateKey ="xprv9z4pot5VBttmtdRTWfWQmoH1taj2axGVzFqSb8C9xaxKymcFzXBDptWmT7FwuEzG3ryjH4ktypQSAewRiNMjANTtpgP4mLTj34bhnZX7UiM",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'/2",
                        HdPublicKey ="xpub6FHa3pjLCk84BayeJxFW2SP4XRrFd1JYnxeLeU8EqN3vDfZmbqBqaGJAyiLjTAwm6ZLRQUMv1ZACTj37sR62cfN7fe5JnJ7dh8zL4fiyLHV",
                        HdPrivateKey ="xprvA2JDeKCSNNZky6uBCviVfJSKyQ1mDYahRjijr5idH2WwLsEd4Hsb2Tyh8RfQMuPh7f7RtyzTtdrbdqqsunu5Mm3wDvUAKRHSC34sJ7in334",
                    },
                    new TestDerivation {
                        Path = "m/0'/1/2'/2/1000000000",
                        HdPublicKey ="xpub6H1LXWLaKsWFhvm6RVpEL9P4KfRZSW7abD2ttkWP3SSQvnyA8FSVqNTEcYFgJS2UaFcxupHiYkro49S8yGasTvXEYBVPamhGW6cFJodrTHy",
                        HdPrivateKey ="xprvA41z7zogVVwxVSgdKUHDy1SKmdb533PjDz7J6N6mV6uS3ze1ai8FHa8kmHScGpWmj4WggLyQjgPie1rFSruoUihUZREPSL39UNdE3BBDu76",
                    },
                }
            },
            new()
            {
                MasterSeed = "fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        HdPublicKey = "xpub661MyMwAqRbcFW31YEwpkMuc5THy2PSt5bDMsktWQcFF8syAmRUapSCGu8ED9W6oDMSgv6Zz8idoc4a6mr8BDzTJY47LJhkJ8UB7WEGuduB",
                        HdPrivateKey = "xprv9s21ZrQH143K31xYSDQpPDxsXRTUcvj2iNHm5NUtrGiGG5e2DtALGdso3pGz6ssrdK4PFmM8NSpSBHNqPqm55Qn3LqFtT2emdEXVYsCzC2U",
                    },
                    new TestDerivation {
                        Path = "m/0",
                        HdPublicKey = "xpub69H7F5d8KSRgmmdJg2KhpAK8SR3DjMwAdkxj3ZuxV27CprR9LgpeyGmXUbC6wb7ERfvrnKZjXoUmmDznezpbZb7ap6r1D3tgFxHmwMkQTPH",
                        HdPrivateKey = "xprv9vHkqa6EV4sPZHYqZznhT2NPtPCjKuDKGY38FBWLvgaDx45zo9WQRUT3dKYnjwih2yJD9mkrocEZXo1ex8G81dwSM1fwqWpWkeS3v86pgKt",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H",
                        HdPublicKey = "xpub6ASAVgeehLbnwdqV6UKMHVzgqAG8Gr6riv3Fxxpj8ksbH9ebxaEyBLZ85ySDhKiLDBrQSARLq1uNRts8RuJiHjaDMBU4Zn9h8LZNnBC5y4a",
                        HdPrivateKey = "xprv9wSp6B7kry3Vj9m1zSnLvN3xH8RdsPP1Mh7fAaR7aRLcQMKTR2vidYEeEg2mUCTAwCd6vnxVrcjfy2kRgVsFawNzmjuHc2YmYRmagcEPdU9",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1",
                        HdPublicKey = "xpub6DF8uhdarytz3FWdA8TvFSvvAh8dP3283MY7p2V4SeE2wyWmG5mg5EwVvmdMVCQcoNJxGoWaU9DCWh89LojfZ537wTfunKau47EL2dhHKon",
                        HdPrivateKey = "xprv9zFnWC6h2cLgpmSA46vutJzBcfJ8yaJGg8cX1e5StJh45BBciYTRXSd25UEPVuesF9yog62tGAQtHjXajPPdbRCHuWS6T8XA2ECKADdw4Ef",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1/2147483646H",
                        HdPublicKey = "xpub6ERApfZwUNrhLCkDtcHTcxd75RbzS1ed54G1LkBUHQVHQKqhMkhgbmJbZRkrgZw4koxb5JaHWkY4ALHY2grBGRjaDMzQLcgJvLJuZZvRcEL",
                        HdPrivateKey = "xprvA1RpRA33e1JQ7ifknakTFpgNXPmW2YvmhqLQYMmrj4xJXXWYpDPS3xz7iAxn8L39njGVyuoseXzU6rcxFLJ8HFsTjSyQbLYnMpCqE2VbFWc",
                        },
                    new TestDerivation {
                        Path = "m/0/2147483647H/1/2147483646H/2",
                        HdPublicKey = "xpub6FnCn6nSzZAw5Tw7cgR9bi15UV96gLZhjDstkXXxvCLsUXBGXPdSnLFbdpq8p9HmGsApME5hQTZ3emM2rnY5agb9rXpVGyy3bdW6EEgAtqt",
                        HdPrivateKey = "xprvA2nrNbFZABcdryreWet9Ea4LvTJcGsqrMzxHx98MMrotbir7yrKCEXw7nadnHM8Dq38EGfSh6dqA9QWTyefMLEcBYJUuekgW4BYPJcr9E7j",
                        },
                }
            },
            new()
            {
                MasterSeed = "4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be",
                Derivations = new [] {
                    new TestDerivation {
                        Path = "m",
                        HdPublicKey = "xpub661MyMwAqRbcEZVB4dScxMAdx6d4nFc9nvyvH3v4gJL378CSRZiYmhRoP7mBy6gSPSCYk6SzXPTf3ND1cZAceL7SfJ1Z3GC8vBgp2epUt13",
                        HdPrivateKey = "xprv9s21ZrQH143K25QhxbucbDDuQ4naNntJRi4KUfWT7xo4EKsHt2QJDu7KXp1A3u7Bi1j8ph3EGsZ9Xvz9dGuVrtHHs7pXeTzjuxBrCmmhgC6",
                        },
                    new TestDerivation {
                        Path = "m/0H",
                        HdPublicKey = "xpub68NZiKmJWnxxS6aaHmn81bvJeTESw724CRDs6HbuccFQN9Ku14VQrADWgqbhhTHBaohPX4CjNLf9fq9MYo6oDaPPLPxSb7gwQN3ih19Zm4Y",
                        HdPrivateKey = "xprv9uPDJpEQgRQfDcW7BkF7eTya6RPxXeJCqCJGHuCJ4GiRVLzkTXBAJMu2qaMWPrS7AANYqdq6vcBcBUdJCVVFceUvJFjaPdGZ2y9WACViL4L",
                        },
                }
            },
        };

        [Fact]
        public void TestCases()
        {
            foreach (var tv in _testVectors) {

                var seed = tv.MasterSeed.HexToBytes();
                var m = HdPrivateKey.MasterBip32(seed);

                foreach (var d in tv.Derivations)
                {
                    // Test key path.
                    var path = new KeyPath(d.Path);
                    var hdPriv = m.Derive(path);
                    var hdPriv2 = m.Derive(d.Path);
                    Assert.Equal(hdPriv, hdPriv2);

                    // Test hierarchical deterministic keys.
                    var hdPub = hdPriv.GetHdPublicKey();
                    var strPriv = hdPriv.ToString();
                    var strPub = hdPub.ToString();
                    Assert.Equal(d.HdPrivateKey, strPriv);
                    Assert.Equal(d.HdPublicKey, strPub);

                    var data = new byte[HdKey.Bip32KeySize];
                    hdPriv.Encode(data);
                    hdPriv2 = HdPrivateKey.FromKey(data);
                    Assert.Equal(hdPriv, hdPriv2);

                    hdPub.Encode(data);
                    var hdPub2 = HdPublicKey.FromKey(data);
                    Assert.Equal(hdPub, hdPub2);

                    // Test private key
                    var b58Key = new Base58HdPrivateKey(hdPriv);
                    Assert.Equal(d.HdPrivateKey, b58Key.ToString());
                    Assert.True(new Base58HdPrivateKey(d.HdPrivateKey) == b58Key);

                    var b58KeyDecodeCheck = new Base58HdPrivateKey(d.HdPrivateKey);
                    var checkKey = b58KeyDecodeCheck.GetKey();

                    // ensure a base58 decoded pubkey also matches
                    Assert.Equal(checkKey, hdPriv);

                    // Skip if private key is not hardened or if the path's parent is null.
                    if (hdPriv.Hardened != false || path.Parent == null) continue;
                    
                    // Compare with public derivation
                    var pubkeyNew2 = m.Derive(path.Parent).GetHdPublicKey().Derive((int)path.Last());
                    Assert.True(pubkeyNew2 != null);
                    Assert.Equal(hdPub, pubkeyNew2);
                }
            }
        }
    }
}
