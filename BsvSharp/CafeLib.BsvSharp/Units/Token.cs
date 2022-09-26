#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.Core.Support;

namespace CafeLib.BsvSharp.Units
{
    /// <summary>
    /// In practice, the value of an amount is often required in terms of a non-Bitcoin fiat or foreign currency.
    /// There are three quantities
    /// </summary>
    public class Token<TExchangeRate> where TExchangeRate : class, IExchangeRate, new()
    {
        private decimal _amount;
        private TExchangeRate _exchangeRate;
        private decimal _tokenQuantity;

        public Token()
        {
            ValueSetOrder = TokenValues.None;
            _amount = decimal.Zero;
            _tokenQuantity = decimal.Zero;
            _exchangeRate = new TExchangeRate();
        }

        public Token(decimal amount)
            : this()
        {
            SetAmount(amount);
        }

        public Token(TExchangeRate exchangeRate, decimal tokenQuantity)
            : this(decimal.Zero)
        {
            SetExchangeRate(exchangeRate);
            SetQuantity(tokenQuantity);
        }

        public Token(decimal amount, TExchangeRate exchangeRate, decimal tokenQuantity)
            : this(amount)
        {
            SetExchangeRate(exchangeRate);
            SetQuantity(tokenQuantity);
        }

        public static implicit operator Token<TExchangeRate>(Amount value) => new(value);

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

        public decimal Amount => GetAmount();

        public TExchangeRate ExchangeRate
        {
            get => HasRate ? _exchangeRate : default; 
            set => _exchangeRate = value;
        }

        public ExchangeUnit ExchangeUnit => _exchangeRate.Foreign;

        public decimal Quantity => GetQuantity();

        /// <summary>
        /// Clear token amount.
        /// </summary>
        public void ClearAmount()
        {
            _amount = decimal.Zero;

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
        public void SetAmount(decimal amount)
        {
            _amount = amount;
            var isZero = _amount == decimal.Zero;

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
        /// <param name="exchangeUnit"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void SetQuantity(decimal tokenQuantity, ExchangeUnit? exchangeUnit = null)
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

            UpdateConstrainedValues(exchangeUnit);
        }

        /// <summary>
        /// Clear exchange rate.
        /// </summary>
        public void ClearExchangeRate()
        {
            _exchangeRate = default;

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
                    ValueSetOrder = Amount == decimal.Zero ? TokenValues.ZS : TokenValues.S;
                    break;

                case TokenValues.RF:
                case TokenValues.FR:
                    ValueSetOrder = _tokenQuantity == decimal.Zero ? TokenValues.ZF : TokenValues.F;
                    break;

                case TokenValues.ZS:
                case TokenValues.ZF:
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
        public void SetExchangeRate(TExchangeRate exchangeRate)
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
        /// Verify and return the bitcoin amount.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TokenException">token exception of token does not have an amount.</exception>
        private decimal GetAmount()
        {
            if (!HasAmount) throw new TokenException("Token does not have an amount.  Use HasAmount to verify.");
            return _amount;
        }

        /// <summary>
        /// Verify and return the bitcoin token quantity.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TokenException">token exception of token does not have a token quantity.</exception>
        private decimal GetQuantity()
        {
            if (!HasQuantity) throw new TokenException("Token does not have an quantity.  Use HasQuantity to verify.");
            return _tokenQuantity;
        }

        /// <summary>
        /// Some sequences of set values are sufficient to fully constrain the least recently set value.
        /// </summary>
        private void UpdateConstrainedValues(ExchangeUnit? exchangeUnit = null)
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
                    _exchangeRate =  Creator.CreateInstance<TExchangeRate>(exchangeUnit.GetValueOrDefault(), _amount / _tokenQuantity, DateTime.UtcNow);
                    break;

                case TokenValues.SR:
                case TokenValues.RS:
                    // Satoshis and ExchangeRate are set, check and compute Fiat (ToValue,ToTicker)
                    _tokenQuantity = _exchangeRate.ToDomesticUnits(_amount);
                    break;

                case TokenValues.FR:
                case TokenValues.RF:
                    // Fiat (ToValue,ToTicker) and ExchangeRate are set, check and compute Satoshis (Value)
                    _amount = _exchangeRate.ToForeignUnits(Math.Round(_tokenQuantity, 8, MidpointRounding.AwayFromZero)); 
                    break;

                case TokenValues.ZS:
                    _tokenQuantity = decimal.Zero;
                    break;

                case TokenValues.ZF:
                    _amount = decimal.Zero;
                    break;

                default:
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }
        }

        #endregion
    }
}
