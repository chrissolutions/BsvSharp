﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Signatures
{
    [Flags]
    public enum SignatureHashEnum : byte
    {
        Unsupported = 0,
        All = 1,
        None = 2,
        Single = 3,
        ForkId = 0x40,
        AnyoneCanPay = 0x80,
    }
}