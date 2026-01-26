using BilQalaam.Domain.Enums;

namespace BilQalaam.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertToUSDAsync(decimal amount, Currency fromCurrency);
        Task<decimal> ConvertToEGPAsync(decimal amount, Currency fromCurrency);
        Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency);
    }


}
