namespace CafeLib.BsvSharp.Units
{
    public interface IAmountExchangeRate 
    {
        decimal ConvertFromAmount(Amount value);

        Amount ConvertToAmount(decimal value);
        //public DateTime AsOfWhen { get; }
        //public void UpdateExchangeRate();
    }
}