namespace CafeLib.BsvSharp.Units
{
    /// <summary>
    /// Used to record how a transaction output value is constrained.
    /// Supports specifying the value by any valid combination of:
    /// <list type="table">
    /// <item><term>S</term><description><see cref="Amount"/> value in satoshis.</description></item>
    /// <item><term>F</term><description>(ToValue,ToTicker) Fiat or Foreign value.</description></item>
    /// <item><term>R</term><description>BSV to Fiat or Foreign exchange Rate.</description></item>
    /// </list>
    /// <para>For user interface support, the order in which these are specified
    /// can be tracked to support automatic consistency by knowing which
    /// value to compute from the two values most recently specified.</para>
    /// <para>The word Fiat here also includes Foreign currency and non-BSV digital assets.</para>
    /// </summary>
    public enum TokenValues : byte
    {
        /// <summary>
        /// No value constraints have been set.
        /// This does not mean the value is unknown, only that set order consistency isn't used.
        /// </summary>
        None = 00,
        /// <summary>
        /// Only non-zero Satoshi value has been set. Fiat value and exchange Rate are unknown. Valid transaction output value.
        /// </summary>
        S = 01,
        /// <summary>
        /// Only non-zero Fiat value has been set. Satoshi value and exchange Rate are unknown. Invalid transaction output value.
        /// </summary>
        F = 02,
        /// <summary>
        /// Only non-zero exchange rate has been set. Satoshi value and Fiat value are unknown. Invalid transaction output value.
        /// </summary>
        R = 03,
        /// <summary>
        /// Satoshi value, then Fiat value were set. Exchange Rate was computed from them. Valid transaction output value.
        /// </summary>
        SF = 12,
        /// <summary>
        /// Satoshi value, then exchange Rate were set. Fiat value was computed from them. Valid transaction output value.
        /// </summary>
        SR = 13,
        /// <summary>
        /// Fiat value, then Satoshi value were set. Exchange Rate was computed from them. Valid transaction output value.
        /// </summary>
        FS = 21,
        /// <summary>
        /// Fiat value, then exchange Rate were set. Satoshi value was computed from them. Valid transaction output value.
        /// </summary>
        FR = 23,
        /// <summary>
        /// Exchange Rate, then Satoshi value were set. Fiat value was computed from them. Valid transaction output value.
        /// </summary>
        RS = 31,
        /// <summary>
        /// Exchange Rate, then Fiat value were set. Satoshi value was computed from them. Valid transaction output value.
        /// </summary>
        RF = 32,
        /// <summary>
        ///  A zero Satoshi value has been set. Fiat value is zero and exchange Rate is null. Valid transaction output value.
        /// </summary>
        ZS = 41,
        /// <summary>
        ///  A zero Fiat value has been set. Satoshi value is zero and exchange Rate is null. Valid transaction output value.
        /// </summary>
        ZF = 42
    }
}
