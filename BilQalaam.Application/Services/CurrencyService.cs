using BilQalaam.Application.Interfaces;
using BilQalaam.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace BilQalaam.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "ExchangeRates_USD";

        // Fallback rates relative to 1 USD
        private static readonly Dictionary<Currency, decimal> _fallbackRates = new()
        {
            { Currency.USD, 1.0m },
            { Currency.EGP, 0.020m },
            { Currency.SAR, 0.266m },
            { Currency.AED, 0.272m },
            { Currency.KWD, 3.25m },
            { Currency.QAR, 0.274m },
            { Currency.EUR, 1.08m },
            { Currency.GBP, 1.27m }
        };

        public CurrencyService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        private async Task<Dictionary<Currency, decimal>> GetRatesAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out Dictionary<Currency, decimal> rates))
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>("https://open.er-api.com/v6/latest/USD");
                    if (response?.rates != null)
                    {
                        rates = new Dictionary<Currency, decimal>();
                        foreach (var enumValue in Enum.GetValues<Currency>())
                        {
                            var code = enumValue.ToString();
                            if (response.rates.TryGetValue(code, out var rate))
                            {
                                // The API gives X currency per 1 USD (e.g., 50 EGP per 1 USD)
                                // We want 1 EGP = X USD (e.g., 1 EGP = 0.02 USD)
                                rates[enumValue] = 1.0m / rate;
                            }
                        }
                        
                        _cache.Set(CacheKey, rates, TimeSpan.FromHours(1));
                    }
                }
                catch
                {
                    // If API fails, we'll try to use fallback later
                }
            }

            return rates;
        }

        public async Task<decimal> ConvertToUSDAsync(decimal amount, Currency fromCurrency)
        {
            if (fromCurrency == Currency.USD) return Math.Round(amount, 3);

            var rates = await GetRatesAsync();
            if (rates != null && rates.TryGetValue(fromCurrency, out var rate))
            {
                return Math.Round(amount * rate, 3);
            }

            // Use Fallback
            if (_fallbackRates.TryGetValue(fromCurrency, out var fallbackRate))
            {
                return Math.Round(amount * fallbackRate, 3);
            }

            return Math.Round(amount, 3);
        }

        public async Task<decimal> ConvertToEGPAsync(decimal amount, Currency fromCurrency)
        {
            return await ConvertAsync(amount, fromCurrency, Currency.EGP);
        }

        public async Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency == toCurrency) return Math.Round(amount, 3);

            var amountInUSD = await ConvertToUSDAsync(amount, fromCurrency);
            if (toCurrency == Currency.USD) return amountInUSD;

            var rates = await GetRatesAsync();
            if (rates != null && rates.TryGetValue(toCurrency, out var rateToUSD))
            {
                // rateToUSD matches 1 unit of toCurrency = X USD
                // so amountInUSD / rateToUSD = amount in toCurrency
                return Math.Round(amountInUSD / rateToUSD, 3);
            }

            // Fallback
            if (_fallbackRates.TryGetValue(toCurrency, out var fallbackRateToUSD))
            {
                return Math.Round(amountInUSD / fallbackRateToUSD, 3);
            }

            return Math.Round(amountInUSD, 3);
        }

        private class ExchangeRateResponse
        {
            public Dictionary<string, decimal> rates { get; set; }
        }
    }
}


