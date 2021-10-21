#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Signatures;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Signatures
{
    public class KzSignatureTests
    {
        [Theory]
        [InlineData("300602010002010001")]
        [InlineData("3008020200ff020200ff01")]
        [InlineData("304402203932c892e2e550f3af8ee4ce9c215a87f9bb831dcac87b2838e2c2eaa891df0c022030b61dd36543125d56b9f9f3a1f9353189e5af33cdda8d77a5209aec03978fa001")]
        [InlineData("30450220076045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        [InlineData("3046022100876045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        public void CanonicalSignatureTest_Static(string hex)
        {
            var signature = Encoders.Hex.Decode(hex);
            Assert.True(Signature.IsTxDerEncoding(signature));
        }

        [Theory]
        [InlineData("300602010002010001")]
        [InlineData("3008020200ff020200ff01")]
        [InlineData("304402203932c892e2e550f3af8ee4ce9c215a87f9bb831dcac87b2838e2c2eaa891df0c022030b61dd36543125d56b9f9f3a1f9353189e5af33cdda8d77a5209aec03978fa001")]
        [InlineData("30450220076045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        [InlineData("3046022100876045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        public void CanonicalSignatureTest_Instance(string hex)
        {
            var signature = Signature.FromHex(hex);
            Assert.True(signature.IsTxDerEncoding());
        }

        [Theory]
        [InlineData("300602010002010001")]
        [InlineData("3008020200ff020200ff01")]
        [InlineData("304402203932c892e2e550f3af8ee4ce9c215a87f9bb831dcac87b2838e2c2eaa891df0c022030b61dd36543125d56b9f9f3a1f9353189e5af33cdda8d77a5209aec03978fa001")]
        [InlineData("30450220076045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        [InlineData("3046022100876045be6f9eca28ff1ec606b833d0b87e70b2a630f5e3a496b110967a40f90a0221008fffd599910eefe00bc803c688c2eca1d2ba7f6b180620eaa03488e6585db6ba01")]
        public void CanonicalSignatureTest_ByteConstructor(string hex)
        {
            var bytes = Encoders.Hex.Decode(hex);
            var signature = new Signature(bytes);
            Assert.True(signature.IsTxDerEncoding());
        }
    }
}