using System;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Response
    {
        public bool IsSuccessful { get; }

        public Exception Exception { get; }

        internal Response()
        {
            IsSuccessful = true;
        }

        internal Response(Exception exception)
        {
            IsSuccessful = false;
            Exception = exception;
        }

        public T GetException<T>() where T : Exception => (T) Exception;
    }
}
