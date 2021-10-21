using System;
using System.Buffers;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Services;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Transactions
{
    public class Transaction : ITxId, IDataSerializer
    {
        private ScriptBuilder _changeScriptBuilder;
        private bool _hasChangeScript;
        private Amount _fee = Amount.Null;
        private long _feePerKb = RootService.Network.Consensus.FeePerKilobyte;

        public string TxId => Encoders.HexReverse.Encode(TxHash);
        public UInt256 TxHash { get; private set; }
        public int Version { get; private set; } = 1;
        public uint LockTime { get; private set; }
        public Address ChangeAddress { get; private set; }

        public TxInCollection Inputs { get; private set; } //this transaction's inputs
        public TxOutCollection Outputs { get; private set; } //this transaction's outputs

        //if we have a Transaction with one input, and a prevTransactionId of zero, it's a coinbase.
        public bool IsCoinbase => Inputs.Count == 1 && Inputs[0].TxHash == UInt256.Zero;

        public TransactionOption Option { get; private set; }

        public Transaction()
        {
            Inputs = new TxInCollection();
            Outputs = new TxOutCollection();
        }

        public Transaction(byte[] bytes)
        {
            var reader = new ByteSequenceReader(bytes);
            TryReadTransaction(ref reader);
        }

        public Transaction(string hex)
            : this(Encoders.Hex.Decode(hex))
        {
        }

        public Transaction(int version, TxInCollection vin, TxOutCollection vout, uint lockTime, long fee = 0L, TransactionOption option = 0)
        {
            Version = version;
            Inputs = vin;
            Outputs = vout;
            LockTime = lockTime;
            _fee = new Amount(fee);
            Option = option;
        }

        /// <summary>
        /// Add transaction input
        /// </summary>
        /// <param name="input"></param>
        /// <returns>transaction</returns>
        public Transaction AddInput(TxIn input)
        {
            Inputs.Add(input);
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add transaction input
        /// </summary>
        /// <param name="prevout"></param>
        /// <param name="scriptSig"></param>
        /// <param name="amount"></param>
        /// <param name="sequence"></param>
        public Transaction AddInput(OutPoint prevout, Script scriptSig, long amount = 0L, uint sequence = TxIn.SequenceFinal)
        {
            Inputs.Add(new TxIn(prevout, amount, scriptSig,  sequence));
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add transaction inputs
        /// </summary>
        /// <param name="inputs">input collection</param>
        /// <returns>transaction</returns>
        public Transaction AddInputs(TxInCollection inputs)
        {
            Inputs.AddRange(inputs);
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add transaction output.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public Transaction AddOutput(TxOut output)
        {
            Outputs.Add(output);
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add transaction output.
        /// </summary>
        /// <param name="txHash"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        /// <param name="script"></param>
        /// <param name="isChangeOutput"></param>
        /// <returns></returns>
        public Transaction AddOutput(UInt256 txHash, int index, Amount amount, ScriptBuilder script, bool isChangeOutput = false)
        {
            return AddOutput(new TxOut(txHash, index, amount, script, isChangeOutput));
        }

        /// <summary>
        /// Add transaction outputs
        /// </summary>
        /// <param name="outputs">output collection</param>
        /// <returns>transaction</returns>
        public Transaction AddOutputs(TxOutCollection outputs)
        {
            Outputs.AddRange(outputs);
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add a DataLockBuilder that creates an unspendable, zero-satoshi output
        /// with associated data attached.
        /// 
        /// This method can be called more than once to add multiple data outputs.
        /// 
        /// [data] - The data to add to the output transaction
        /// 
        /// [scriptBuilder] - An instance (or subclass) of [DataLockBuilder] that
        /// will provide the scriptPubKey. The base [DataLockBuilder] will be used
        /// by default, and that results in a very simple data output that has the form
        ///    `OP_FALSE OP_RETURN &lt;data&gt;`
        /// 
        /// Returns an instance of the current Transaction as part of the builder pattern.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="scriptBuilder"></param>
        /// <returns></returns>
        public Transaction AddData(byte[] data, ScriptBuilder scriptBuilder = null)
        {
            scriptBuilder ??= new DataScriptBuilder();
            scriptBuilder.Add(data);
            var dataOut = new TxOut(TxHash, Outputs.Count, scriptBuilder);
            Outputs.Add(dataOut);
            return this;
        }

        /// <summary>
        /// Get change output
        /// </summary>
        /// <param name="changeBuilder"></param>
        /// <returns></returns>
        public TxOut GetChangeOutput(ScriptBuilder changeBuilder)
        {
            var txOut = Outputs.SingleOrDefault(x => x.IsChangeOutput);
            if (txOut != null) return txOut;

            txOut = new TxOut(TxHash, Outputs.Count, changeBuilder, true);
            return txOut;
        }

        /// <summary>
        ///  Calculates the fee of the transaction.
        ///
        ///  If there's a fixed fee set, return that.
        ///
        ///  If there is no change output set, the fee is the
        ///  total value of the outputs minus inputs. Note that
        ///  a serialized transaction only specifies the value
        ///  of its outputs. (The value of inputs are recorded
        ///  in the previous transaction outputs being spent.)
        ///  This method therefore raises a 'MissingPreviousOutput'
        ///  error when called on a serialized transaction.
        ///
        ///  If there's no fee set and no change address,
        ///  estimate the fee based on size.
        ///
        ///  *NOTE* : This fee calculation strategy is taken from the MoneyButton/BSV library.
        /// </summary>
        /// <returns></returns>
        public Amount GetFee()
        {
            if (IsCoinbase)
            {
                return Amount.Zero;
            }

            if (_fee != Amount.Null)
            {
                return _fee;
            }

            // if no change output is set, fees should equal all the unspent amount
            return !_hasChangeScript ? GetUnspentAmount() : EstimateFee();
        }

        /// <summary>
        /// Add a "change" output to this transaction
        /// 
        /// When a new transaction is created to spend coins from an input transaction,
        /// the entire *UTXO* needs to be consumed. I.e you cannot *partially* spend coins.
        /// What needs to happen is :
        ///   1) You consumer the entire UTXO in the new transaction input
        ///   2) You subtract a *change* amount from the UTXO and the remainder will be sent to the receiving party
        /// 
        /// The change amount is automatically calculated based on the fee rate that you set with [withFee()] or [withFeePerKb()]
        /// 
        /// [changeAddress] - A bitcoin address where a standard P2PKH (Pay-To-Public-Key-Hash) output will be "sent"
        /// 
        /// [scriptBuilder] - A [LockingScriptBuilder] that will be used to create the locking script (scriptPubKey) for the [TransactionOutput].
        ///                   A null value results in a [P2PKHLockBuilder] being used by default, which will create a Pay-to-Public-Key-Hash output script.
        /// 
        /// Returns an instance of the current Transaction as part of the builder pattern.
        /// 
        /// </summary>
        /// <param name="changeAddress"></param>
        /// <param name="scriptBuilder"></param>
        /// <returns></returns>
        public Transaction SendChangeTo(Address changeAddress, ScriptBuilder scriptBuilder = null)
        {
            scriptBuilder ??= new P2PkhLockBuilder(changeAddress);

            _hasChangeScript = true;
            //get fee, and if there is not enough change to cover fee, remove change outputs

            //delete previous change transaction if exists
            ChangeAddress = changeAddress;
            _changeScriptBuilder = scriptBuilder;
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Spend from Utxo
        /// </summary>
        /// <param name="txHash">utxo transaction hash</param>
        /// <param name="outputIndex">utxo index</param>
        /// <param name="amount">amount</param>
        /// <param name="scriptPubKey">script pub key</param>
        /// <param name="scriptBuilder">signed unlock script</param>
        /// <returns>transaction</returns>
        public Transaction SpendFrom(UInt256 txHash, int outputIndex, Amount amount, Script scriptPubKey, SignedUnlockBuilder scriptBuilder = null)
        {
            var txIn = new TxIn(txHash, outputIndex, amount, scriptPubKey, scriptBuilder);
            Inputs.Add(txIn);
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Spend from Utxo
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="checkInputExist"></param>
        /// <returns></returns>
        public Transaction SpendFromUtxo(Utxo utxo, bool checkInputExist = false) =>
            SpendFromUtxo(utxo, null, checkInputExist);
        
        /// <summary>
        /// Spend from Utxo
        /// </summary>
        /// <param name="utxo">utxo</param>
        /// <param name="scriptBuilder"></param>
        /// <param name="checkInputExist"></param>
        /// <returns>transaction</returns>
        public Transaction SpendFromUtxo(Utxo utxo, SignedUnlockBuilder scriptBuilder, bool checkInputExist = false)
        {
            return checkInputExist && InputExists(utxo.TxHash, utxo.Index)
                ? this
                : SpendFrom(utxo.TxHash, utxo.Index, utxo.Amount, utxo.ScriptPubKey, scriptBuilder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="sats"></param>
        /// <param name="scriptBuilder"></param>
        /// <returns></returns>
        public Transaction SpendTo(Address recipient, Amount sats, ScriptBuilder scriptBuilder = null)
        {
            if (sats <= Amount.Zero) throw new TransactionAmountException("You can only spend a positive amount of satoshis");

            scriptBuilder ??= new P2PkhLockBuilder(recipient);
            var txOut = new TxOut(TxHash, Outputs.Count, sats, scriptBuilder);
            return AddOutput(txOut);
        }

        /// <summary>
        /// Sort inputs and outputs according to Bip69
        /// </summary>
        /// <returns>transaction</returns>
        public Transaction Sort()
        {
            SortInputs(Inputs);
            SortOutputs(Outputs);
            return this;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>transaction string representation</returns>
        public override string ToString()
        {
            return TxHash.ToString();
        }

        /// <summary>
        /// Add fee to transaction.
        /// </summary>
        /// <param name="fee"></param>
        /// <returns>transaction</returns>
        public Transaction WithFee(Amount fee)
        {
            _fee = fee;
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// With fee per kilobyte.
        /// </summary>
        /// <param name="feePerKb"></param>
        /// <returns></returns>
        public Transaction WithFeePerKb(int feePerKb)
        {
            _feePerKb = feePerKb;
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// With locktime.
        /// </summary>
        /// <param name="lockTime">locktime</param>
        /// <returns>transaction</returns>
        public Transaction WithVersion(uint lockTime)
        {
            LockTime = lockTime;
            return this;
        }

        public Transaction WithOption(TransactionOption option)
        {
            Option = option;
            return this;
        }

        /// <summary>
        /// With version.
        /// </summary>
        /// <param name="version">version number</param>
        /// <returns>transaction</returns>
        public Transaction WithVersion(int version)
        {
            Version = version;
            return this;
        }
        
        /// <summary>
        /// Set the locktime flag on the transaction to prevent it becoming
        /// spendable before specified date
        ///
        /// [future] - The date in future before which transaction will not be spendable.
        /// </summary>
        /// <param name="future"></param>
        /// <returns></returns>
        public Transaction LockUntilDate(DateTime future)
        {
            if (future.ToUnixTime() < RootService.Network.Consensus.LocktimeBlockheightLimit)
            {
                throw new LockTimeException("Block time is set too early");
            }

            Inputs.ForEach(x =>
            {
                if (x.SequenceNumber == RootService.Network.Consensus.DefaultSeqnumber)
                {
                    x.SequenceNumber = (uint)RootService.Network.Consensus.DefaultLocktimeSeqnumber;
                }
            });

            LockTime = (uint)TimeSpan.FromMilliseconds(future.ToUnixTime()).TotalSeconds;
            return this;
        }

        /// <summary>
        /// Verify signature.
        /// </summary>
        /// <param name="scriptSig"></param>
        /// <param name="publicKey"></param>
        /// <param name="nTxIn"></param>
        /// <param name="script"></param>
        /// <param name="amount"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool VerifySignature(VarType scriptSig, PublicKey publicKey, int nTxIn, Script script, Amount amount, ScriptFlags flags)
        {
            var checker = new TransactionSignatureChecker(this, nTxIn, amount);
            return checker.CheckSignature(scriptSig, publicKey.ToArray(), script, flags);
        }

        /// <summary>
        /// Sign input transaction.
        /// </summary>
        /// <param name="nTxIn"></param>
        /// <param name="privateKey"></param>
        /// <param name="sighashType"></param>
        public void SignInput(int nTxIn, PrivateKey privateKey, SignatureHashEnum sighashType = SignatureHashEnum.Unsupported)
        {
            if (nTxIn + 1 > Inputs.Count)
            {
                throw new TransactionException($"Input index out of range. Max index is {Inputs.Count + 1}");
            }

            if (!Inputs.Any())
            {
                throw new TransactionException("No Inputs defined. Please add some Transaction Inputs");
            }

            Inputs[nTxIn].Sign(this, privateKey, sighashType);
        }

        /// <summary>
        /// Deserialize transaction.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool TryReadTransaction(ref ByteSequenceReader r)
        {
            var start = r.Data.Position;

            if (!r.TryReadLittleEndian(out int version)) return false;
            Version = version;

            if (!r.TryReadVariant(out var countIn)) return false;
            Inputs = new TxInCollection();
            for (var i = 0L; i < countIn; i++)
            {
                var txIn = new TxIn();
                if (!txIn.TryReadTxIn(ref r)) return false;
                Inputs.Add(txIn);
            }

            if (!r.TryReadVariant(out long countOut)) return false;
            Outputs = new TxOutCollection();
            for (var i = 0L; i < countOut; i++)
            {
                var txOut = new TxOut();
                if (!txOut.TryReadTxOut(ref r)) return false;
                Outputs.Add(txOut);
            }

            if (!r.TryReadLittleEndian(out uint lockTime)) return false;
            LockTime = lockTime;

            var end = r.Data.Position;

            // Compute the transaction hash.
            var txBytes = r.Data.Sequence.Slice(start, end).ToArray();
            var hash1 = Hashes.ComputeSha256(txBytes);
            var hash2 = Hashes.ComputeSha256(hash1);
            TxHash = new UInt256(hash2);
            return true;
        }

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="performChecks">perform serialization check flag</param>
        /// <returns>readonly byte span</returns>
        public byte[] Serialize(bool performChecks = false)
        {
            var writer = (ByteDataWriter) WriteTo(new ByteDataWriter(), new {performChecks});
            return writer.ToArray();
        }

        /// <summary>
        /// Try verify transaction.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>true if successful; false otherwise</returns>
        public bool TryVerify(out string errorMessage)
        {
            try
            {
                DoSerializationChecks();
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Verify transaction.
        /// </summary>
        /// <returns>true if successful; false otherwise</returns>
        public bool Verify() => TryVerify(out _);

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer) => WriteTo(writer, new { performChecks = false });

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <param name="parameters">performCheck parameter</param>
        /// <returns></returns>
        public IDataWriter WriteTo(IDataWriter writer, object parameters)
        {
            dynamic args = parameters;
            if (args.performChecks)
            {
                DoSerializationChecks();
            }

            return UncheckedSerialize(writer);
        }

        #region Helpers

        /// <summary>
        ///  Check for missing signature.
        /// </summary>
        /// <exception cref="TransactionException"></exception>
        private void CheckForMissingSignatures()
        {
            if ((Option & TransactionOption.DisableFullySigned) != 0) return;

            if (!IsFullySigned)
            {
                throw new TransactionException("Missing Signatures");
            }
        }

        /// <summary>
        /// Check for fee errors
        /// </summary>
        /// <param name="unspent">unspent amount</param>
        /// <exception cref="TransactionException"></exception>
        private void CheckForFeeErrors(Amount unspent)
        {
            if (_fee != Amount.Null && _fee != unspent)
            {
                throw new TransactionFeeException($"Unspent amount is {unspent} but the specified fee is {_fee}.");
            }

            if ((Option & TransactionOption.DisableLargeFees) != 0) return;

            var maximumFee = RootService.Network.Consensus.FeeSecurityMargin * EstimateFee();
            if (unspent <= maximumFee) return;

            if (!_hasChangeScript)
            {
                throw new TransactionFeeException("Fee is too large and no change address was provided");
            }

            throw new TransactionFeeException($"Expected less than {maximumFee} but got {unspent}");
        }

        /// <summary>
        /// Determines whether an input transaction exist. 
        /// </summary>
        /// <param name="txHash"></param>
        /// <param name="outputIndex"></param>
        /// <returns></returns>
        private bool InputExists(UInt256 txHash, int outputIndex) =>
            Inputs.Any(x => x.PrevOut.TxId == txHash && x.PrevOut.Index == outputIndex);

        /// <summary>
        ///  Is the collection of inputs fully signed.
        /// </summary>
        /// <returns></returns>
        private bool IsFullySigned => Inputs.All(x => x.IsFullySigned);

        /// <summary>
        /// Update the transaction change output.
        /// </summary>
        private void UpdateChangeOutput()
        {
            if (ChangeAddress == null) return;

            if (_changeScriptBuilder == null) return;

            RemoveChangeOutputs();

            if (NonChangeRecipientTotals() == InputTotals()) return;

            var txOut = GetChangeOutput(_changeScriptBuilder);
            var changeAmount = RecalculateChange();

            ////can't spend negative amount of change :/
            if (changeAmount <= Amount.Zero) return;
            Outputs.Add(new TxOut(txOut.TxHash, 0, changeAmount, _changeScriptBuilder, true));
        }

        private void RemoveChangeOutputs() => Outputs.Where(x => x.IsChangeOutput).ForEach(x => Outputs.Remove(x));

        private Amount NonChangeRecipientTotals() =>
            Outputs
                .Where(txOut => !txOut.IsChangeOutput)
                .Aggregate(Amount.Zero, (prev, x) => prev + x.Amount);

        private Amount RecipientTotals() => Outputs.Aggregate(Amount.Zero, (prev, x) => prev + x.Amount);

        private Amount InputTotals() => Inputs.Aggregate(Amount.Zero, (prev, x) => prev + x.Amount);

        private Amount RecalculateChange()
        {
            var inputAmount = InputTotals();
            var outputAmount = NonChangeRecipientTotals();
            var unspent = inputAmount - outputAmount;
            return unspent - GetFee();
        }

        /// Estimates fee from serialized transaction size in bytes.

        /// <summary>
        /// Get the transaction unspent amount.  
        /// </summary>
        /// <returns></returns>
        private Amount GetUnspentAmount() => InputTotals() - RecipientTotals();

        /// <summary>
        /// Calculate fee estimate.
        /// </summary>
        /// <returns>fee estimate</returns>
        private Amount EstimateFee()
        {
            var estimatedSize = EstimateSize();
            var available = GetUnspentAmount();

            var fee = new Amount((long)Math.Ceiling((double)estimatedSize / 1000 * _feePerKb));
            if (available > fee)
            {
                estimatedSize += RootService.Network.Consensus.ChangeOutputMaxSize;
            }

            fee = new Amount((long)Math.Ceiling((double)estimatedSize / 1000 * _feePerKb));
            return fee;
        }

        /// <summary>
        /// Determine size estimate.
        /// </summary>
        /// <returns></returns>
        private int EstimateSize()
        {
            var result = RootService.Network.Consensus.MaximumExtraSize;

            //_txnInputs.forEach((input) {
            //    result += SCRIPT_MAX_SIZE; 
            //});

            //Note: we're only spending P2PKH atm.
            result += RootService.Network.Consensus.ScriptMaxSize * Inputs.Count;

            // <---- HOW DO WE CALCULATE SCRIPT FROM JUST AN ADDRESS !? AND LENGTH ???
            Outputs.ForEach(x => result += Encoders.Hex.Decode(x.Script.ToHexString()).Length + 9);
            return result;
        }

        /// <summary>
        /// Sort inputs in accordance to BIP69.
        /// </summary>
        /// <param name="inputs"></param>
        private void SortInputs(TxInCollection inputs)
        {
            Inputs = new TxInCollection(inputs.OrderBy(x => x.TxId).ToArray());
        }

        /// <summary>
        /// Sort outputs in accordance to BIP69.
        /// </summary>
        /// <param name="outputs"></param>
        private void SortOutputs(TxOutCollection outputs)
        {
            Outputs = new TxOutCollection(outputs.OrderBy(x => x.Amount).ToArray());
        }

        private void DoSerializationChecks()
        {
            if (Outputs.Any(x => !x.ValidAmount))
            {
                throw new TransactionAmountException("Invalid amount of satoshis");
            }

            var unspent = GetUnspentAmount();
            if (unspent < Amount.Zero && (Option & TransactionOption.DisableMoreOutputThanInput) == 0)
            {
                throw new TransactionAmountException("Invalid output sum of satoshis");
            }

            CheckForFeeErrors(unspent);
            CheckForDustErrors();
            CheckForMissingSignatures();
        }

        private void CheckForDustErrors()
        {
            if ((Option & TransactionOption.DisableDustOutputs) != 0) return;

            if (Outputs.Any(x => x.Amount < RootService.Network.Consensus.DustLimit && !x.IsDataOut))
            {
                throw new TransactionAmountException("You have outputs with spending values below the dust limit");
            }
        }

        /// <summary>
        /// Returns the raw transaction as a hexadecimal string, skipping all checks.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        private IDataWriter UncheckedSerialize(IDataWriter writer)
        {
            // set the transaction version
            writer.Write(Version);

            // set the number of inputs
            writer.Write(new VarInt(Inputs.Count));

            // write the inputs
            Inputs.ForEach(x => x.WriteTo(writer));

            //set the number of outputs to come
            writer.Write(new VarInt(Outputs.Count));

            // write the outputs
            Outputs.ForEach(x => x.WriteTo(writer));

            // write the locktime
            writer.Write(LockTime);

            return writer;
        }

        #endregion
    }
}
