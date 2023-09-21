using System.Globalization;

namespace ExchangeRateApp.Helpers
{
	public class Helper
	{
	}

	public static class DateTimeExtensions
	{
		public static DateTime? SetKindUtc(this DateTime? dateTime)
		{
			if (dateTime.HasValue)
			{
				return dateTime.Value.SetKindUtc();
			}
			else
			{
				return null;
			}
		}
		public static DateTime SetKindUtc(this DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Utc) { return dateTime; }
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
		}
	}

	public static class CurrencyTools
	{
		private static readonly IDictionary<string, string> map;

		static CurrencyTools()
		{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			map = CultureInfo
				.GetCultures(CultureTypes.AllCultures)
				.Where(c => !c.IsNeutralCulture)
				.Select(culture => {
					try
					{
						return new RegionInfo(culture.Name);
					}
					catch
					{
						return null;
					}
				})
				.Where(ri => ri != null)
				.GroupBy(ri => ri.ISOCurrencySymbol)
				.ToDictionary(x => x.Key, x => x.First().CurrencySymbol);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		}

		public static bool TryGetCurrencySymbol(string ISOCurrencySymbol)
		{
			return map.TryGetValue(ISOCurrencySymbol, out _);
		}
	}
}
