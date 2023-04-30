namespace CurrencyApp.EFCore.Exceptions;

public class RecordNotFoundException<TRecord> : Exception where TRecord : class
{
    public RecordNotFoundException(object id)
        : base($"No {typeof(TRecord).FullName} record found with ID: {id}.") { }
}
