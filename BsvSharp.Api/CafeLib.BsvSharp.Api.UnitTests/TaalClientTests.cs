using System;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Mapi.Extensions;
using CafeLib.BsvSharp.Mapi.Models;
using CafeLib.BsvSharp.Mapi.Responses;
using CafeLib.BsvSharp.Mapi.Taal;
using CafeLib.Core.Extensions;
using CafeLib.Core.Support;
using CafeLib.Web.Request;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests 
{
    public class TaalClientTests
    {
        private readonly string _apiKey;
        private readonly TaalClient _taal;

        public TaalClientTests()
        {
            _apiKey = Guid.NewGuid().ToString();
            _taal = new TaalClient(_apiKey);
        }

        #region Mapi

        [Fact]
        public async Task GetFeeQuotes_Test()
        {
            var response = await _taal.GetFeeQuote();
            Assert.NotNull(response);
            Assert.Equal("taal", response.Result.ProviderName);

            var feeQuote = response.Result.Payload;
            Assert.True(feeQuote.Expiry > DateTime.UtcNow);
            Assert.True(Math.Abs((feeQuote.Timestamp - DateTime.UtcNow).TotalMinutes) < 1);
            Assert.Equal(0, feeQuote.CurrentHighestBlockHeight);
            Assert.Equal("", feeQuote.CurrentHighestBlockHash);
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
            var response = await _taal.GetTransactionStatus(txHash);
            Assert.NotNull(response);
            Assert.Equal("taal", response.Result.ProviderName);
            Assert.NotNull(response.Result.JsonPayload);

            var status = response.Result.Payload;
            Assert.NotNull(status);
            Assert.Equal("success", status.ReturnResult);
            Assert.Null(status.ResultDescription);
            Assert.True(Math.Abs((status.Timestamp - DateTime.UtcNow).TotalMinutes) < 1);
            Assert.True(new PublicKey(status.MinerId).IsValid);
            Assert.Equal("0000000000000000011e0221844b65bfbbc2599bbd7f71ca0f914a53d90fa8b6", status.BlockHash);
            Assert.True(status.Confirmations >= 81187);
        }

        [Theory]
        [InlineData("0100000001747623f8e6f9b684c2c72d81245d1f1532043088e76ba63805339823e5b16389000000006a47304402204c108078b91ef1f6d2ce154b11bca8c31f6d37dac451a28c07edd7e737efef3802201ecfb09763d64d3ad293eec1c4ecaf0fd45b91dbdf428e422d282b98483300de4121036166800571f944768676842e4d2f8f96825c0f030139b6b78d6c9830de082828ffffffff09f9e15100000000001976a9143e0ea504169d4ef931e913cbbecb3f07b1d4b6f088acf9e15100000000001976a914229db1b4735321f46165ae5837e47dabd064f16e88acf9e15100000000001976a9144a5b03c7eea7b8e6e611559627a56963d514d1ea88ac6e6e5700000000001976a914c042299061557b60e0e5085bee8fadc8d7e5483388acf9e15100000000001976a914633d58a958c54d9858887b0f3aa65be4eb37f07488acf9e15100000000001976a9148d3cf51026f94d03fda5709160c7171b855ba22488ac50c84c00000000001976a9142a03a8943e47cdbd9ba448994e61d237e8d1ac4b88acf9e15100000000001976a914359f98091121e785e6663f10251832d9ae556f8588acf9e15100000000001976a91415a8feff23bfce20f837956c82e1eb1f2457f93488ac00000000")]
        public async Task SubmitTransaction_Test(string txRaw)
        {
            var mapiClientMock = new Mock<TaalClient>(_apiKey);

            mapiClientMock.Setup(x => x.SubmitTransaction(txRaw)).ReturnsAsync(() =>
            {
                const string json = @"{
                    ""payload"": ""{\""apiVersion\"":\""0.1.0\"",\""timestamp\"":\""2020-01-15T11:40:29.826Z\"",\""txid\"":\""6bdbcfab0526d30e8d68279f79dff61fb4026ace8b7b32789af016336e54f2f0\"",\""returnResult\"":\""success\"",\""resultDescription\"":\""\"",\""minerId\"":\""03fcfcfcd0841b0a6ed2057fa8ed404788de47ceb3390c53e79c4ecd1e05819031\"",\""currentHighestBlockHash\"":\""71a7374389afaec80fcabbbf08dcd82d392cf68c9a13fe29da1a0c853facef01\"",\""currentHighestBlockHeight\"":207,\""txSecondMempoolExpiry\"":0}
                    "",
                    ""signature"": ""3045022100f65ae83b20bc60e7a5f0e9c1bd9aceb2b26962ad0ee35472264e83e059f4b9be022010ca2334ff088d6e085eb3c2118306e61ec97781e8e1544e75224533dcc32379"",
                    ""publicKey"": ""03fcfcfcd0841b0a6ed2057fa8ed404788de47ceb3390c53e79c4ecd1e05819031"",
                    ""encoding"": ""UTF - 8"",
                    ""mimetype"": ""application / json""
                }";

                var response = JsonConvert.DeserializeObject<TransactionSubmitResponse>(json) ?? throw new ArgumentNullException();
                response.ProviderName = mapiClientMock.Object.Name;
                response.Payload = JsonConvert.DeserializeObject<TransactionSubmit>(response.JsonPayload) ?? throw new ArgumentNullException();
                response.ProviderId = response.Payload.MinerId;
                return Creator.CreateInstance<ApiResponse<TransactionSubmitResponse>>(response);
            });

            var response = await mapiClientMock.Object.SubmitTransaction(txRaw);
            Assert.NotNull(response);
            Assert.Equal("taal", response.Result.ProviderName);
            Assert.NotNull(response.Result.JsonPayload);

            var submit = response.Result.Payload;
            Assert.Equal(207, submit.CurrentHighestBlockHeight);
            Assert.Equal("71a7374389afaec80fcabbbf08dcd82d392cf68c9a13fe29da1a0c853facef01", submit.CurrentHighestBlockHash);
            Assert.Equal(DateTime.Parse("2020-01-15T11:40:29.826"), submit.Timestamp);
            Assert.True(new PublicKey(submit.MinerId).IsValid);
            Assert.Equal("success", submit.ReturnResult);
            Assert.Equal(string.Empty, submit.ResultDescription);
            Assert.Equal("6bdbcfab0526d30e8d68279f79dff61fb4026ace8b7b32789af016336e54f2f0", submit.TxId);
            Assert.Equal(0, submit.TxSecondMempoolExpiry); // e.g. Not enough fees

        }

        [Theory]
        [InlineData("0100000001747623f8e6f9b684c2c72d81245d1f1532043088e76ba63805339823e5b16389000000006a47304402204c108078b91ef1f6d2ce154b11bca8c31f6d37dac451a28c07edd7e737efef3802201ecfb09763d64d3ad293eec1c4ecaf0fd45b91dbdf428e422d282b98483300de4121036166800571f944768676842e4d2f8f96825c0f030139b6b78d6c9830de082828ffffffff09f9e15100000000001976a9143e0ea504169d4ef931e913cbbecb3f07b1d4b6f088acf9e15100000000001976a914229db1b4735321f46165ae5837e47dabd064f16e88acf9e15100000000001976a9144a5b03c7eea7b8e6e611559627a56963d514d1ea88ac6e6e5700000000001976a914c042299061557b60e0e5085bee8fadc8d7e5483388acf9e15100000000001976a914633d58a958c54d9858887b0f3aa65be4eb37f07488acf9e15100000000001976a9148d3cf51026f94d03fda5709160c7171b855ba22488ac50c84c00000000001976a9142a03a8943e47cdbd9ba448994e61d237e8d1ac4b88acf9e15100000000001976a914359f98091121e785e6663f10251832d9ae556f8588acf9e15100000000001976a91415a8feff23bfce20f837956c82e1eb1f2457f93488ac00000000")]
        public async Task SubmitTransaction_VerifyHeader_Test(string txRaw)
        {
            var response = await _taal.SubmitTransaction(txRaw);

            Assert.False(response.IsSuccessful);
            Assert.Equal(_apiKey, _taal.ApiKey);
            Assert.DoesNotContain("Authorization", _taal.Headers.Keys);
        }

        #endregion
    }
    }
