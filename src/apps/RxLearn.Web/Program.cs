using MudBlazor.Services;
using RxLearn.Web.Components;
using RxLearn.Web.Application.State;
using RxLearn.Web.Infrastructure.Services;
using RxLearn.Web.Infrastructure.State;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

// Reactive services and state
builder.Services.AddSingleton<IDemoClockService, DemoClockService>();
builder.Services.AddSingleton<ISubjectHub, SubjectHub>();
builder.Services.AddSingleton(typeof(IAppState<>), typeof(BehaviorAppState<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();
