namespace ExchangeRateApp.NBPApi
{
	public class NBPContracts
	{
		public class GetExchangeRatesResponse
		{
			public string? Code { get; set; }
			public Rate[] Rates { get; set; } = Array.Empty<Rate>();

			public class Rate
			{
				public string? EffectiveDate { get; set; }
				public float Mid { get; set; }
			}
		}

		public class GetExchangeRatesTableResponse
		{
			public string? Table { get; set; }

			public string? EffectiveDate { get; set; }

			public Rate[] Rates { get; set; } = Array.Empty<Rate>();

			public class Rate
			{
				public string? Code { get; set; }

				public float Mid { get; set; }
			}
		}
	}
}
