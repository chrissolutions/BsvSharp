using System;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record PaymailResponse
    {
        private readonly Lazy<bool> _lazyFunc;

        protected PaymailResponse()
            : this(true)
        {
        }

        protected PaymailResponse(bool successful = true)
        {
            _lazyFunc = new Lazy<bool>(() => successful);
            Exception = null;
        }

        protected PaymailResponse(Func<bool> successful)
        {
            _lazyFunc = new Lazy<bool>(() => successful == null || successful());
            Exception = null;
        }

        protected PaymailResponse(Exception ex)
        {
            _lazyFunc = new Lazy<bool>(() => false);
            Exception = ex;
        }

        public bool IsSuccessful => _lazyFunc.Value;

        public Exception Exception { get; private init; }
    }
}
