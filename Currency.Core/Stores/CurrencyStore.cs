using CurrencyApp.EFCore.Context;
using CurrencyApp.EFCore.Exceptions;
using CurrencyApp.EFCore.Models;
using CurrencyApp.EFCore.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace CurrencyApp.EFCore.Stores;

public class CurrencyStore
{
    private readonly CurrencyDbContext context;

    public CurrencyStore(CurrencyDbContext context)
    {
        this.context = context;
    }

    public async Task<List<Currency>> GetAllCurrencies()
        => await context.Currencies.ToListAsync();

    public async Task<Currency> GetCurrency(int id)
    {
        var currency = await context.Currencies
                                    .Where(x => x.Id == id)
                                    .FirstOrDefaultAsync();

        if (currency == null) throw new RecordNotFoundException<Currency>(id);

        return currency;
    }
    public async Task<Currency?> GetCurrency(string currencyCode)
    {
        var currency = await context.Currencies
                                    .Where(x => x.CurrencyCode.ToLower() == currencyCode.ToLower())
                                    .FirstOrDefaultAsync();

        return currency;
    }

    public async Task CreateCurrency(CreateCurrency create)
    {
        // The only requirement here is that CurrencyCodes should be unique

        var existingCurrencyCode = await GetCurrency(create.CurrencyCode);

        if (existingCurrencyCode != null) throw new Exception("Currency Code must be unique!");

        await context.Currencies.AddAsync(Currency.CreateFromDto(create));
        await context.SaveChangesAsync();
    }

    public async Task UpdateCurrency(int id, UpdateCurrency update)
    {
        var currency = await GetCurrency(id);

        if (update.CurrencyCode != null) currency.CurrencyCode = update.CurrencyCode;
        if (update.Rate.HasValue) currency.Rate = update.Rate.Value;

        context.Update(currency);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCurrency(int id)
    {
        var currency = await GetCurrency(id);

        context.Currencies.Remove(currency);
        await context.SaveChangesAsync();
    }

    public async Task<decimal> ConvertCurrency(decimal amount, int fromCurrencyId, int toCurrencyId)
    {
        // Fetch queried records
        var from = await GetCurrency(fromCurrencyId);
        var to = await GetCurrency(toCurrencyId);

        // First, fetch all currency records from persistence
        var allCurrencies = await GetAllCurrencies();

        // Second, we need to create a graph of currencies, and how they relate to each other
        var graph = BuildCurrencyGraph(allCurrencies);

        // Third, prepare Dijkstra's algorithm to find the quickest route from currency -> to currency
        var visitedNodes = new HashSet<string>();
        var distance = new Dictionary<string, decimal>();

        foreach (var node in graph.Keys)
        {
            distance[node] = decimal.MaxValue;
        }
        distance[from.CurrencyCode] = 1m;

        // Search unvisited nodes only
        while (visitedNodes.Count < graph.Keys.Count)
        {
            string minNode = null;
            decimal minDist = decimal.MaxValue;

            // Basically, find the nearest node, then update our distance state to the node's nearest siblings or edges
            foreach (var node in graph.Keys)
            {
                if (!visitedNodes.Contains(node) && distance[node] < minDist)
                {
                    minNode = node;
                    minDist = distance[node];
                }
            }

            // We have visited every node
            if (minNode == null) break;

            // Mark this node as selected, and do the update
            visitedNodes.Add(minNode);
            foreach (var sibling in graph[minNode])
            {
                decimal alt = distance[minNode] * sibling.Value;
                if (alt < distance[sibling.Key])
                {
                    distance[sibling.Key] = alt;
                }
            }
        }

        // Use the selected exchange rate to calculate the amount
        var precise = amount * distance[to.CurrencyCode];

        // We will round up to the nearest fourth decimal point
        return Math.Round(precise, 4);
    }

    private Dictionary<string, Dictionary<string, decimal>> BuildCurrencyGraph(List<Currency> currencySource)
    {
        var currencyGraph = new Dictionary<string, Dictionary<string, decimal>>();

        foreach (var currency in currencySource)
        {
            if (!currencyGraph.ContainsKey(currency.CurrencyCode))
            {
                // Add the currency code as a root entry to the graph
                currencyGraph[currency.CurrencyCode] = new Dictionary<string, decimal>();
            }

            // It's own currency exchange rate is obviously just 1
            // E.g 100 USD = 100 USD
            currencyGraph[currency.CurrencyCode][currency.CurrencyCode] = 1m;

            foreach (var otherCurrency in currencySource.Where(x => x.CurrencyCode != currency.CurrencyCode))
            {
                // Now calculate each other currency's exchange rate, and at it to this currency's graph
                
                // Huge caveat here, these exchange rates revolve around whatever baseline rate is used
                // most commonly, USD. Trying to calculate Euros -> Japanese Yen, for example, probably won't be accurate to real-world, since both rates 
                // are based on how they convert to that baseline (USD)
                currencyGraph[currency.CurrencyCode][otherCurrency.CurrencyCode] = (decimal)currency.Rate / (decimal)otherCurrency.Rate;
            }
        }

        return currencyGraph;
    }
}
