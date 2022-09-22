#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Units
{
    /// <summary>
    /// In practice, the value of an amount is often required in terms of a non-Bitcoin fiat or foreign currency.
    /// There are three quantities
    /// </summary>
    public class Token
    {
        private decimal _tokenQuantity;
        private BsvExchangeRate _exchangeRate;

        public Token()
        {
            ValueSetOrder = TokenValues.None;
            Amount = Amount.Zero;
            _tokenQuantity = decimal.Zero;
            _exchangeRate = BsvExchangeRate.Default;
        }

        public Token(Amount amount)
            : this()
        {
            SetAmount(amount);
        }

        public Token(BsvExchangeRate exchangeRate, decimal tokenQuantity)
            : this(Amount.Zero)
        {
            SetExchangeRate(exchangeRate);
            SetQuantity(tokenQuantity);
        }

        public Token(Amount amount, BsvExchangeRate exchangeRate, decimal tokenQuantity)
            : this()
        {
            SetAmount(amount);
            SetExchangeRate(exchangeRate);
            SetQuantity(tokenQuantity);
        }


        public static implicit operator Token(Amount value) => new(value);

        public bool HasAll => ValueSetOrder > TokenValues.R;
        public bool HasAmount => ValueSetOrder is > TokenValues.R or TokenValues.S;
        public bool HasRate => ValueSetOrder is > TokenValues.R or TokenValues.R;
        public bool HasQuantity => ValueSetOrder is > TokenValues.R or TokenValues.F;

        public bool HasComputedAmount => ValueSetOrder is TokenValues.FR or TokenValues.RF or TokenValues.ZF;
        public bool HasComputedQuantity => ValueSetOrder is TokenValues.RS or TokenValues.SR or TokenValues.ZS;
        public bool HasComputedRate => ValueSetOrder is TokenValues.FS or TokenValues.SF;

        public bool HasSetAmount => ValueSetOrder is TokenValues.S or TokenValues.SR or TokenValues.SF or TokenValues.RS or TokenValues.FS or TokenValues.ZS;
        public bool HasSetQuantity => ValueSetOrder is TokenValues.F or TokenValues.FR or TokenValues.FS or TokenValues.RF or TokenValues.SF or TokenValues.ZF;
        public bool HasSetRate => ValueSetOrder is TokenValues.R or TokenValues.RS or TokenValues.RF or TokenValues.SR or TokenValues.FR;

        public TokenValues ValueSetOrder { get; set; }

        public Amount Amount { get; private set; }

        public long? Satoshis => HasAmount ? Amount.Satoshis : null;

        public BsvExchangeRate Rate
        {
            get => HasRate ? _exchangeRate : null; 
            set => _exchangeRate = value;
        }

        public ExchangeUnit ExchangeUnit => _exchangeRate.Foreign;

        public decimal? TokenQuantity
        {
            get => HasQuantity ? _tokenQuantity : null; 
            set => _tokenQuantity = value ?? decimal.Zero;
        }

        /// <summary>
        /// Clear token amount.
        /// </summary>
        public void ClearAmount()
        {
            Amount = Amount.Zero;

            // Update _SetOrder to reflect the loss of Amount Satoshis.
            switch (ValueSetOrder)
            {
                case TokenValues.None:
                case TokenValues.S:
                case TokenValues.ZS:
                    ValueSetOrder = TokenValues.None;
                    break;

                case TokenValues.F:
                case TokenValues.R:
                case TokenValues.RF:
                case TokenValues.FR:
                case TokenValues.ZF:
                    break;

                case TokenValues.FS:
                case TokenValues.SF:
                    ValueSetOrder = TokenValues.F;
                    break;

                case TokenValues.SR:
                case TokenValues.RS:
                    ValueSetOrder = TokenValues.R;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Set a specific bitcoin amount.
        /// </summary>
        /// <param name="amount">bitcoin amount</param>
        public void SetAmount(Amount amount)
        {
            Amount = amount;
            var isZero = Amount == Amount.Zero;

            switch (ValueSetOrder)
            {
                case TokenValues.None:
                case TokenValues.S:
                case TokenValues.ZS:
                case TokenValues.ZF:
                    ValueSetOrder = isZero ? TokenValues.ZS : TokenValues.S;
                    break;

                case TokenValues.F:
                case TokenValues.SF:
                case TokenValues.RF:
                case TokenValues.FS:
                    ValueSetOrder = isZero ? TokenValues.ZS : TokenValues.FS;
                    break;

                case TokenValues.R:
                case TokenValues.SR:
                case TokenValues.FR:
                case TokenValues.RS:
                    ValueSetOrder = TokenValues.RS;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Clears a previously set token quantity.
        /// </summary>
        public void ClearQuantity()
        {
            // Retain the ToTicker as the best default even when clearing value.
            _tokenQuantity = decimal.Zero;

            // Update _SetOrder to reflect the loss of Fiat/Foreign value.
            switch (ValueSetOrder)
            {
                case TokenValues.None:
                case TokenValues.F:
                case TokenValues.ZF:
                    ValueSetOrder = TokenValues.None;
                    break;

                case TokenValues.S:
                case TokenValues.R:
                case TokenValues.RS:
                case TokenValues.SR:
                case TokenValues.ZS:
                    break;

                case TokenValues.FS:
                case TokenValues.SF:
                    ValueSetOrder = TokenValues.S;
                    break;

                case TokenValues.FR:
                case TokenValues.RF:
                    ValueSetOrder = TokenValues.R;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Set token quantity.
        /// </summary>
        /// <param name="tokenQuantity"></param>
        public void SetQuantity(decimal tokenQuantity)
        {
            _tokenQuantity = tokenQuantity;

            // Update _SetOrder to reflect a new Fiat/Foreign value.
            var isZero = _tokenQuantity == decimal.Zero;

            ValueSetOrder = ValueSetOrder switch
            {
                TokenValues.None or TokenValues.F or TokenValues.ZS or TokenValues.ZF => isZero ? TokenValues.ZF : TokenValues.F,
                TokenValues.S or TokenValues.FS or TokenValues.RS or TokenValues.SF => isZero ? TokenValues.ZF : TokenValues.SF,
                TokenValues.R or TokenValues.SR or TokenValues.FR or TokenValues.RF => TokenValues.RF,
                _ => throw new NotSupportedException(nameof(ValueSetOrder)),
            };

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Clear exchange rate.
        /// </summary>
        public void ClearExchangeRate()
        {
            _exchangeRate = null;

            // Update _SetOrder to reflect the loss of exchange Rate value.
            switch (ValueSetOrder)
            {
                case TokenValues.None:
                case TokenValues.R:
                    ValueSetOrder = TokenValues.None;
                    break;

                case TokenValues.S:
                case TokenValues.F:
                case TokenValues.SF:
                case TokenValues.FS:
                    break;

                case TokenValues.SR:
                case TokenValues.RS:
                    ValueSetOrder = Amount == Amount.Zero ? TokenValues.ZS : TokenValues.S;
                    break;

                case TokenValues.RF:
                case TokenValues.FR:
                    ValueSetOrder = _tokenQuantity == decimal.Zero ? TokenValues.ZF : TokenValues.F;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Set a specific exchange rate.
        /// A zero exchange rate is treated as a null value, clearing exchange rate constraints.
        /// </summary>
        /// <param name="exchangeRate"></param>
        public void SetExchangeRate(BsvExchangeRate exchangeRate)
        {
            if (exchangeRate == null || exchangeRate.Rate == decimal.Zero)
            {
                ClearExchangeRate();
                return;
            }

            _exchangeRate = exchangeRate;

            // Update _SetOrder to reflect a new exchange Rate value.
            ValueSetOrder = ValueSetOrder switch
            {
                TokenValues.None or TokenValues.R => TokenValues.R,
                TokenValues.S or TokenValues.FS or TokenValues.RS or TokenValues.SR or TokenValues.ZS => TokenValues.SR,
                TokenValues.F or TokenValues.SF or TokenValues.RF or TokenValues.FR or TokenValues.ZF => TokenValues.FR,
                _ => throw new NotSupportedException(nameof(ValueSetOrder)),
            };

            UpdateConstrainedValues();
        }

        #region Helpers

        /// <summary>
        /// Some sequences of set values are sufficient to fully constrain the least recently set value.
        /// </summary>
        private void UpdateConstrainedValues()
        {
            switch (ValueSetOrder)
            {
                case TokenValues.None:
                case TokenValues.S:
                case TokenValues.F:
                case TokenValues.R:
                    // Nothing to update if less than two values have been set.
                    break;

                case TokenValues.SF:
                case TokenValues.FS:
                    // Satoshis (Value) and Fiat (ToValue,ToTicker) are set, check and compute ExchangeRate
                    _exchangeRate = new BsvExchangeRate(ExchangeUnit, _tokenQuantity / Amount.ToBitcoin());
                    break;

                case TokenValues.SR:
                case TokenValues.RS:
                    // Satoshis and ExchangeRate are set, check and compute Fiat (ToValue,ToTicker)
                    _tokenQuantity = _exchangeRate.ToForeignUnits(Amount);
                    break;

                case TokenValues.FR:
                case TokenValues.RF:
                    // Fiat (ToValue,ToTicker) and ExchangeRate are set, check and compute Satoshis (Value)
                    Amount = new Amount(_exchangeRate.ToForeignUnits(Math.Round(_tokenQuantity, 8, MidpointRounding.AwayFromZero)), BitcoinUnit.Bitcoin); 
                    break;

                case TokenValues.ZS:
                    _tokenQuantity = decimal.Zero;
                    break;

                case TokenValues.ZF:
                    Amount = Amount.Zero;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }
        }

        #endregion
    }
}
