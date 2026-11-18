using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class RegisterDto
{
    [Required, StringLength(50)] public string Nome { get; set; } = "";
    [Required, StringLength(50)] public string Cognome { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, StringLength(72, MinimumLength = 8)] public string Password { get; set; } = "";
}