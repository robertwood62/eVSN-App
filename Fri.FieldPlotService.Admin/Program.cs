using Fri.FieldPlotService.Admin;
using Fri.FieldPlotService.Admin.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Determine the environment settings from the Browser URL
string environment = EnvironmentDetector.Detect(builder.Configuration, builder.HostEnvironment.BaseAddress);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddLocalization();
builder.Services.AddScoped(sp =>
{
    var authorizationMessageHandler = sp.GetRequiredService<AuthorizationMessageHandler>();
    authorizationMessageHandler.InnerHandler = new HttpClientHandler();
    authorizationMessageHandler = authorizationMessageHandler.ConfigureHandler(
        authorizedUrls: new[] { builder.Configuration[$"{environment}:FieldPlotServiceApi:BaseUrl"] },
        scopes: new[] { builder.Configuration[$"{environment}:FieldPlotServiceApi:Scopes"] });
    return new HttpClient(authorizationMessageHandler)
    {
        BaseAddress = new Uri(builder.Configuration[$"{environment}:FieldPlotServiceApi:BaseUrl"] ?? string.Empty)
    };
});

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind($"{environment}:AzureAd", options.ProviderOptions.Authentication);
});
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind($"{environment}:AzureAd", options.ProviderOptions.Authentication);
    var scopes = builder.Configuration[$"{environment}:FieldPlotServiceApi:Scopes"];
    if (!string.IsNullOrWhiteSpace(scopes))
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scopes);
    }
});

builder.Services.AddMudServices();

builder.Services.AddScoped(typeof(AdminContextOptions), (options) => new AdminContextOptions(builder.Configuration, environment));
builder.Services.AddScoped(typeof(AdminContext));
builder.Services.AddScoped(typeof(DownloadHelper));


await builder.Build().RunAsync();
