using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Dtos;

namespace Api.Controllers;

// Tabella utenti

// POST /api/utenti/register   Registrazione (sempre con ruolo Dipendente)
//      201 Created  utente creato (senza password)
//      400 Bad Request  dati non validi (campi mancanti, email non valida,
//                       password sotto gli 8 caratteri)
//      409 Conflict  quando si entra in conflitto con richieste del DB

     // api/utenti/login   


[ApiController]
[Route("api/[controller]")]

public class UtentiController : ControllerBase{

   private readonly AppDbContext _context;

   public UtentiController(AppDbContext context){
    _context=context;
   }


[HttpPost("login")]

public async Task<ActionResult<UtenteDto>> Login (LoginDto dto){

   var email = dto.Email.Trim().ToLowerInvariant();
   var utente = await _context.Utentis.FirstOrDefaultAsync(u=>u.Email == email);

   if(utente == null || !BCrypt.Net.BCrypt.Verify(dto.Password, utente.PasswordHash)) {
      return Unauthorized("Credenziali non valide");
   }
   return Ok(new UtenteDto{
      UtenteId= utente.UtenteId,
      Nome = utente.Nome,
      Cognome = utente.Cognome,
      Email = email,
      DataCreazione = utente.DataCreazione
   });
}


[HttpPost("register")]

public async Task<ActionResult<UtenteDto>> AddUtente(RegisterDto dto){

   var email = dto.Email.Trim().ToLowerInvariant();


  if(await _context.Utentis.AnyAsync(u=>u.Email == email)){
   return Conflict("Email già registrata");
  }
  

   var ut = new Utenti {
      Email = email,
      Nome = dto.Nome,
      Cognome = dto.Cognome,
      DataCreazione = DateTime.UtcNow,
      Ruolo = "Dipendente",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)

   };

   _context.Utentis.Add(ut);
   await _context.SaveChangesAsync();

   return StatusCode(StatusCodes.Status201Created, new UtenteDto{
    UtenteId = ut.UtenteId,
    Nome = ut.Nome,
    Cognome = ut.Cognome,
    Email = ut.Email,
    DataCreazione = ut.DataCreazione

   });
}

private string CreaToken(Utenti utente)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, utente.UtenteId.ToString()), // chi è
        new Claim(ClaimTypes.Role, utente.Ruolo)                          // cosa può fare
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
    return new JwtSecurityTokenHandler().WriteToken(token);
}


}
