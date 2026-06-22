using EscolhaAI.Client.Components;


using EscolhaAI.Client.Services;

using EscolhaAI.Services;
using EscolhaAI.Models;




var builder = WebApplication.CreateBuilder(args);






// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();          // <- adiciona suporte a controllers





// You can also access the environment type directly
IWebHostEnvironment environment = builder.Environment;


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}





app.UseHttpsRedirection();


app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddInteractiveWebAssemblyRenderMode();

app.MapControllers();  // <- mapeia os controllers













app.Run();


