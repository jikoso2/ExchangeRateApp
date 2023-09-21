using ExchangeRateApp.Helpers;
using ExchangeRateApp.Models;
using ExchangeRateApp.NBPApi;

using Microsoft.AspNetCore.Mvc;

using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace ExchangeRateApp.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class RateController : ControllerBase
	{
		protected readonly AppDbContext _dbcontext;
		protected readonly INBPApiService _NBPApi;

		public RateController(AppDbContext dbContext, INBPApiService NBPApi)
		{
			_dbcontext = dbContext;
			_NBPApi = NBPApi;
		}

		/// <summary>
		/// Returns all rates.
		/// </summary>
		[HttpGet]
		[ProducesResponseType(typeof(List<Rate>), StatusCodes.Status200OK)]
		public IActionResult GetAll()
		{
			return Ok(_dbcontext.Rate);
		}

		/// <summary>
		/// Return rates per currency by id.
		/// </summary>
		/// <param name="currencyId">Currency Id</param>
		/// <param name="dateStart">Optional parameter, example: 2023-09-10</param>
		/// <param name="dateEnd">Optional parameter, example: 2023-09-18</param>
		/// <response code="404">Currency with requested Id doesn't exist</response>
		[HttpGet("byId/{id:int:required}")]
		[ProducesResponseType(typeof(List<Rate>), StatusCodes.Status200OK)]
		public IActionResult GetCurrencyRatesById([FromRoute(Name = "id")] int currencyId, DateOnly? dateStart, DateOnly? dateEnd)
		{
			var currency = _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyId);

			if (currency == null)
				return StatusCode(StatusCodes.Status404NotFound, "Currency doesn't exist");
			else
				return Ok(GetCurrencyRates(currency, dateStart, dateEnd));
		}

		/// <summary>
		/// Return rates per currency by code.
		/// </summary>
		/// <param name="code">Currency Code (ISO 4217)</param>
		/// <param name="dateStart">Optional parameter, example: 2023-09-10</param>
		/// <param name="dateEnd">Optional parameter, example: 2023-09-18</param>
		/// <response code="404">Currency with requested code doesn't exist</response>
		[HttpGet("byCode/{code:required}")]
		[ProducesResponseType(typeof(List<Rate>), StatusCodes.Status200OK)]
		public IActionResult GetCurrencyRatesByCode([FromRoute(Name = "code")] string code, DateOnly? dateStart, DateOnly? dateEnd)
		{
			var currency = _dbcontext.Currency.FirstOrDefault(a => a.Code == code);

			if (currency == null)
				return StatusCode(StatusCodes.Status404NotFound, "Currency doesn't exist");
			else
				return Ok(GetCurrencyRates(currency, dateStart, dateEnd));
		}

		private IQueryable<Rate> GetCurrencyRates(Currency currency, DateOnly? dateStart, DateOnly? dateEnd)
		{
			if (dateStart.HasValue && dateEnd.HasValue)
				return _dbcontext.Rate.Where(a => a.CurrencyId == currency.Id && a.RateDate >= dateStart.Value && a.RateDate <= dateEnd.Value);
			else if (dateStart.HasValue)
				return _dbcontext.Rate.Where(a => a.CurrencyId == currency.Id && a.RateDate >= dateStart.Value);
			else
				return _dbcontext.Rate.Where(a => a.CurrencyId == currency.Id);
		}
	};
}