using System;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public class BloomFilter // : IBitcoinSerializable
    {
        // 20,000 items with fp rate < 0.1% or 10,000 items and <0.0001%
        private const uint MaxBloomFilterSize = 36000; // bytes
        private const uint MaxHashFunctions = 50;
        private const decimal Ln2Squared = 0.4804530139182014246671025263266649717305529515945455M;
        private const decimal Ln2 = 0.6931471805599453094172321214581765680755001343602552M;

        byte[] vData;
        uint nHashFuncs;
        uint nTweak;
        byte nFlags;
        private bool isFull = false;
        private bool isEmpty;

        public BloomFilter()
        {

        }

        public BloomFilter(int nElements, double nFPRate, BloomFlags nFlagsIn = BloomFlags.UPDATE_ALL)
            : this(nElements, nFPRate, RandomUtils.GetUInt32(), nFlagsIn)
        {
        }


        public BloomFilter(int nElements, double nFPRate, uint nTweakIn, BloomFlags nFlagsIn = BloomFlags.UPDATE_ALL)
        {
            // The ideal size for a bloom filter with a given number of elements and false positive rate is:
            // - nElements * log(fp rate) / ln(2)^2
            // We ignore filter parameters which will create a bloom filter larger than the protocol limits
            vData = new byte[Math.Min((uint)(-1 / Ln2Squared * nElements * (decimal)Math.Log(nFPRate)),
                MaxBloomFilterSize) / 8];
            //vData(min((unsigned int)(-1  / LN2SQUARED * nElements * log(nFPRate)), MAX_BLOOM_FILTER_SIZE * 8) / 8),
            // The ideal number of hash functions is filter size * ln(2) / number of elements
            // Again, we ignore filter parameters which will create a bloom filter with more hash functions than the protocol limits
            // See http://en.wikipedia.org/wiki/Bloom_filter for an explanation of these formulas

            this.nHashFuncs = Math.Min((uint)(vData.Length * 8 / nElements * Ln2), MaxHashFunctions);
            this.nTweak = nTweakIn;
            this.nFlags = (byte)nFlagsIn;


        }

        uint Hash(uint nHashNum, byte[] vDataToHash)
        {
            // 0xFBA4C795 chosen as it guarantees a reasonable bit difference between nHashNum values.
            return (uint)(Hashes.MurmurHash3(nHashNum * 0xFBA4C795 + nTweak, vDataToHash) % (vData.Length * 8));
        }

        public void Insert(byte[] vKey)
        {
            if (isFull)
                return;
            for (uint i = 0; i < nHashFuncs; i++)
            {
                uint nIndex = Hash(i, vKey);
                // Sets bit nIndex of vData
                vData[nIndex >> 3] |= (byte)(1 << (7 & (int)nIndex));
            }

            isEmpty = false;
        }

        public bool Contains(byte[] vKey)
        {
            if (isFull)
                return true;
            if (isEmpty)
                return false;
            for (uint i = 0; i < nHashFuncs; i++)
            {
                uint nIndex = Hash(i, vKey);
                // Checks bit nIndex of vData
                if ((vData[nIndex >> 3] & (byte)(1 << (7 & (int)nIndex))) == 0)
                    return false;
            }

            return true;
        }

        public bool Contains(OutPoint outPoint)
        {
            return Contains(outPoint.ToBytes());
        }

        public bool Contains(uint256 hash)
        {
            return Contains(hash.ToBytes());
        }

        public void Insert(OutPoint outPoint)
        {
            Insert(outPoint.ToBytes());
        }

        public void Insert(uint256 value)
        {
            Insert(value.ToBytes());
        }

        public bool IsWithinSizeConstraints()
        {
            return vData.Length <= MaxBloomFilterSize && nHashFuncs <= MaxHashFunctions;
        }

        #region IBitcoinSerializable Members

        public void ReadWrite(BitcoinStream stream)
        {
            stream.ReadWriteAsVarString(ref vData);
            stream.ReadWrite(ref nHashFuncs);
            stream.ReadWrite(ref nTweak);
            stream.ReadWrite(ref nFlags);
        }

        #endregion



        public bool IsRelevantAndUpdate(Transaction tx)
        {
            var hash = tx.TxHash;
            bool fFound = false;

            // Match if the filter contains the hash of tx
            //  for finding tx when they appear in a block
            if (isFull)
                return true;

            if (isEmpty)
                return false;

            if (Contains(hash))
                fFound = true;

            tx.Outputs.ForEach((x, i) =>
            {
                // Match if the filter contains any arbitrary script data element in any scriptPubKey in tx
                // If this matches, also add the specific output that was matched.
                // This means clients don't have to update the filter themselves when a new relevant tx 
                // is discovered in order to find spending transactions, which avoids round-tripping and race conditions.
                if (((ScriptBuilder)x.Script).Operands.Any(op => op.Length != 0 && Contains(op.Operand.Data)))
                {
                    fFound = true;
                    switch (nFlags & (byte)BloomFlags.UPDATE_MASK)
                    {
                        case (byte)BloomFlags.UPDATE_ALL:
                            Insert(new OutPoint(hash, i));
                            break;

                        case (byte)BloomFlags.UPDATE_P2PUBKEY_ONLY:
                        {

                            var template = StandardScripts.GetTemplateFromScriptPubKey(x.Script);
                            if (template != null &&
                                (template.Type == TxOutType.TX_PUBKEY || template.Type == TxOutType.TX_MULTISIG))
                                Insert(new OutPoint(hash, i));

                            break;
                        }
                    }
                }
            });

            for (uint i = 0; i < tx.Outputs.Count; i++)
            {
                var txout = tx.Outputs[(int)i];
                // Match if the filter contains any arbitrary script data element in any scriptPubKey in tx
                // If this matches, also add the specific output that was matched.
                // This means clients don't have to update the filter themselves when a new relevant tx 
                // is discovered in order to find spending transactions, which avoids round-tripping and race conditions.
                foreach (Op op in txout.ScriptPubKey.ToOps())
                {
                    if (op.PushData != null && op.PushData.Length != 0 && Contains(op.PushData))
                    {
                        fFound = true;
                        if ((nFlags & (byte)BloomFlags.UPDATE_MASK) == (byte)BloomFlags.UPDATE_ALL)
                            Insert(new OutPoint(hash, i));
                        else if ((nFlags & (byte)BloomFlags.UPDATE_MASK) == (byte)BloomFlags.UPDATE_P2PUBKEY_ONLY)
                        {
                            var template = StandardScripts.GetTemplateFromScriptPubKey(txout.ScriptPubKey);
                            if (template != null &&
                                (template.Type == TxOutType.TX_PUBKEY || template.Type == TxOutType.TX_MULTISIG))
                                Insert(new OutPoint(hash, i));
                        }

                        break;
                    }
                }
            }

            if (fFound)
                return true;

            foreach (TxIn txin in tx.Inputs)
            {
                // Match if the filter contains an outpoint tx spends
                if (Contains(txin.PrevOut))
                    return true;

                // Match if the filter contains any arbitrary script data element in any scriptSig in tx
                foreach (Op op in txin.ScriptSig.ToOps())
                {
                    if (op.PushData != null && op.PushData.Length != 0 && Contains(op.PushData))
                        return true;
                }
            }

            return false;
        }
    }
}
