using CafeLib.Web.Request;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Health
    {
        public Health()
        {
            IsSuccessful = true;
            StatusCode = 200;
        }

        public Health(WebRequestException e)
        {
            IsSuccessful = false;
            StatusCode = e.Response.StatusCode;
            ErrorMessage = e.Response.ReasonPhrase;
        }

        public bool IsSuccessful { get; }

        public int StatusCode { get; }

        public string ErrorMessage { get; } = "";
    }
}
