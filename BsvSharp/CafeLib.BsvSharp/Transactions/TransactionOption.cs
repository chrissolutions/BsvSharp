using System;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    /// When serializing the transaction to hexadecimal it is possible
    /// to selectively disable some checks. See [Transaction.serialize()]
    /// </summary>
    [Flags]
    public enum TransactionOption
    {
        ///  Disables checking if the transaction spends more Bitcoin than the sum of the input amounts
        DisableMoreOutputThanInput = 1,

        ///  Disables checking for fees that are too large
        DisableLargeFees = 2,

        ///  Disables checking if there are no outputs that are dust amounts
        DisableDustOutputs = 4,

        ///  Disables checking if all inputs are fully signed
        DisableFullySigned = 8,

        ///  Disables all checks
        DisableAll = -1
    }
}
