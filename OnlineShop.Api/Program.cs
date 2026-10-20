using OnlineShop.Api.Data;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Services;
using OnlineShop.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlite(connectionString));
builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth", option =>
{
    option.Cookie.Name = "OnlineShop.Auth";
    option.Cookie.HttpOnly = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app =  builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();


app.MapGet("/api/v1/products", async (IProductService productService, CancellationToken ct) =>
{
    var products = await productService.GetAllAsync(ct);
    return Results.Ok(products);
});
app.MapPost("/api/v1/auth/register", async ([FromBody] RegisterRequestDto request, IAuthService authService, CancellationToken ct) =>
{
    try
    {
        await authService.RegisterAsync(request, ct);
        return Results.Ok(new { Message = "User registered Successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});
app.MapPost("/api/v1/auth/login", async ([FromBody] LoginRequestDto request, IAuthService AuthService,
    HttpContext httpContext, CancellationToken ct) =>
{
    try
    {
        var user = await AuthService.LoginAsync(request, ct);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await httpContext.SignInAsync("CookieAuth", claimsPrincipal);
        return Results.Ok(new { Message = "Login Successfully" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

app.Run();