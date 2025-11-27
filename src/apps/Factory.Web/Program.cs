using MudBlazor.Services;
using Factory.Web.Components;
using Factory.Simulation.Engine;
using Factory.Infrastructure.Repositories;
using Factory.Domain.Entities;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Factory.Web.Jincheng;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<SimulationEngine>();
builder.Services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();
builder.Services.AddSingleton<IBlueprintRepository, InMemoryBlueprintRepository>();
builder.Services.AddSingleton<JinchengDataStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();

// Serve external static assets for big-data views
var bigDataPath = Path.Combine(app.Environment.ContentRootPath, "..", "..", "SourceCode", "big-data-view", "web");
bigDataPath = Path.GetFullPath(bigDataPath);
if (Directory.Exists(bigDataPath)) {
  app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(bigDataPath),
    RequestPath = "/big-data"
  });
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/api/sim/metrics", (SimulationEngine eng) => Results.Json(new {
  energyKWh = eng.TotalEnergyKWh,
  inventory = eng.Inventory,
  produced = eng.Produced
}));

app.MapGet("/api/items", (ICatalogRepository repo) => Results.Json(repo.GetItems()));
app.MapGet("/api/recipes", (ICatalogRepository repo) => Results.Json(repo.GetRecipes()));

app.MapPost("/api/blueprints", (IBlueprintRepository repo, Blueprint bp) => {
  repo.Save(bp);
  return Results.Created($"/api/blueprints/{bp.Id}", bp);
});

app.MapGet("/api/blueprints/{id}", (IBlueprintRepository repo, string id) => {
  var bp = repo.Get(id);
  return bp is null ? Results.NotFound() : Results.Json(bp);
});

app.MapPost("/api/factories/{id}/deploy", (IBlueprintRepository repo, string id, string blueprintId, Dictionary<string, object>? parameters) => {
  var bp = repo.Get(blueprintId);
  if (bp is null) return Results.NotFound();
  var deployed = new { factoryId = id, blueprintId, parameters = parameters ?? bp.Parameters };
  return Results.Ok(deployed);
});

app.MapGet("/api/jincheng/dashboard", (JinchengDataStore store) => Results.Json(store.Get()));
app.MapPost("/api/jincheng/dashboard", (JinchengDataStore store, JinchengDashboardData data) => {
  store.Set(data);
  return Results.Ok();
}).DisableAntiforgery();

app.Run();
