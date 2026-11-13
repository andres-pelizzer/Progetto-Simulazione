using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class Utenti
{
    public int UtenteId { get; set; }

    public string Nome { get; set; } = null!;

    public string Cognome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Ruolo { get; set; } = null!;

    public DateTime DataCreazione { get; set; }

    public virtual ICollection<RichiestePermesso> RichiestePermessoUtenteValutaziones { get; set; } = new List<RichiestePermesso>();

    public virtual ICollection<RichiestePermesso> RichiestePermessoUtentes { get; set; } = new List<RichiestePermesso>();
}

