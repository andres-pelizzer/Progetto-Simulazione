using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Services;

// Blazor chiama GetAuthenticationStateAsync per sapere chi è l'utente e che ruolo ha.
// Il token è nel localStorage; qui ne leggo il payload 
public class JwtAuthStateProvider : AuthenticationStateProvider
{
  private readonly ILocalStorageService _storage;

  public JwtAuthStateProvider(ILocalStorageService storage)
  {
    _storage = storage;
  }

  public override async Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    var token = await _storage.GetItemAsStringAsync("token");
    if (string.IsNullOrEmpty(token))
      return Anonimo();

    var claims = LeggiClaims(token);

    // "exp" è un timestamp unix in secondi: se è passato, il token è scaduto
    var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
    if (exp != null && DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) < DateTimeOffset.UtcNow)
    {
      await _storage.RemoveItemAsync("token");
      return Anonimo();
    }

    // "sub" = nome, "role" = ruolo: sono i nomi scritti dall'API in CreaToken
    var identity = new ClaimsIdentity(claims, "jwt", "sub", "role");
    return new AuthenticationState(new ClaimsPrincipal(identity));
  }

  public async Task Login(string token)
  {
    await _storage.SetItemAsStringAsync("token", token); // qui salva il token nel local storage
    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); // avvisa Blazor che lo stato di login è cambiato
  }

  public async Task Logout()
  {
    await _storage.RemoveItemAsync("token");
    NotifyAuthenticationStateChanged(Task.FromResult(Anonimo()));
  }

  private static AuthenticationState Anonimo() =>
      new(new ClaimsPrincipal(new ClaimsIdentity()));

  // Il payload è JSON in base64url: lo converto in base64 normale, decodifico e leggo le chiavi
  private static List<Claim> LeggiClaims(string token)
  {
    var payload = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
    payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
    var dati = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Convert.FromBase64String(payload))!;
    return dati.Select(kv => new Claim(kv.Key, kv.Value.ToString())).ToList();
  }
}