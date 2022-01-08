using CafeLib.Web.Request;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Health
    {
        public Health(string message)
        {
            IsSuccessful = true;
            StatusCode = 200;
            Message = message;
        }

        public Health(WebRequestException e)
        {
            IsSuccessful = false;
            StatusCode = e.Response.StatusCode;
            ErrorMessage = e.Response.ReasonPhrase;
        }

        public bool IsSuccessful { get; }

        public string Message { get; }

        public int StatusCode { get; }

        public string ErrorMessage { get; } = "";
    }
}
