using Cronos;
using ExchangeRateApp.Controllers;
using ExchangeRateApp.Models;
using ExchangeRateApp.NBPApi;

namespace ExchangeRateApp.Services
{
	public class PeriodicUpdateRateService : BackgroundService
	{
		private const string schedule = "0 0 2,23 * * *"; // every day at 2 am and 11 pm
		private readonly CronExpression _cron;
		private readonly IServiceScopeFactory _serviceFactory;
		protected readonly ILogger<PeriodicUpdateRateService> _logger;

		public PeriodicUpdateRateService(IServiceScopeFactory serviceFactory, ILogger<PeriodicUpdateRateService> logger)
		{
			_cron = CronExpression.Parse(schedule, CronFormat.IncludeSeconds);
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var utcNow = DateTime.UtcNow;
				var nextUtc = _cron.GetNextOccurrence(utcNow) ?? DateTime.Now.AddDays(1);
				await Task.Delay(nextUtc - utcNow, stoppingToken);
				await DailyUpdateData();
			}
		}

		private Task DailyUpdateData()
		{
			using (var scope = _serviceFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
				var apiService = scope.ServiceProvider.GetService<INBPApiService>();

				if (dbContext != null && apiService != null)
				{
					try
					{
						var dailyData = apiService.GetCurrentData();
						var currencies = dbContext.Currency.ToList();

						foreach (var currency in currencies)
						{
							var dateTimeNow = DateTime.Now.ToString("yyyy-M-d");
							var rates = dbContext.Rate.Where(a => a.CurrencyId == currency.Id).ToList();

							if (!rates.Where(a=>a.RateDate.ToString("yyyy-M-d") == dateTimeNow).Any() && dailyData != null)
							{
								var newValue = dailyData.Rates.FirstOrDefault(a => a.Code == currency.Code)?.Mid;
								if (newValue == null)
									continue;
								dbContext.Rate.Add(new Rate() { CurrencyId = currency.Id, RateDate = DateOnly.Parse(dateTimeNow), Value = (float)newValue });
								dbContext.SaveChanges();
							}
						}
					}
					catch (Exception ex)
					{
						var message = $"Something wen't wrong during deleting currency rate data. Details: {ex}";
						_logger.LogError("{Message}",message);
					}
				}
			}
			return Task.FromResult("Done");

		}
	}
}
