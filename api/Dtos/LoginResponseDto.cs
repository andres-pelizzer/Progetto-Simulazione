namespace Api.Dtos;

public class LoginResponseDto
{
    public string Token { get; set; } = "";
    public UtenteDto Utente { get; set; } = null!;
}