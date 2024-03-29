﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Network;
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
    public class Transaction : ITransactionId, IDataSerializer
    {
        private ScriptBuilder _changeScriptBuilder;
        private bool _hasChangeScript;
        private Amount _fee = Amount.Null;
        private readonly Lazy<Consensus> _consensus;
        private long _feePerKb = -1L;

        public string TxId => Encoders.HexReverse.Encode(TxHash);

        public UInt256 TxHash => Hashes.Hash256(Serialize());

        public int Version { get; private set; } = 1;

        public uint LockTime { get; private set; }

        public Address ChangeAddress { get; private set; }

        /// <summary>
        /// Transaction inputs.
        /// </summary>
        public TransactionInputList Inputs { get; }

        /// <summary>
        /// Transaction outputs.
        /// </summary>
        public TransactionOutputList Outputs { get; } //this transaction's outputs

        /// <summary>
        /// Determine whether the transaction is a coinbase transaction.
        /// </summary>
        /// <returns>
        /// true if the transaction has a single input having a prevTransactionId of zero; false otherwise.
        /// </returns>
        public bool IsCoinbase => Inputs.Count == 1 && Inputs[0].TxHash == UInt256.Zero;

        /// <summary>
        /// Transaction option.
        /// </summary>
        public TransactionOption Option { get; private set; }

        /// <summary>
        /// Transaction default constructor.
        /// </summary>
        /// <param name="networkType">network type</param>
        public Transaction(NetworkType? networkType = null)
        {
            _consensus = new Lazy<Consensus>(() => RootService.GetNetwork(networkType).Consensus);
            Inputs = new TransactionInputList();
            Outputs = new TransactionOutputList();
        }

        /// <summary>
        /// Transaction constructor
        /// </summary>
        /// <param name="version">version</param>
        /// <param name="vin">inputs</param>
        /// <param name="vout">outputs</param>
        /// <param name="lockTime">lock time</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="networkType">network type</param>
        /// <param name="option">options</param>
        public Transaction(int version, TransactionInputList vin, TransactionOutputList vout, uint lockTime, long fee = 0L, NetworkType? networkType = null, TransactionOption option = 0)
        {
            Version = version;
            Inputs = vin;
            Outputs = vout;
            LockTime = lockTime;
            Option = option;
            _fee = new Amount(fee);
            _consensus = new Lazy<Consensus>(() => RootService.GetNetwork(networkType).Consensus);
        }

        /// <summary>
        /// Deserialize transaction from byte array.
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <returns>transaction</returns>
        public static Transaction FromBytes(byte[] bytes)
        {
            var transaction = new Transaction();
            var reader = new ByteSequenceReader(bytes);
            transaction.TryReadTransaction(ref reader);
            return transaction;
        }

        /// <summary>
        /// Deserialize transaction from hexadecimal string.
        /// </summary>
        /// <param name="hex">hex string</param>
        /// <returns>transaction</returns>
        public static Transaction FromHex(string hex) => FromBytes(Encoders.Hex.Decode(hex));


        /// <summary>
        /// Add transaction input
        /// </summary>
        /// <param name="input"></param>
        /// <returns>transaction</returns>
        public Transaction AddInput(TransactionInput input)
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
        public Transaction AddInput(OutPoint prevout, Script scriptSig, long amount = 0L, uint sequence = TransactionInput.SequenceFinal)
        {
            Inputs.Add(new TransactionInput(prevout, amount, scriptSig,  sequence));
            UpdateChangeOutput();
            return this;
        }

        /// <summary>
        /// Add transaction inputs
        /// </summary>
        /// <param name="inputs">input collection</param>
        /// <returns>transaction</returns>
        public Transaction AddInputs(TransactionInputList inputs)
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
        public Transaction AddOutput(TransactionOutput output)
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
            return AddOutput(new TransactionOutput(txHash, index, amount, script, isChangeOutput));
        }

        /// <summary>
        /// Add transaction outputs
        /// </summary>
        /// <param name="outputs">output collection</param>
        /// <returns>transaction</returns>
        public Transaction AddOutputs(TransactionOutputList outputs)
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
            var dataOut = new TransactionOutput(TxHash, Outputs.Count, scriptBuilder);
            Outputs.Add(dataOut);
            return this;
        }

        /// <summary>
        /// Get change output
        /// </summary>
        /// <param name="changeBuilder"></param>
        /// <returns></returns>
        public TransactionOutput GetChangeOutput(ScriptBuilder changeBuilder)
        {
            var txOut = Outputs.SingleOrDefault(x => x.IsChangeOutput);
            if (txOut != null) return txOut;

            txOut = new TransactionOutput(TxHash, Outputs.Count, changeBuilder, true);
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
        /// Spend from transaction
        /// </summary>
        /// <param name="txId">utxo transaction id</param>
        /// <param name="outputIndex">utxo index</param>
        /// <param name="amount">amount</param>
        /// <param name="scriptPubKey">script pub key</param>
        /// <param name="scriptBuilder">signed unlock script</param>
        /// <returns>transaction</returns>
        public Transaction SpendFrom(UInt256 txId, int outputIndex, Amount amount, Script scriptPubKey, SignedUnlockBuilder scriptBuilder = null)
        {
            var txIn = new TransactionInput(txId, outputIndex, amount, scriptPubKey, scriptBuilder);
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
            return checkInputExist && InputExists(utxo.TxId, utxo.Index)
                ? this
                : SpendFrom(utxo.TxId, utxo.Index, utxo.Amount, utxo.ScriptPubKey, scriptBuilder);
        }

        /// <summary>
        /// Spend to recipiant address
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="sats"></param>
        /// <param name="scriptBuilder"></param>
        /// <returns></returns>
        public Transaction SpendTo(Address recipient, Amount sats, ScriptBuilder scriptBuilder = null)
        {
            if (sats <= Amount.Zero) throw new TransactionAmountException("You can only spend a positive amount of satoshis");

            scriptBuilder ??= new P2PkhLockBuilder(recipient);
            var txOut = new TransactionOutput(TxHash, Outputs.Count, sats, scriptBuilder);
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
        /// Obtain the hex representation of the public key.
        /// </summary>
        /// <returns></returns>
        public string ToHex() => Encoders.Hex.Encode(Serialize());

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>transaction string representation</returns>
        public override string ToString()
        {
            return TxId;
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
            if (future.ToUnixTime() < Consensus.LocktimeBlockheightLimit)
            {
                throw new LockTimeException("Block time is set too early");
            }

            Inputs.ForEach(x =>
            {
                if (x.SequenceNumber == Consensus.DefaultSeqnumber)
                {
                    x.SequenceNumber = (uint)Consensus.DefaultLocktimeSeqnumber;
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
        /// Sign transaction.
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="sighashType">signature hash type</param>
        /// <returns>transaction</returns>
        public Transaction Sign(PrivateKey privateKey, SignatureHashEnum sighashType = SignatureHashEnum.Unsupported)
        {
            return Sign(0, privateKey, sighashType);
        }

        /// <summary>
        /// Sign transaction.
        /// </summary>
        /// <param name="nTxIn">input index</param>
        /// <param name="privateKey">private key</param>
        /// <param name="sighashType">signature hash type</param>
        /// <returns>transaction</returns>
        public Transaction Sign(int nTxIn, PrivateKey privateKey, SignatureHashEnum sighashType = SignatureHashEnum.Unsupported)
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
            return this;
        }

        /// <summary>
        /// Deserialize transaction.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        internal bool TryReadTransaction(ref ByteSequenceReader r)
        {
            if (!r.TryReadLittleEndian(out int version)) return false;
            Version = version;

            if (!r.TryReadVariant(out var countIn)) return false;
            Inputs.Clear();
            for (var i = 0L; i < countIn; i++)
            {
                var txIn = new TransactionInput();
                if (!txIn.TryReadTxIn(ref r)) return false;
                Inputs.Add(txIn);
            }

            if (!r.TryReadVariant(out long countOut)) return false;
            Outputs.Clear();
            for (var i = 0L; i < countOut; i++)
            {
                var txOut = new TransactionOutput();
                if (!txOut.TryReadTxOut(ref r)) return false;
                Outputs.Add(txOut);
            }

            if (!r.TryReadLittleEndian(out uint lockTime)) return false;
            LockTime = lockTime;

            return true;
        }

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="performChecks">perform serialization check flag</param>
        /// <returns>readonly byte span</returns>
        public byte[] Serialize(bool performChecks = false)
        {
            var writer = (ByteDataWriter) WriteTo(new ByteDataWriter(), performChecks);
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
        public IDataWriter WriteTo(IDataWriter writer) => WriteTo(writer, false);

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <param name="performChecks">performCheck parameter</param>
        /// <returns></returns>
        public IDataWriter WriteTo(IDataWriter writer, bool performChecks)
        {
            if (performChecks)
            {
                DoSerializationChecks();
            }

            return UncheckedSerialize(writer);
        }

        #region Helpers


        private Consensus Consensus => _consensus.Value;


        //The hash is the double-sha256 of the serialized transaction (reversed)
        //private UInt256 GetHash() => 
        //List<int> _getHash()
        //{
        //    List<int> hash = sha256Twice(HEX.decode(serialize(performChecks: false)));
        //    return hash;
        //}

        ////The id is the hex encoded form of the hash
        //String _getId()
        //{
        //    var id = HEX.encode(_getHash().reversed.toList());
        //    _txId = id;
        //    return _txId;
        //}

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

            var maximumFee = Consensus.FeeSecurityMargin * EstimateFee();
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

            Outputs.Add(new TransactionOutput(UInt256.Zero, 0, 0L, _changeScriptBuilder, true));

            var txOut = GetChangeOutput(_changeScriptBuilder);
            var changeAmount = RecalculateChange();

            Outputs.RemoveAt(Outputs.Length - 1);

            ////can't spend negative amount of change :/
            if (changeAmount <= Amount.Zero) return;
            Outputs.Add(new TransactionOutput(txOut.TxHash, 0, changeAmount, _changeScriptBuilder, true));
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
            var fee = new Amount((long)Math.Ceiling((double)estimatedSize / 1000 * GetFeePerKb()));
            return fee;
        }

        private Amount GetFeePerKb()
        {
            return _feePerKb == -1L ? Consensus.FeePerKilobyte : _feePerKb;
        }

        /// <summary>
        /// Estimate transaction size.
        /// </summary>
        /// <returns></returns>
        private int EstimateSize()
        {
            var result = sizeof(int) + sizeof(int); // size of version + size of locktime
            result += new VarInt(Inputs.Length).Length;
            result += new VarInt(Outputs.Length).Length;
            result += Consensus.ScriptMaxSize * Inputs.Count; //P2PKH script size.

            var writer = new ByteDataWriter();
            Outputs.ForEach(x => x.WriteTo(writer));
            result += writer.ToArray().Length;
            return result;
        }

        /// <summary>
        /// Sort inputs in accordance to BIP69.
        /// </summary>
        /// <param name="inputs"></param>
        private void SortInputs(TransactionInputList inputs)
        {
            Inputs.Clear();
            Inputs.AddRange(new TransactionInputList(inputs.OrderBy(x => x.TxId).ToArray()));
        }

        /// <summary>
        /// Sort outputs in accordance to BIP69.
        /// </summary>
        /// <param name="outputs"></param>
        private void SortOutputs(TransactionOutputList outputs)
        {
            Outputs.Clear();
            Outputs.AddRange(new TransactionOutputList(outputs.OrderBy(x => x.Amount).ToArray()));
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

            if (Outputs.Any(x => x.Amount < Consensus.DustLimit && !x.IsDataOut))
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

            //set the number of outputs
            writer.Write(new VarInt(Outputs.Count));

            // write the outputs
            Outputs.ForEach(x => x.WriteTo(writer));

            // write the locktime
            writer.Write(LockTime);

            // return writer.
            return writer;
        }

        #endregion
    }
}
