using System;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Mapi.Extensions;
using CafeLib.BsvSharp.Mapi.MatterPool;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests 
{
    public class MatterPoolClientTests
    {
        private readonly MatterPoolClient _matterPool = new MatterPoolClient();

        #region Mapi

        [Fact]
        public async Task GetFeeQuotes_Test()
        {
            var response = await _matterPool.GetFeeQuote();
            Assert.NotNull(response);
            Assert.Equal("matterpool", response.Result.ProviderName);

            var feeQuote = response.Result.Cargo;
            Assert.True(feeQuote.Expiry > DateTime.UtcNow);
            Assert.True(Math.Abs((feeQuote.Timestamp - DateTime.UtcNow).TotalMinutes) < 1);
            Assert.True(feeQuote.CurrentHighestBlockHeight > 630000);
            Assert.True(new UInt256(feeQuote.CurrentHighestBlockHash).ToBigInteger() > 0);
            Assert.Equal(2, feeQuote.Fees.Length);
            Assert.True(new PublicKey(feeQuote.MinerId).IsValid);
            Assert.True(feeQuote.GetStandardMiningFee().Bytes > 0);
            Assert.True(feeQuote.GetStandardMiningFee().Satoshis >= 0);
            Assert.True(feeQuote.GetStandardRelayFee().Bytes > 0);
            Assert.True(feeQuote.GetStandardRelayFee().Satoshis >= 0);
        }

        [Theory]
        [InlineData("3ea6bb35923dbff216aa11084280e0d6d477d78ed8010edac92c3253b3d79024")]
        public async Task GetTransactionStatus_Test(string txHash)
        {
            var response = await _matterPool.GetTransactionStatus(txHash);
            Assert.NotNull(response);
            Assert.Equal("matterpool", response.Result.ProviderName);
            Assert.NotNull(response.Result.Payload);

            var status = response.Result.Cargo;
            Assert.NotNull(status);
            Assert.Equal("failure", status.ReturnResult);
            Assert.True(status.ResultDescription.Length > 0);
            Assert.True(Math.Abs((status.Timestamp - DateTime.UtcNow).TotalMinutes) < 1);
            Assert.True(new PublicKey(status.MinerId).IsValid);
            Assert.Null(status.BlockHash);
            Assert.True(status.Confirmations == 0);
        }

        [Theory]
        [InlineData("0100000001747623f8e6f9b684c2c72d81245d1f1532043088e76ba63805339823e5b16389000000006a47304402204c108078b91ef1f6d2ce154b11bca8c31f6d37dac451a28c07edd7e737efef3802201ecfb09763d64d3ad293eec1c4ecaf0fd45b91dbdf428e422d282b98483300de4121036166800571f944768676842e4d2f8f96825c0f030139b6b78d6c9830de082828ffffffff09f9e15100000000001976a9143e0ea504169d4ef931e913cbbecb3f07b1d4b6f088acf9e15100000000001976a914229db1b4735321f46165ae5837e47dabd064f16e88acf9e15100000000001976a9144a5b03c7eea7b8e6e611559627a56963d514d1ea88ac6e6e5700000000001976a914c042299061557b60e0e5085bee8fadc8d7e5483388acf9e15100000000001976a914633d58a958c54d9858887b0f3aa65be4eb37f07488acf9e15100000000001976a9148d3cf51026f94d03fda5709160c7171b855ba22488ac50c84c00000000001976a9142a03a8943e47cdbd9ba448994e61d237e8d1ac4b88acf9e15100000000001976a914359f98091121e785e6663f10251832d9ae556f8588acf9e15100000000001976a91415a8feff23bfce20f837956c82e1eb1f2457f93488ac00000000")]
        public async Task SubmitTransaction_Test(string txRaw)
        {
            var response = await _matterPool.SubmitTransaction(txRaw);
            Assert.NotNull(response);
            Assert.Equal("matterpool", response.Result.ProviderName);
            Assert.NotNull(response.Result.Payload);

            var submit = response.Result.Cargo;
            Assert.True(submit.CurrentHighestBlockHeight > 630000);
            Assert.True(new UInt256(submit.CurrentHighestBlockHash) > UInt256.Zero);
            Assert.True(Math.Abs((submit.Timestamp - DateTime.UtcNow).TotalMinutes) < 1);
            Assert.True(new PublicKey(submit.MinerId).IsValid);
            Assert.Equal("failure", submit.ReturnResult);
            Assert.True(submit.ResultDescription.Length > 0); // e.g. Not enough fees
            Assert.Equal("", submit.TxId); // e.g. Not enough fees
            Assert.Equal(0, submit.TxSecondMempoolExpiry); // e.g. Not enough fees
        }

        #endregion
    }
}
