using ParcelBox.Web;
using ParcelBox.Web.Components;

var currentDirectory = Directory.GetCurrentDirectory();
var projectDirectory = Directory.Exists(Path.Combine(currentDirectory, "wwwroot"))
    ? currentDirectory
    : Path.Combine(currentDirectory, "src", "ParcelBox.Web");

var contentRoot = Directory.Exists(Path.Combine(projectDirectory, "wwwroot"))
    ? projectDirectory
    : currentDirectory;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot
});

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddWebClients(builder.Configuration);

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
