using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Dtos;

namespace Api.Controllers;

[ApiController] // attiva comportamenti automatici , come la risposta 400 quando il body non rispetta le regole di validazione 
[Route("api/[controller]")]

// Tabella categorie

// GET /api/categorie
//     Restituisce tutte le categorie (ferie, visita medica, ...)
//     200 OK  perchè restituisce una lista di elementi, se è vuota restituisce 200 OK con una lista vuota




public class CategorieController : ControllerBase   // ControllerBase è la classe base che mi dà i metodi Ok(), NotFound(), BadRequest() 
{
    private readonly AppDbContext _context;

    public CategorieController(AppDbContext context)
    {
        _context = context;
    }


[HttpGet]
public async Task<ActionResult<List<CategoriaDto>>> GetCategorie()
{
    var categorie = await _context.CategoriePermessos.OrderBy(c=>c.Descrizione).Select(c=>new CategoriaDto {
        CategoriaId = c.CategoriaId,
        Descrizione = c.Descrizione
    } ).ToListAsync();
    return Ok(categorie);
}
}

