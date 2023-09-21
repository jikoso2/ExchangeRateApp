using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ExchangeRateApp.Models
{
	[Table("currency")]
	public class Currency
	{
		[Key]
		[Required]
		[JsonIgnore]
		[Column("id")]
		public int Id { get; set; }

		[DataMember]
		[Required]
		[MaxLength(50)]
		[Column("name")]
		public required string Name { get; set; }

		[DataMember]
		[Required]
		[MaxLength(3)]
		[Column("code")]
		public required string Code { get; set; }

	}
}
