using System;

namespace CafeLib.BsvSharp.Mapi
{
    public class MerchantClientException<T> : Exception
    {
        public T Result { get; }

        public MerchantClientException(string message)
            : base(message)
        {
        }

        public MerchantClientException(T result, string message)
            : base(message)
        {
            Result = result;
        }
    }
}
