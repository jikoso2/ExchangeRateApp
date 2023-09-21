using ExchangeRateApp.Helpers;
using ExchangeRateApp.Models;
using ExchangeRateApp.NBPApi;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using System.Data;

namespace ExchangeRateApp.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CurrencyController : ControllerBase
	{
		protected readonly ILogger<CurrencyController> _logger;
		protected readonly AppDbContext _dbcontext;
		protected readonly INBPApiService _NBPApi;

		public CurrencyController(ILogger<CurrencyController> logger, AppDbContext dbContext, INBPApiService NBPApi)
		{
			_logger = logger;
			_dbcontext = dbContext;
			_NBPApi = NBPApi;
		}

		/// <summary>
		/// Returns all available currencies.
		/// </summary>
		/// <response code="200">OK</response>
		[HttpGet]
		public IActionResult GetAll()
		{
			return Ok(_dbcontext.Currency.OrderBy(a => a.Id));
		}

		/// <summary>
		/// Return currency.
		/// </summary>
		/// <param name="currencyId">Currency Id</param>
		/// <response code="200">OK</response>
		/// <response code="404">Currency with requested Id doesn't exist</response>
		[HttpGet("{id:int:required}")]
		public IActionResult GetCurrency([FromRoute(Name = "id")] int currencyId)
		{
			var entity = _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyId);

			if (entity == null)
				return StatusCode(StatusCodes.Status404NotFound, "Currency doesn't exist");
			else
				return Ok(entity);
		}

		/// <summary>
		/// Delete currency.
		/// </summary>
		/// <param name="currencyId">Currency Id</param>		
		/// <response code="200">OK</response>
		/// <response code="404">Currency with requested Id doesn't exist</response>
		[HttpDelete("{id:int:required}")]
		public IActionResult DeleteCurrency([FromRoute(Name = "id")] int currencyId)
		{
			var entity = _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyId);

			if (entity == null)
				return StatusCode(StatusCodes.Status404NotFound, "Currency doesn't exist");
			else
			{
				_dbcontext.Currency.Remove(entity);
				_dbcontext.SaveChanges();
				return Ok();
			}
		}

		/// <summary>
		/// Create currency.
		/// </summary>
		/// <response code="201">Currency has been successfully created</response>
		/// <response code="400">Bad request</response>
		[HttpPost]
		public IActionResult CreateCurrency([FromBody] Currency currencyDTO)
		{
			if (!CurrencyTools.TryGetCurrencySymbol(currencyDTO.Code))
				return StatusCode(StatusCodes.Status400BadRequest, "Currency code is wrong");

			if (_dbcontext.Currency.Any(a => a.Code == currencyDTO.Code))
				return StatusCode(StatusCodes.Status400BadRequest, $"Currency code ({currencyDTO.Code}) already exist");

			var result = _dbcontext.Currency.Add(currencyDTO);
			_dbcontext.SaveChanges();
			var entity = _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyDTO.Id);

			if (entity == null)
				return StatusCode(StatusCodes.Status400BadRequest, "Something went wrong");

			InsertCurrencyData(entity);

			return CreatedAtAction(nameof(CreateCurrency), _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyDTO.Id));
		}

		/// <summary>
		/// Update currency.
		/// </summary>
		/// <param name="currencyId">Currency Id</param>
		/// <param name="currencyDTO">Currency Data Model</param>
		/// <response code="200">Currency has been successfully updated</response>
		/// <response code="400">Bad request</response>
		/// <response code="404">Currency with requested Id doesn't exist</response>
		[HttpPut("{id:int:required}")]
		public IActionResult UpdateCurrency([FromRoute(Name = "id")] int currencyId, [FromBody] Currency currencyDTO)
		{
			if (!CurrencyTools.TryGetCurrencySymbol(currencyDTO.Code))
				return StatusCode(StatusCodes.Status400BadRequest, "Currency code is wrong");

			if (_dbcontext.Currency.Any(a => a.Code == currencyDTO.Code && a.Id != currencyId))
				return StatusCode(StatusCodes.Status400BadRequest, $"Currency code ({currencyDTO.Code}) already exist");

			var entity = _dbcontext.Currency.FirstOrDefault(a => a.Id == currencyId);

			if (entity == null)
				return StatusCode(StatusCodes.Status404NotFound, "Currency doesn't exist");

			if (entity.Code != currencyDTO.Code)
			{
				entity.Code = currencyDTO.Code;
				DeleteCurrencyData(entity);
				InsertCurrencyData(entity);
			}

			entity.Name = currencyDTO.Name;

			_dbcontext.SaveChanges();

			return Ok(entity);
		}

		private void InsertCurrencyData(Currency currency)
		{
			try
			{
				var fullData = _NBPApi.GetFullData(currency.Code);
				var insertedItems = fullData.Select(a => new Rate()
				{
					CurrencyId = currency.Id,
					RateDate = DateOnly.Parse(a.EffectiveDate ?? ""),
					Value = a.Mid
				});
				_dbcontext.Rate.AddRange(insertedItems);
				_dbcontext.SaveChanges();
			}
			catch (Exception ex)
			{
				var message = $"Something wen't wrong with import currency rate data. Details: {ex}";
				_logger.LogInformation("{Message}", message);
			}
		}

		private bool DeleteCurrencyData(Currency currency)
		{
			try
			{
				var entitiesToDelete = _dbcontext.Rate.Where(a => a.CurrencyId == currency.Id);
				_dbcontext.Rate.RemoveRange(entitiesToDelete);
				_dbcontext.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				var message = $"Something wen't wrong during deleting currency rate data. Details: {ex}";
				_logger.LogError("{Message}", message);
				throw new Exception(message);
			}
		}
	}
}