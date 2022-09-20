#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Network;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Keys
{
    public class KzAddressTests
    {
        [Theory]
        [InlineData("13k3vneZ3yvZnc9dNWYH2RJRFsagTfAERv", AddressType.PublicKeyHash, NetworkType.Main)]
        [InlineData("15vkcKf7gB23wLAnZLmbVuMiiVDc1Nm4a2", AddressType.PublicKeyHash, NetworkType.Main)]
        [InlineData("1A6ut1tWnUq1SEQLMr4ttDh24wcbJ5o9TT", AddressType.PublicKeyHash, NetworkType.Main)]
        [InlineData("1BpbpfLdY7oBS9gK7aDXgvMgr1DPvNhEB2", AddressType.PublicKeyHash, NetworkType.Main)]
        [InlineData("1Jz2yCRd5ST1p2gUqFB5wsSQfdm3jaFfg7", AddressType.PublicKeyHash, NetworkType.Main)]
        [InlineData("n28S35tqEMbt6vNad7A5K3mZ7vdn8dZ86X", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("n45x3R2w2jaSC62BMa9MeJCd3TXxgvDEmm", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("mursDVxqNQmmwWHACpM9VHwVVSfTddGsEM", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("mtX8nPZZdJ8d3QNLRJ1oJTiEi26Sj6LQXS", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("342ftSRCvFHfCeFFBuz4xwbeqnDw6BGUey", AddressType.ScriptHash, NetworkType.Main)]
        [InlineData("33vt8ViH5jsr115AGkW6cEmEz9MpvJSwDk", AddressType.ScriptHash, NetworkType.Main)]
        [InlineData("37Sp6Rv3y4kVd1nQ1JV5pfqXccHNyZm1x3", AddressType.ScriptHash, NetworkType.Main)]
        [InlineData("3QjYXhTkvuj8qPaXHTTWb5wjXhdsLAAWVy", AddressType.ScriptHash, NetworkType.Main)]
        [InlineData("mszYqVnqKoQx4jcTdJXxwKAissE3Jbrrc1", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("mrU9pEmAx26HcbKVrABvgL7AwA5fjNFoDc", AddressType.PublicKeyHash, NetworkType.Test)]
        [InlineData("mgBCJAsvzgT2qNNeXsoECg2uPKrUsZ76up", AddressType.PublicKeyHash, NetworkType.Test)]
        public void AddressToStringTest(string base58, AddressType addressType, NetworkType networkType)
        {
            var address = new Address(base58);
            Assert.Equal(base58, address.ToString());
            Assert.Equal(addressType, address.AddressType);
            Assert.Equal(networkType, address.NetworkType);
        }
        
        [Theory]
        [InlineData("15vkcKf7gB23wLAnZLmbVuMiiVDc3nq4a2")]
        [InlineData("1A6ut1tWnUq1SEQLMr4ttDh24wcbj4w2TT")]
        [InlineData("1BpbpfLdY7oBS9gK7aDXgvMgr1DpvNH3B2")]
        [InlineData("1Jz2yCRd5ST1p2gUqFB5wsSQfdmEJaffg7")]
        public void Address_BadChecksumTest(string base58)
        {
            Assert.Throws<FormatException>(() => new Address(base58));
        }
    }
}
