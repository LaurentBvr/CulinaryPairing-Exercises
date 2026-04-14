using CulinaryPairing.Application.Features.Dishes;
using CulinaryPairing.Application.Features.Pairings;
using CulinaryPairing.Infrastructure;
using CulinaryPairing.Infrastructure.Database;
using CulinaryPairing.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<GetDishesHandler>();
builder.Services.AddScoped<GetDishByIdHandler>();
builder.Services.AddScoped<CreateDishHandler>();
builder.Services.AddScoped<GetDishPairingsHandler>();
builder.Services.AddScoped<CreatePairingHandler>();
builder.Services.AddScoped<ValidatePairingHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();