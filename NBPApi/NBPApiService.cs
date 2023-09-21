using System.Text.Json;
using static ExchangeRateApp.NBPApi.NBPContracts;

namespace ExchangeRateApp.NBPApi
{
	public interface INBPApiService
	{
		public List<GetExchangeRatesResponse.Rate> GetFullData(string currencyCode);
		public GetExchangeRatesTableResponse? GetCurrentData();
	}

	public class NBPApiService : INBPApiService
	{
		public NBPApiService(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory;
		}

		private readonly IHttpClientFactory _httpClient;

		#region const
		private const string ExchangeRates = "exchangerates";
		private const string Rates = "rates";
		private const string Tables = "tables";
		private readonly DateTime StartDate = DateTime.Parse("2002-01-02");
		#endregion

		public enum RequestNBPTableType
		{
			A,
			B,
			C
		}

		public static T DeserializeJson<T>(string input)
		{
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var result = JsonSerializer.Deserialize<T>(input, options);
			if (result != null)
				return result;
			else
				throw new Exception($"Error while deserializing: {result}");
		}

		public List<GetExchangeRatesResponse.Rate> GetFullData(string currencyCode)
		{
			var httpClient = _httpClient.CreateClient("NBPApi");
			var result = new List<GetExchangeRatesResponse.Rate>();
			var startDate = StartDate;

			do
			{
				if (startDate.Year == DateTime.Now.Year)
				{
					var json = httpClient.GetAsync(Path.Combine(ExchangeRates, Rates, RequestNBPTableType.A.ToString(), currencyCode, startDate.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"))).Result.Content.ReadAsStringAsync().Result;
					var items = DeserializeJson<GetExchangeRatesResponse>(json).Rates.ToList();
					result.AddRange(items);
				}
				else
				{
					var json = httpClient.GetAsync(Path.Combine(ExchangeRates, Rates, RequestNBPTableType.A.ToString(), currencyCode, startDate.ToString("yyyy-MM-dd"), startDate.AddYears(1).ToString("yyyy-MM-dd"))).Result.Content.ReadAsStringAsync().Result;
					var items = DeserializeJson<GetExchangeRatesResponse>(json).Rates.ToList();
					result.AddRange(items);
				}
				startDate = startDate.AddYears(1);
			}
			while(startDate.Year != DateTime.Now.Year + 1);

			return result;
		}

		public GetExchangeRatesTableResponse? GetCurrentData()
		{
			var httpClient = _httpClient.CreateClient("NBPApi");

			var json = httpClient.GetAsync(Path.Combine(ExchangeRates, Tables, RequestNBPTableType.A.ToString())).Result.Content.ReadAsStringAsync().Result;

			return DeserializeJson<GetExchangeRatesTableResponse[]>(json).FirstOrDefault();
		}
	}
}
