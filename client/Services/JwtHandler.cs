using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Client.Services;

// Ogni richiesta verso l'API parte con "Authorization: Bearer <token>"
public class JwtHandler : DelegatingHandler
{
  private readonly ILocalStorageService _storage;

  public JwtHandler(ILocalStorageService storage)
  {
    _storage = storage;
  }

  protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, CancellationToken ct)
  {
    var token = await _storage.GetItemAsStringAsync("token", ct);
    if (!string.IsNullOrEmpty(token))
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    return await base.SendAsync(request, ct);
  }
}