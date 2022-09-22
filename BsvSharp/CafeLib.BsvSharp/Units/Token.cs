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
        private Amount _amount;
        private decimal _fiatValue;
        private ExchangeRate _exchangeRate;

        public Token()
        {
            ValueSetOrder = TokenValues.None;
            _amount = Units.Amount.Zero;
            _fiatValue = decimal.Zero;
            _exchangeRate = ExchangeRate.Default;
        }

        public Token(Amount amount)
            : this()
        {
            SetAmount(amount);
        }

        public Token(ExchangeRate rate, decimal fiatValue)
            : this()
        {
            SetRate(rate);
            SetFiatValue(fiatValue);
        }

        public Token(Amount amount, ExchangeUnit fiatTicker, decimal fiatValue)
            : this()
        {
            SetAmount(amount);
            SetFiatValue(fiatValue);
        }


        public static implicit operator Token(Amount value) => new(value);

        public bool HasAll => ValueSetOrder > TokenValues.R;
        public bool HasAmount => ValueSetOrder > TokenValues.R || ValueSetOrder == TokenValues.S;
        public bool HasRate => ValueSetOrder > TokenValues.R || ValueSetOrder == TokenValues.R;
        public bool HasFiat => ValueSetOrder > TokenValues.R || ValueSetOrder == TokenValues.F;

        public bool HasComputedAmount => ValueSetOrder == TokenValues.FR || ValueSetOrder == TokenValues.RF || ValueSetOrder == TokenValues.ZF;
        public bool HasComputedFiat => ValueSetOrder == TokenValues.RS || ValueSetOrder == TokenValues.SR || ValueSetOrder == TokenValues.ZS;
        public bool HasComputedRate => ValueSetOrder == TokenValues.FS || ValueSetOrder == TokenValues.SF;

        public bool HasSetAmount => ValueSetOrder == TokenValues.S || ValueSetOrder == TokenValues.SR || ValueSetOrder == TokenValues.SF || ValueSetOrder == TokenValues.RS || ValueSetOrder == TokenValues.FS || ValueSetOrder == TokenValues.ZS;
        public bool HasSetFiat => ValueSetOrder == TokenValues.F || ValueSetOrder == TokenValues.FR || ValueSetOrder == TokenValues.FS || ValueSetOrder == TokenValues.RF || ValueSetOrder == TokenValues.SF || ValueSetOrder == TokenValues.ZF;
        public bool HasSetRate => ValueSetOrder == TokenValues.R || ValueSetOrder == TokenValues.RS || ValueSetOrder == TokenValues.RF || ValueSetOrder == TokenValues.SR || ValueSetOrder == TokenValues.FR;

        public TokenValues ValueSetOrder { get; set; }

        public Amount? Amount
        {
            get => HasAmount ? _amount : null;
            set => _amount = value ?? Units.Amount.Zero;
        }

        public long? Satoshis => HasAmount ? _amount.Satoshis : null;

        public ExchangeRate Rate
        {
            get => HasRate ? _exchangeRate : null; 
            set => _exchangeRate = value;
        }

        public ExchangeUnit ExchangeUnit => _exchangeRate.Foreign;

        public decimal? FiatValue
        {
            get => HasFiat ? _fiatValue : null; 
            set => _fiatValue = value ?? decimal.Zero;
        }

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
                    _exchangeRate = new ExchangeRate {
                        Domestic = ExchangeUnit.BSV,
                        Foreign = ExchangeUnit,
                        Timestamp = DateTime.UtcNow,
                        Rate = _fiatValue / _amount.ToBitcoin()
                    };
                    break;

                case TokenValues.SR:
                case TokenValues.RS:
                    // Satoshis and ExchangeRate are set, check and compute Fiat (ToValue,ToTicker)
                    _fiatValue = _exchangeRate.ToForeignUnits(_amount);
                    break;

                case TokenValues.FR:
                case TokenValues.RF:
                    // Fiat (ToValue,ToTicker) and ExchangeRate are set, check and compute Satoshis (Value)
                    _amount = new Amount(Math.Round(_exchangeRate.ToDomesticUnits(_fiatValue), 8, MidpointRounding.AwayFromZero), BitcoinUnit.Bitcoin);
                    break;

                case TokenValues.ZS:
                    _fiatValue = decimal.Zero;
                    break;

                case TokenValues.ZF:
                    _amount = Units.Amount.Zero;
                    break;

                default: 
                    throw new NotSupportedException(nameof(ValueSetOrder));
            }
        }

        /// <summary>
        /// Set a specific bitcoin amount.
        /// The amount must be in the range <see cref="BsvSharp.Units.Amount.MinValue"/> to <see cref="BsvSharp.Units.Amount.MaxValue"/>.
        /// If the amount is zero, it constrains the Fiat value to be zero as well, but leaves Rate as it was.
        /// </summary>
        /// <param name="amount"></param>
        public void SetAmount(Amount? amount)
        {
            if (amount.HasValue) 
            {
                if (amount > BsvSharp.Units.Amount.MaxValue)
                    throw new ArgumentException("Maximum value exceeded.");

                if (amount < BsvSharp.Units.Amount.MinValue)
                    throw new ArgumentException("Minimum value exceeded.");

                _amount = amount.Value;
                // Update _SetOrder to reflect a new Satoshi value.
                var isZero = _amount == BsvSharp.Units.Amount.Zero;

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
            } 
            else 
            {
                _amount = BsvSharp.Units.Amount.Zero;

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
                        // No change.
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
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Set a specific fiat or foreign currency value, or clears a previously set value.
        /// If the fiat value is zero, it constrains the bitcoin amount to be zero as well, but leaves Rate as it was.
        /// If the fiat value is null, clears fiat constraints on value.
        /// </summary>
        /// <param name="fiatValue"></param>
        public void SetFiatValue(decimal? fiatValue)
        {
            if (fiatValue.HasValue)
            {
                _fiatValue = fiatValue.Value;

                // Update _SetOrder to reflect a new Fiat/Foreign value.
                var isZero = _fiatValue == decimal.Zero;

                switch (ValueSetOrder) 
                {
                    case TokenValues.None:
                    case TokenValues.F:
                    case TokenValues.ZS:
                    case TokenValues.ZF:
                        ValueSetOrder = isZero ? TokenValues.ZF : TokenValues.F;
                        break;

                    case TokenValues.S:
                    case TokenValues.FS:
                    case TokenValues.RS:
                    case TokenValues.SF:
                        ValueSetOrder = isZero ? TokenValues.ZF : TokenValues.SF;
                        break;

                    case TokenValues.R:
                    case TokenValues.SR:
                    case TokenValues.FR:
                    case TokenValues.RF:
                        ValueSetOrder = TokenValues.RF;
                        break;

                    default:
                        throw new NotSupportedException(nameof(ValueSetOrder));
                }
            } 
            else
            {
                // Retain the ToTicker as the best default even when clearing value.
                _fiatValue = decimal.Zero;

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
            }

            UpdateConstrainedValues();
        }

        /// <summary>
        /// Set a specific exchange rate, or clears a previously set value.
        /// A zero exchange rate is treated as a null value, clearing exchange rate constraints.
        /// </summary>
        /// <param name="rate"></param>
        public void SetRate(ExchangeRate rate)
        {
            //rate.CheckOfTickerIsBSV();
            _exchangeRate = rate.Rate != decimal.Zero ? rate : null;

            if (_exchangeRate != null) 
            {
                // Update _SetOrder to reflect a new exchange Rate value.
                switch (ValueSetOrder)
                {
                    case TokenValues.None:
                    case TokenValues.R:
                        ValueSetOrder = TokenValues.R;
                        break;

                    case TokenValues.S:
                    case TokenValues.FS:
                    case TokenValues.RS:
                    case TokenValues.SR:
                    case TokenValues.ZS:
                        ValueSetOrder = TokenValues.SR;
                        break;

                    case TokenValues.F:
                    case TokenValues.SF:
                    case TokenValues.RF:
                    case TokenValues.FR:
                    case TokenValues.ZF:
                        ValueSetOrder = TokenValues.FR;
                        break;

                    default:
                        throw new NotSupportedException(nameof(ValueSetOrder));
                }
            } 
            else 
            {
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
                        // No change.
                        break;

                    case TokenValues.SR:
                    case TokenValues.RS:
                        ValueSetOrder = _amount == BsvSharp.Units.Amount.Zero ? TokenValues.ZS : TokenValues.S;
                        break;

                    case TokenValues.RF:
                    case TokenValues.FR:
                        ValueSetOrder = _fiatValue == decimal.Zero ? TokenValues.ZF : TokenValues.F;
                        break;

                    default:
                        throw new NotSupportedException(nameof(ValueSetOrder));
                }
            }

            UpdateConstrainedValues();
        }
    }
}
