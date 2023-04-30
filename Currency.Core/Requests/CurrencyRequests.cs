using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyApp.EFCore.Requests;

/// <summary>
/// The Update Currency request coming from the client offers
/// optional fields to update different fields on the record.
/// </summary>
/// <param name="CurrencyCode"></param>
/// <param name="Rate"></param>
public record UpdateCurrency(
    string? CurrencyCode,
    double? Rate
)
{
    /// <summary>
    /// Provides default behavior for the model binder to construct this record from the HTTP Request
    /// </summary>
    [JsonConstructor]
    public UpdateCurrency() : this(default!, default!) { }
}


// The double required annotation is annoying, but they serve different purposes
// The Required Attribute provides information to model binders that the request body must include the field

// The required keyword is to help with the new C# nullable analyzer,
// we never have to worry about these fields being null on our object
public record CreateCurrency
{
    [Required]
    public required string CurrencyCode { get; set; }

    [Required]
    public required double Rate { get; set; }
}