using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PathStructureServer.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ServerConfigStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapRazorPages();

app.MapFallbackToPage("/_Host");

app.Run();
