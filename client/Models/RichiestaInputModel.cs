namespace Client.Models;

public class RichiestaInputModel
{
  public DateTime? DataInizio { get; set; }
  public DateTime? DataFine { get; set; }
  public int? CategoriaId { get; set; }
  public string Motivazione { get; set; } = "";
}