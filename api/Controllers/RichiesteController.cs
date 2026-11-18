using System.Security.Claims;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

// Tabella richieste (tutte le rotte richiedono il token)
//
// GET    /api/richieste               Dipendente: solo le sue | Responsabile: tutte
// GET    /api/richieste/da-approvare  Solo Responsabile. Richieste "In attesa"
// GET    /api/richieste/{id}          200 | 404 (inesistente o di un altro dipendente)
// POST   /api/richieste               201 | 400 date incoerenti o categoria inesistente
// PUT    /api/richieste/{id}          204 | 400 non "In attesa" o dati non validi | 404
// DELETE /api/richieste/{id}          204 | 400 | 404
//                                     Il Responsabile può eliminare anche le approvate
// PUT    /api/richieste/{id}/approva  Solo Responsabile. 204 | 400 non "In attesa" | 404
// PUT    /api/richieste/{id}/rifiuta  Solo Responsabile. 204 | 400 non "In attesa" | 404

[ApiController]
[Authorize]   // tutte le rotte richiedono il token
[Route("api/[controller]")]
public class RichiesteController : ControllerBase
{
    private readonly AppDbContext _context;

    public RichiesteController(AppDbContext context)
    {
        _context = context;
    }

    // Chi sta chiamando, letto dal token (claim "sub" scritto in CreaToken)
    private int UtenteCorrenteId => int.Parse(User.FindFirstValue("sub")!);
    private bool IsResponsabile => User.IsInRole("Responsabile");


    [HttpGet]
    public async Task<ActionResult<List<RichiestaDto>>> GetRichieste()
    {
        var query = _context.RichiestePermessos.AsQueryable();

        // Il dipendente vede solo le sue. La query parte solo con ToListAsync(),
        // quindi il Where condizionale finisce nello stesso SQL.
        if (!IsResponsabile)
            query = query.Where(r => r.UtenteId == UtenteCorrenteId);

        var richieste = await query.OrderByDescending(r => r.DataRichiesta).Select(r => new RichiestaDto
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
            Valutatore = r.UtenteValutazione == null ? null : r.UtenteValutazione.Nome + " " + r.UtenteValutazione.Cognome,
            ValutatoreId = r.UtenteValutazioneId
        }).ToListAsync();

        return Ok(richieste);
    }


    [HttpGet("da-approvare")]
    [Authorize(Roles = "Responsabile")]   // si somma a [Authorize] della classe: token E ruolo
    public async Task<ActionResult<List<RichiestaDto>>> GetRichiesteDaApprovare()
    {
        var richieste = await _context.RichiestePermessos.Where(r => r.Stato == "In attesa").OrderBy(r => r.DataRichiesta).Select(r => new RichiestaDto
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
            Richiedente = r.Utente.Nome + " " + r.Utente.Cognome
        }).ToListAsync();

        return Ok(richieste);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<RichiestaDto>> GetRichiesta(int id)
    {
        var query = _context.RichiestePermessos.Where(r => r.RichiestaId == id);   // filtro PRIMA del Select

        // Richiesta di un altro dipendente: 404, come se non esistesse
        if (!IsResponsabile)
            query = query.Where(r => r.UtenteId == UtenteCorrenteId);

        var richiesta = await query.Select(r => new RichiestaDto
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
        }).FirstOrDefaultAsync();   // esegue la query, restituisce 1 DTO o null

        if (richiesta == null)
            return NotFound();

        return Ok(richiesta);
    }


    [HttpPost]
    public async Task<ActionResult<RichiestaDto>> AddRichiesta(RichiestaInputDto dto)
    {
        // [Required] ha già garantito che i campi esistano, quindi il confronto è sicuro
        if (dto.DataFine < dto.DataInizio)
            return BadRequest("La data di fine non può precedere quella di inizio.");

        if (!await _context.CategoriePermessos.AnyAsync(c => c.CategoriaId == dto.CategoriaId))
            return BadRequest("Categoria non valida.");

        // Niente controllo sull'utente: l'id arriva dal token, non dal client
        var richiesta = new RichiestePermesso
        {
            UtenteId = UtenteCorrenteId,
            CategoriaId = dto.CategoriaId!.Value,
            DataInizio = dto.DataInizio!.Value,
            DataFine = dto.DataFine!.Value,
            Motivazione = dto.Motivazione.Trim(),
            DataRichiesta = DateTime.UtcNow,
            Stato = "In attesa"
        };

        _context.RichiestePermessos.Add(richiesta);
        await _context.SaveChangesAsync();

        // Rileggo con la proiezione: l'entità appena salvata ha Categoria e Utente a null
        // e, se la restituissi così, esporrebbe anche dati che non deve esporre.
        var creata = await _context.RichiestePermessos.Where(r => r.RichiestaId == richiesta.RichiestaId).Select(r => new RichiestaDto
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
        }).FirstAsync();

        return CreatedAtAction(nameof(GetRichiesta), new { id = richiesta.RichiestaId }, creata);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRichiesta(int id, RichiestaInputDto dto)
    {
        // Deve esistere ED essere di chi chiama (anche per il Responsabile)
        var esistente = await _context.RichiestePermessos
            .FirstOrDefaultAsync(r => r.RichiestaId == id && r.UtenteId == UtenteCorrenteId);

        if (esistente == null)
            return NotFound();

        if (esistente.Stato != "In attesa")
            return BadRequest("Si può modificare solo una richiesta in attesa.");

        if (dto.DataFine < dto.DataInizio)
            return BadRequest("La data di fine non può precedere quella di inizio.");

        // Senza questo controllo una categoria inesistente darebbe 500 (chiave esterna)
        if (!await _context.CategoriePermessos.AnyAsync(c => c.CategoriaId == dto.CategoriaId))
            return BadRequest("Categoria non valida.");

        esistente.DataInizio = dto.DataInizio!.Value;
        esistente.DataFine = dto.DataFine!.Value;
        esistente.Motivazione = dto.Motivazione.Trim();
        esistente.CategoriaId = dto.CategoriaId!.Value;

        await _context.SaveChangesAsync();
        return NoContent();   // 204: modificato, niente body
    }


    [HttpPut("{id:int}/approva")]
    [Authorize(Roles = "Responsabile")]
    public async Task<ActionResult> ApprovaRichiesta(int id)
    {
        var esistente = await _context.RichiestePermessos.FindAsync(id);
        if (esistente == null)
            return NotFound();

        if (esistente.Stato != "In attesa")
            return BadRequest("Non è possibile perché la richiesta non è in attesa");

        esistente.DataValutazione = DateTime.UtcNow;
        esistente.UtenteValutazioneId = UtenteCorrenteId;   // il responsabile, dal token
        esistente.Stato = "Approvato";

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPut("{id:int}/rifiuta")]
    [Authorize(Roles = "Responsabile")]
    public async Task<ActionResult> RifiutaRichiesta(int id)
    {
        var esistente = await _context.RichiestePermessos.FindAsync(id);
        if (esistente == null)
            return NotFound();

        if (esistente.Stato != "In attesa")
            return BadRequest("Non è possibile perché la richiesta non è in attesa");

        esistente.DataValutazione = DateTime.UtcNow;
        esistente.UtenteValutazioneId = UtenteCorrenteId;   // il responsabile, dal token
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

        if (!IsResponsabile)
        {
            // Dipendente: solo le sue e solo se ancora in attesa
            if (richiesta.UtenteId != UtenteCorrenteId)
                return NotFound();   // di un altro: come se non esistesse

            if (richiesta.Stato != "In attesa")
                return BadRequest("Si può eliminare solo una richiesta in attesa.");
        }
        // Il Responsabile arriva qui senza altri controlli: elimina anche le approvate

        _context.RichiestePermessos.Remove(richiesta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}