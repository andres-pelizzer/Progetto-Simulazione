using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Blazored.LocalStorage;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);   // prepara l'app che gira nel browser
builder.RootComponents.Add<App>("#app");   // dice che il componente App va disegnato dentro l'elemento <div id="app"> di index.html
builder.RootComponents.Add<HeadOutlet>("head::after");
// questo serve ai componenti per modificare il <head>


var apiUrl = "http://localhost:5238";   // indirizzo dell'API

builder.Services.AddBlazoredLocalStorage(); // questo registra ILocalStorageService, l'oggetto che legge e scrive nel localStorage del browser

// HttpClient con base address dell'API e handler che aggiunge il token
builder.Services.AddScoped(sp => new HttpClient(
    new JwtHandler(sp.GetRequiredService<ILocalStorageService>()) { InnerHandler = new HttpClientHandler() })
{
  BaseAddress = new Uri(apiUrl)
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState(); // rende lo stato di login disponibile in tutti i componenti
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RichiesteService>();
builder.Services.AddRadzenComponents();


await builder.Build().RunAsync();
