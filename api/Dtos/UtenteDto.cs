namespace Api.Dtos;


public class UtenteDto
{
    public int UtenteId { get; set; }
    public string Nome { get; set; } = "";
    public string Cognome { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DataCreazione { get; set; }
}