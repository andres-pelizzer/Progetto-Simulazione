namespace Client.Models;

public class RichiestaModel
{
  public int RichiestaId { get; set; }
  public DateTime DataRichiesta { get; set; }
  public DateOnly DataInizio { get; set; }
  public DateOnly DataFine { get; set; }
  public string Motivazione { get; set; } = null!;
  public string Stato { get; set; } = null!;

  public int CategoriaId { get; set; }
  public string Categoria { get; set; } = null!;

  // null finché la richiesta è "In attesa"
  public DateTime? DataValutazione { get; set; }
  public string? Valutatore { get; set; }
}