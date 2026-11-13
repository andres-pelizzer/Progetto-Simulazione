using System;
using System.Collections.Generic;

namespace Api.Models;

public partial class CategoriePermesso
{
    public int CategoriaId { get; set; }

    public string Descrizione { get; set; } = null!;

    public virtual ICollection<RichiestePermesso> RichiestePermessos { get; set; } = new List<RichiestePermesso>();
}
