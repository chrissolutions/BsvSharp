#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Units
{
    /// <summary>
    /// How many satoshis to each unit.
    /// </summary>
    public enum BitcoinUnit : long
    {
        Bitcoin = 100_000_000,
        MilliBitcoin = 100_000,
        Bit = 100,
        Satoshi = 1
    }
}
