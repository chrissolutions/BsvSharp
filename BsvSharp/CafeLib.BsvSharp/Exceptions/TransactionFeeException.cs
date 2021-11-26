namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionFeeException : TransactionException
    {
        public TransactionFeeException(string message)
            : base(message)
        {
        }
    }
}