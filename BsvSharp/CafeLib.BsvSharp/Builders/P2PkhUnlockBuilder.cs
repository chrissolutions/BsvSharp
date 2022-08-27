﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;

namespace CafeLib.BsvSharp.Builders
{
    public class P2PkhUnlockBuilder : SignedUnlockBuilder
    {
        /// <summary>
        /// P2PkhUnlockBuilder constructor.
        /// </summary>
        /// <param name="publicKey">public key</param>
        public P2PkhUnlockBuilder(PublicKey publicKey)
            : base(publicKey, TemplateId.Pay2PublicKeyHash)
        {
            Pushdata(new byte[72]) // This will become the CHECKSIG signature
                .Pushdata(publicKey);
        }
        
        public P2PkhUnlockBuilder(Script script)
            : base(null, TemplateId.Pay2PublicKeyHash)
        {
            UnlockScript(script);
        }

        public override void Sign(Script scriptSig)
        {
            UnlockScript(scriptSig);
        }

        public override Script ToScript()
        {
            base.Clear();
            
            Pushdata(Signatures.FirstOrDefault().ToTxFormat().Data)
                .Pushdata(PublicKey);

            return base.ToScript();
        }

        private void UnlockScript(Script scriptSig)
        {
            if (scriptSig == Script.None)
            {
                throw new ScriptException("Invalid Script or Malformed Script.");
            }

            Set(scriptSig);

            if (Operands.Count != 2)
            {
                throw new ScriptException("Wrong number of data elements for P2PKH ScriptSig");
            }

            AddSignature(new Signature(Operands.First().Operand.Data));
            PublicKey = new PublicKey(Operands.Last().Operand.Data);
        }
    }
}
