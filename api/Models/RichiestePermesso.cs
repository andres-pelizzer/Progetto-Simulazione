using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class RichiestePermesso
{
    public int RichiestaId { get; set; }

    public DateTime DataRichiesta { get; set; }

    public DateOnly DataInizio { get; set; }

    public DateOnly DataFine { get; set; }

    public int CategoriaId { get; set; }

    public string Motivazione { get; set; } = null!;

    public string Stato { get; set; } = null!;

    public int UtenteId { get; set; }

    public DateTime? DataValutazione { get; set; }

    public int? UtenteValutazioneId { get; set; }

    public virtual CategoriePermesso Categoria { get; set; } = null!;

    public virtual Utenti Utente { get; set; } = null!;

    public virtual Utenti? UtenteValutazione { get; set; }
}
