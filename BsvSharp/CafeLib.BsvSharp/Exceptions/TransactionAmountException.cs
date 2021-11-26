namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionAmountException : TransactionException
    {
        public TransactionAmountException(string message)
            : base(message)
        {
        }
    }
}