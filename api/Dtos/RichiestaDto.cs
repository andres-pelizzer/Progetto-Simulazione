namespace Api.Dtos;

public class RichiestaDto{
  public int RichiestaId {get;set;}
  public DateTime DataRichiesta {get;set;}
  public DateOnly DataInizio {get;set;}
  public DateOnly DataFine {get;set;}
  public string Motivazione {get;set;}
  public string Stato {get;set;}

  public int CategoriaId{get;set;}
  public string Categoria {get;set;}

  public int UtenteId {get;set;}
  public string Richiedente {get;set;}

  public DateTime? DataValutazione {get;set;}
  public string Valutatore {get;set;}
  public int? ValutatoreId{get;set;}

}

