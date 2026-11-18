// api/Dtos/RichiestaInputDto.cs
using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class RichiestaInputDto
{
  [Required] public DateOnly? DataInizio { get; set; }
  [Required] public DateOnly? DataFine { get; set; }
  [Required] public int? CategoriaId { get; set; }
  [Required] public string Motivazione { get; set; } = "";
}