#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;

namespace CafeLib.BsvSharp.Api.UnitTests
{

    public class KzElectrumSv
    {
        public static ExtPrivateKey GetMasterPrivateKey(string passphrase, string password = null) =>
            ExtPrivateKey.MasterBip39(passphrase, password, passwordPrefix: "electrum");
    }
}