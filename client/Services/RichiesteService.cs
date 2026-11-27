namespace Client.Services;

using System.Net.Http.Json;
using Client.Models;

public class RichiesteService
{
  private readonly HttpClient _httpClient;

  public RichiesteService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<List<RichiestaModel>> GetRichieste()
  {
    var risultato = await _httpClient.GetFromJsonAsync<List<RichiestaModel>>("api/richieste");
    return risultato ?? new List<RichiestaModel>();
  }
}