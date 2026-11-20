namespace Client.Models;

public class LoginModel
{
  public string Email { get; set; } = "";
  public string Password { get; set; } = "";
}

public class RegisterModel
{
  public string Nome { get; set; } = "";
  public string Cognome { get; set; } = "";
  public string Email { get; set; } = "";
  public string Password { get; set; } = "";
}