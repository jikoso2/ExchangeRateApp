using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ExchangeRateApp.Models
{
	[Table("rate")]
	public class Rate
	{
		[Key]
		[DataMember]
		[Required]
		[Column("id")]
		public int Id { get; set; }

		[DataMember]
		[Required]
		[Column("ratedate")]
		public DateOnly RateDate { get; set; }

		[DataMember]
		[Required]
		[Column("currency_id")]
		public int CurrencyId { get; set; }

		[DataMember]
		[Required]
		[Column("value")]
		public float Value { get; set; }

	}
}
