using System.Threading.Tasks;
using CafeLib.BsvSharp.Mapi.Responses;
using CafeLib.Web.Request;

namespace CafeLib.BsvSharp.Mapi 
{
    public interface IMerchantClient
    {
        Task<ApiResponse<FeeQuoteResponse>> GetFeeQuote();

        Task<ApiResponse<TransactionStatusResponse>> GetTransactionStatus(string txHash);

        Task<ApiResponse<TransactionSubmitResponse>> SubmitTransaction(string txRaw);
    }
}
