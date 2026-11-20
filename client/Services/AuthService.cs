using System.Net;
using System.Net.Http.Json;

namespace Client.Services;

public class AuthService
{
  private readonly HttpClient _http;
  private readonly JwtAuthStateProvider _provider;

  public AuthService(HttpClient http, JwtAuthStateProvider provider)
  {
    _http = http;
    _provider = provider;
  }

  private record LoginRisposta(string Token);   // dalla risposta dell'API mi serve solo il token

  public async Task<string?> LoginAsync(string email, string password)
  {
    var resp = await _http.PostAsJsonAsync("api/utenti/login", new { email, password });

    if (resp.StatusCode == HttpStatusCode.Unauthorized) return "Credenziali non valide";
    if (!resp.IsSuccessStatusCode) return "Errore durante il login";

    var dati = await resp.Content.ReadFromJsonAsync<LoginRisposta>();
    await _provider.Login(dati!.Token);   // salva il token e aggiorna lo stato di login
    return null;
  }

  public async Task<string?> RegisterAsync(string nome, string cognome, string email, string password)
  {
    var resp = await _http.PostAsJsonAsync("api/utenti/register", new { nome, cognome, email, password });

    if (resp.StatusCode == HttpStatusCode.Conflict) return "Email già registrata";
    if (resp.StatusCode == HttpStatusCode.BadRequest) return "Dati non validi";
    if (!resp.IsSuccessStatusCode) return "Errore durante la registrazione";
    return null;
  }

  public Task LogoutAsync() => _provider.Logout();
}