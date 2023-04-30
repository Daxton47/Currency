using CurrencyApp.EFCore.Requests;

namespace CurrencyApp.EFCore.Models;

public class Currency
{
    /// <summary>
    /// The Unique Identifier assigned by the database
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The region this currency represents
    /// </summary>
    public required string CurrencyCode { get; set; }

    /// <summary>
    /// The rate, relative to the baseline USD
    /// </summary>
    public required double Rate { get; set; }

    /// <summary>
    /// Convenience factory method, ideally would be dealt with by AutoMapper, 
    /// but that seems a little over-kill 🙂
    /// </summary>
    /// <param name="create"></param>
    /// <returns></returns>
    public static Currency CreateFromDto(CreateCurrency create)
    {
        return new Currency
        {
            CurrencyCode = create.CurrencyCode,
            Rate = create.Rate
        };
    }
}
