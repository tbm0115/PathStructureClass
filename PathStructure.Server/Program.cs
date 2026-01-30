using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PathStructure.Server.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ServerConfigStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions
{
    RequestPath = "/admin"
});
app.UseStaticFiles();
app.MapGet("/", () => Results.Redirect("/admin/"));
app.MapControllers();

app.Run();
