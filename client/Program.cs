using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Blazored.LocalStorage;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var apiUrl = "http://localhost:5238";   // indirizzo dell'API: da cambiare al deploy

builder.Services.AddBlazoredLocalStorage();

// HttpClient con base address dell'API e handler che aggiunge il token
builder.Services.AddScoped(sp => new HttpClient(
    new JwtHandler(sp.GetRequiredService<ILocalStorageService>()) { InnerHandler = new HttpClientHandler() })
{
  BaseAddress = new Uri(apiUrl)
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddRadzenComponents();


await builder.Build().RunAsync();
