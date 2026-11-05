using ApiContracts.Auth;
using Blazor_Application.Auth;
using Blazor_Application.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Blazor_Application.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string webApiBaseAddress = builder.Configuration["WebApiBaseAddress"] ?? "http://localhost:5028/";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(webApiBaseAddress)
});
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();
builder.Services.AddScoped<IUserService, HttpUserService>();
builder.Services.AddScoped<IPostService, HttpPostService>();
builder.Services.AddScoped<ICommentService, HttpCommentService>();
builder.Services.AddScoped<ISubForumService, HttpSubForumService>();

AuthorizationPolicies.AddPolicies(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();
