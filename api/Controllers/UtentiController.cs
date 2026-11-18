using System.Security.Claims;
using System.Text;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UtentiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;   // serve per leggere Jwt:Key da appsettings.json

    public UtentiController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var utente = await _context.Utentis.FirstOrDefaultAsync(u => u.Email == email);

        // Stesso messaggio se l'email non esiste o la password è sbagliata:
        // così non si scopre quali email sono registrate.
        if (utente == null || !BCrypt.Net.BCrypt.Verify(dto.Password, utente.PasswordHash))
            return Unauthorized("Credenziali non valide");

        return Ok(new LoginResponseDto { Token = CreaToken(utente), Utente = ToDto(utente) });
    }

    [HttpPost("register")]
    public async Task<ActionResult<UtenteDto>> AddUtente(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Utentis.AnyAsync(u => u.Email == email))
            return Conflict("Email già registrata");

        var ut = new Utenti
        {
            Email = email,
            Nome = dto.Nome,
            Cognome = dto.Cognome,
            DataCreazione = DateTime.UtcNow,
            Ruolo = "Dipendente",   // mai dal client
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Utentis.Add(ut);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToDto(ut));
    }

    // Rotta protetta di prova: se risponde, il token funziona.
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UtenteDto>> Me()
    {
        var id = int.Parse(User.FindFirstValue("sub")!);
        var ut = await _context.Utentis.FindAsync(id);
        if (ut == null) return Unauthorized();
        return Ok(ToDto(ut));
    }

    private string CreaToken(Utenti utente)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", utente.UtenteId.ToString()),  // chi è
                new Claim("role", utente.Ruolo)                // cosa può fare
            }),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static UtenteDto ToDto(Utenti u) => new()
    {
        UtenteId = u.UtenteId,
        Nome = u.Nome,
        Cognome = u.Cognome,
        Email = u.Email,
        Ruolo = u.Ruolo,
        DataCreazione = u.DataCreazione
    };
}



