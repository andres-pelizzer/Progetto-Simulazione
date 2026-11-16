using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Dtos;
using Microsoft.AspNetCore.Authorization

namespace Api.Controllers;

// Tabella richieste

// GET    /api/richieste        Elenco di tutte le richieste
//        200 OK  lista di richieste o 200 Ok con lista vuota se non c'è nessuna richiesta
// GET    /api/richieste/{id}   Dettaglio di una richiesta
//        200 OK  richiesta | 404 Not Found  id inesistente

// POST   /api/richieste        Crea una richiesta 
//        201 Created  richiesta creata
//        400 Bad Request  date incoerenti, categoria o utente inesistenti

// PUT    /api/richieste/{id}   Modifica date, categoria e motivazione
//        200 OK  richiesta aggiornata, 204 invece [NoContent()]
//        400 Bad Request  richiesta non "In attesa" o dati non validi
//        404 Not Found  id inesistente

// DELETE /api/richieste/{id}   Elimina una richiesta
//        204 No Content  eliminata | 404 Not Found  id inesistente



[ApiController]
[Route("api/[controller]")]
public class RichiesteController : ControllerBase
{
    private readonly AppDbContext _context;

    public RichiesteController(AppDbContext context)
    {
        _context = context;
    }


[HttpGet]
public async Task<ActionResult<List<RichiestaDto>>> GetRichieste()
{
    var richieste = await _context.RichiestePermessos.OrderByDescending(r=>r.DataRichiesta).Select(r=> new RichiestaDto{
        RichiestaId = r.RichiestaId,
        DataRichiesta = r.DataRichiesta,
        DataInizio = r.DataInizio,
        DataFine = r.DataFine,
        Motivazione = r.Motivazione,
        Stato = r.Stato,
        CategoriaId = r.CategoriaId,
        Categoria= r.Categoria.Descrizione,
        UtenteId=r.UtenteId,
        Richiedente= r.Utente.Nome + " " + r.Utente.Cognome,
        DataValutazione = r.DataValutazione,
        Valutatore = r.UtenteValutazione==null? null : r.UtenteValutazione.Nome + " " + r.UtenteValutazione.Cognome,
        ValutatoreId =  r.UtenteValutazioneId

    }).ToListAsync();
    return Ok(richieste);
}



[HttpGet("da-approvare")]
[Authorize(Roles = "Responsabile")]

public async Task<ActionResult<List<RichiestaDto>>> GetRichiesteDaApprovare(){
    var richieste = await _context.RichiestePermessos.OrderBy(r=>r.DataRichiesta).Where(r=> r.Stato == "In attesa").Select(r=> new RichiestaDto{
        RichiestaId = r.RichiestaId,
        DataRichiesta = r.DataRichiesta,
        DataInizio = r.DataInizio,
        DataFine = r.DataFine,
        Motivazione = r.Motivazione,
        Stato = r.Stato,
        CategoriaId = r.CategoriaId,
        Categoria= r.Categoria.Descrizione,
        UtenteId=r.UtenteId,
        Richiedente= r.Utente.Nome + " " + r.Utente.Cognome
       

    }).ToListAsync();
    return Ok(richieste);
}



[HttpGet("{id:int}")]

public async Task<ActionResult<RichiestaDto>> GetRichiesta(int id)
{
    var richiesta = await _context.RichiestePermessos
        .Where(r => r.RichiestaId == id)          // filtro PRIMA del Select
        .Select(r => new RichiestaDto
        {
            RichiestaId = r.RichiestaId,
            DataRichiesta = r.DataRichiesta,
            DataInizio = r.DataInizio,
            DataFine = r.DataFine,
            Motivazione = r.Motivazione,
            Stato = r.Stato,
            CategoriaId = r.CategoriaId,
            Categoria = r.Categoria.Descrizione,
            UtenteId = r.UtenteId,
            Richiedente = r.Utente.Nome + " " + r.Utente.Cognome,
            DataValutazione = r.DataValutazione,
            Valutatore = r.UtenteValutazione == null
                ? null
                : r.UtenteValutazione.Nome + " " + r.UtenteValutazione.Cognome,
            ValutatoreId = r.UtenteValutazioneId
        })
        .FirstOrDefaultAsync();                   // esegue la query, restituisce 1 DTO o null  

    if (richiesta == null)
        return NotFound();

    return Ok(richiesta);
}






[HttpPost]
public async Task<ActionResult<RichiestePermesso>> AddRichiesta(RichiestePermesso richiesta)
{
    if (richiesta.DataFine < richiesta.DataInizio)
        return BadRequest("La data di fine non può precedere quella di inizio.");

    if (!await _context.CategoriePermessos.AnyAsync(c => c.CategoriaId == richiesta.CategoriaId))
        return BadRequest("Categoria non valida.");

    if (!await _context.Utentis.AnyAsync(u => u.UtenteId == richiesta.UtenteId))
        return BadRequest("Utente non valido.");

    richiesta.Stato = "In attesa";
    richiesta.DataRichiesta = DateTime.UtcNow;
    richiesta.DataValutazione = null;
    richiesta.UtenteValutazioneId = null;

    _context.RichiestePermessos.Add(richiesta);
    await _context.SaveChangesAsync();

   return CreatedAtAction(nameof(GetRichiesta), new { id = richiesta.RichiestaId }, richiesta);
}


[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateRichiesta(int id, RichiestePermesso richiesta)
{
    var esistente = await _context.RichiestePermessos.FindAsync(id);
    if (esistente == null)
        return NotFound();

    
    if (esistente.Stato != "In attesa")
        return BadRequest("Si può modificare solo una richiesta in attesa.");

   
    if (richiesta.DataFine < richiesta.DataInizio)
        return BadRequest("La data di fine non può precedere quella di inizio.");

   
    esistente.DataInizio = richiesta.DataInizio;
    esistente.DataFine = richiesta.DataFine;
    esistente.Motivazione = richiesta.Motivazione;
    esistente.CategoriaId = richiesta.CategoriaId;   // permette il cambio categoria

    await _context.SaveChangesAsync();
    return NoContent();   // 204: modificato, niente body
}

[HttpPut("{id:int}/approva")]
[Authorize(Roles = "Responsabile")]

public async Task<ActionResult> ApprovaRichiesta (int id ){
    var esistente = await _context.RichiestePermessos.FindAsync(id);
    if(esistente==null){
        return NotFound();
    }
    if(esistente.Stato!=="In attesa"){
       return BadRequest("Non è possibile perché la richiesta non è in attesa")
    }
    esistente.DataValutazione = DateTime.UtcNow;
    esistente.UtenteValutazioneId=// arriva dal token

    esistente.Stato = "Approvato";
    await _context.SaveChangesAsync();

    return NoContent();
}


[HttpPut("{id:int}/rifiuta")]
[Authorize(Roles = "Responsabile")]

public async Task<ActionResult> RifiutaRichiesta (int id ){
    var esistente = await _context.RichiestePermessos.FindAsync(id);
     if(esistente==null){
        return NotFound();
    }
      if(esistente.Stato!=="In attesa"){
       return BadRequest("Non è possibile perché la richiesta non è in attesa")
    }
    esistente.DataValutazione = DateTime.UtcNow;
    esistente.UtenteValutazioneId=// arriva dal token

    esistente.Stato = "Rifiutato";
    await _context.SaveChangesAsync();

    return NoContent();

}


[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteRichiesta(int id)
{
    var richiesta = await _context.RichiestePermessos.FindAsync(id);
    if (richiesta == null)
        return NotFound();

    
    if (richiesta.Stato != "In attesa")
        return BadRequest("Si può eliminare solo una richiesta in attesa.");

    _context.RichiestePermessos.Remove(richiesta);
    await _context.SaveChangesAsync();
    return NoContent();
}

}

