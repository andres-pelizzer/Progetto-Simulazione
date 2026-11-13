using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

// Tabella utenti

// POST /api/utenti/register   Registrazione (sempre con ruolo Dipendente)
//      201 Created  utente creato (senza password)
//      400 Bad Request  dati non validi (campi mancanti, email non valida,
//                       password sotto gli 8 caratteri)
//      409 Conflict  quando si entra in conflitto con richieste del DB

[ApiController]
[Route("api/[controller]")]

public class UtentiController : ControllerBase{

   private readonly AppDbContext _context;

   public UtentiController(AppDbContext context){
    _context=context;
   }

//  [HttpPost("register")]

//  public async Task<ActionResult<Utenti>> AddUtente(Utenti utente){



//   if(!await _context.Utentis.AnyAsync(u=>u.email == utente.email)){
//     return Conflict();
//   }


//   utente.DataCreazione = DateTime.UtcNow;
//   utente.RichiestePermessoUtenteValutaziones=[];
//   utente.RichiestePermessoUtentes=[];
//    _context.Utentis.Add(utente);
//   await _context.SaveChangesAsync();
//   return null;

//  }
 
}
